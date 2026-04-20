// round.js - Background polling for students to detect round changes
// CONSERVATIVE approach: Only reload when the round number ACTUALLY changes

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    console.log('Educator detected - no round polling');
} else {
    let gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');
    let reloadPending = false;
    // Initialize tracked round number from the page so we don't reload immediately
    const roundNumberElement = document.querySelector('[data-round-number]');
    const pageRoundNumber = roundNumberElement ? parseInt(roundNumberElement.getAttribute('data-round-number')) : null;
    let currentRoundNumber = pageRoundNumber;
    let pollAttempts = 0;
    const MAX_CONSECUTIVE_FAILURES = 3;
    let consecutiveFailures = 0;
    // Require two consecutive polls that indicate a new round before reloading (debounce)
    let consecutiveNewRoundCount = 0;
    const REQUIRED_CONSECUTIVE_NEWROUND = 2;

    async function checkRoundStatus() {
        // Respect transient UI and external flag
        if (window.pollActive) return;

        // Do not poll while the game changer overlay is visible
        if (document.getElementById('gc-overlay')) return;

        // Do not poll while the submission feedback is visible
        if (document.getElementById('rd-submission-feedback')) return;

        // Do not trigger multiple reloads simultaneously
        if (reloadPending) return;

        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            
            if (!response.ok) {
                consecutiveFailures++;
                if (consecutiveFailures >= MAX_CONSECUTIVE_FAILURES) {
                    console.warn('Too many failed poll attempts, stopping polling');
                    clearInterval(pollInterval);
                }
                return;
            }

            const data = await response.json();
            consecutiveFailures = 0; // Reset on successful response

            // Game completed - go to results
            if (data.gameCompleted) {
                reloadPending = true;
                console.log('Game completed, navigating to results');
                window.location.href = '/Results';
                return;
            }

            // Update tracked round number on first successful poll if null
            if (currentRoundNumber === null && pageRoundNumber !== null) {
                currentRoundNumber = pageRoundNumber;
                console.log(`Polling started for round ${currentRoundNumber}`);
            }

            // Debounced new-round detection:
            // increment consecutive counter when server says a new round is open; only reload after REQUIRED_CONSECUTIVE_NEWROUND polls
            if (data.newRoundOpen) {
                consecutiveNewRoundCount++;
            } else {
                consecutiveNewRoundCount = 0;
            }

            if (consecutiveNewRoundCount >= REQUIRED_CONSECUTIVE_NEWROUND) {
                // Confirm pageRoundNumber is different (we only want to reload when page's round differs)
                const pageElem = document.querySelector('[data-round-number]');
                const pageNum = pageElem ? parseInt(pageElem.getAttribute('data-round-number')) : null;
                if (pageNum !== null && pageNum !== currentRoundNumber) {
                    console.log(`New round detected (debounced): ${pageNum} -> reloading`);
                    reloadPending = true;
                    window.location.reload();
                    return;
                } else {
                    // If pageNum is the same, it's likely server-side noise; reset counter
                    consecutiveNewRoundCount = 0;
                }
            }

        } catch (err) {
            consecutiveFailures++;
            console.error('Error checking round status:', err);
            if (consecutiveFailures >= MAX_CONSECUTIVE_FAILURES) {
                console.warn('Too many polling errors, stopping polling');
                clearInterval(pollInterval);
            }
        }
    }

    // Poll every 5 seconds (increased from 3 to reduce flashing)
    const pollInterval = setInterval(checkRoundStatus, 5000);
    
    // Check immediately on page load
    checkRoundStatus();
}

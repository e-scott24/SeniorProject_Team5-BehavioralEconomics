// round.js - Background polling for students to detect round changes

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    console.log('Educator detected - no round polling');
} else {
    let gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');
    let hasSubmitted = false;
    let lastCheckTime = Date.now();

    // Check if we're on the "waiting" screen
    function isOnWaitingScreen() {
        return document.body.textContent.includes('Waiting for the educator') ||
            document.body.textContent.includes('Choice submitted');
    }

    async function checkRoundStatus() {
        // Do not poll while the game changer overlay is visible
        if (document.getElementById('gc-overlay')) return;

        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            const data = await response.json();

            // Game completed - go to results
            if (data.gameCompleted) {
                console.log('Game completed, redirecting to results...');
                window.location.href = '/Results';
                return;
            }

            // Track if we submitted
            if (data.playerSubmitted) {
                hasSubmitted = true;
            }

            // If we're waiting and a new round opened, reload ONCE
            if (isOnWaitingScreen() && data.newRoundOpen && !data.currentRoundClosed) {
                // Only reload if we haven't reloaded in the last 3 seconds (prevent reload loops)
                const now = Date.now();
                if (now - lastCheckTime > 3000) {
                    console.log('New round detected, reloading...');
                    lastCheckTime = now;
                    window.location.reload();
                }
            }

        } catch (err) {
            console.error('Error checking round status:', err);
        }
    }

    // Poll every 3 seconds (not every 1 second - too aggressive)
    setInterval(checkRoundStatus, 3000);
    checkRoundStatus();
}
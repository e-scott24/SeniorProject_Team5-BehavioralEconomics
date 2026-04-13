// round.js - Background polling for students to detect round changes

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    console.log('Educator detected - no round polling');
} else {
    let gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');

    // Track the current round ID from the page
    let currentRoundId = document.querySelector('[data-round-id]')?.getAttribute('data-round-id');

    let isReloading = false; // Prevent multiple simultaneous reloads

    // Check if we're on the "waiting" screen
    function isOnWaitingScreen() {
        return document.body.textContent.includes('Waiting for the educator') ||
            document.body.textContent.includes('Choice submitted');
    }

    async function checkRoundStatus() {
        // Prevent overlapping checks
        if (isReloading) return;

        // Do not poll while the game changer overlay is visible
        if (document.getElementById('gc-overlay')) return;

        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            const data = await response.json();

            // Game completed - go to results
            if (data.gameCompleted) {
                console.log('Game completed, redirecting to results...');
                isReloading = true;
                window.location.href = '/Results';
                return;
            }

            // Enhanced round detection: Check if the open round ID is different from current
            if (data.openRoundId && currentRoundId && data.openRoundId.toString() !== currentRoundId) {
                console.log(`Round changed from ${currentRoundId} to ${data.openRoundId}, reloading...`);
                isReloading = true;
                window.location.reload();
                return;
            }

            // If we're on the waiting screen and there's an open round
            // (this handles case where currentRoundId is null/empty)
            if (isOnWaitingScreen() && data.newRoundOpen) {
                console.log('New round detected while waiting, reloading...');
                isReloading = true;
                window.location.reload();
                return;
            }

            // If we're NOT on waiting screen (we're looking at cards)
            // but the current round closed, that means educator closed it
            if (!isOnWaitingScreen() && data.currentRoundClosed) {
                console.log('Current round was closed by educator, reloading...');
                isReloading = true;
                window.location.reload();
                return;
            }

        } catch (err) {
            console.error('Error checking round status:', err);
        }
    }

    // Poll every 2 seconds for responsive updates
    setInterval(checkRoundStatus, 2000);
    checkRoundStatus(); // Run immediately on page load
}
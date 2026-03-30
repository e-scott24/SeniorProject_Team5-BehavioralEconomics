// round.js - Polling for students to detect round changes and game completion

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    // Educators don't poll on the Round page - they stay in Lobby
    console.log('Educator detected - no round polling');
} else {
    // Students poll to detect:
    // 1. Round closure (redirect to next round)
    // 2. Game completion (redirect to Results)

    let currentRoundId = document.querySelector('[data-round-id]')?.getAttribute('data-round-id');
    let gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');

    async function checkRoundStatus() {
        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            const data = await response.json();

            // Game completed - go to results
            if (data.gameCompleted) {
                window.location.href = '/Results';
                return;
            }

            // Current round closed and new round opened - reload to show new round
            if (data.currentRoundClosed && data.newRoundOpen) {
                window.location.reload();
                return;
            }

            // Submitted and waiting - show waiting message if not already shown
            if (data.playerSubmitted && !data.currentRoundClosed) {
                // Player has submitted, just waiting for educator to advance
                console.log('Waiting for educator to advance round...');
            }

        } catch (err) {
            console.error('Error checking round status:', err);
        }
    }

    // Poll every 2 seconds
    setInterval(checkRoundStatus, 2000);
    checkRoundStatus();
}
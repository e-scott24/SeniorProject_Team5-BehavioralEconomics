// round.js - Background polling: students detect when a new round opens after submission

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    console.log('[round.js] Educator view — no polling needed');
} else {
    const gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');
    let reloadPending = false;

    // If the page rendered in waiting state (player already submitted this round),
    // set this flag immediately so we start watching for the next round right away.
    // The waiting state renders a div with id="rd-waiting-submitted".
    let playerHasSubmitted = document.getElementById('rd-waiting-submitted') !== null;

    async function checkRoundStatus() {
        // Do not poll while the game changer overlay is visible
        if (document.getElementById('gc-overlay')) return;

        // Do not poll while the Kahoot submission feedback flash is visible
        if (document.getElementById('rd-submission-feedback')) return;

        // Prevent stacking reload calls
        if (reloadPending) return;

        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            if (!response.ok) return;

            const data = await response.json();

            // Game is over — navigate to results
            if (data.gameCompleted) {
                reloadPending = true;
                window.location.href = '/Results';
                return;
            }

            // Track submission state: once true it stays true for this page load
            if (data.playerSubmitted) {
                playerHasSubmitted = true;
            }

            // Reload condition:
            // The player submitted the previous round (playerHasSubmitted = true)
            // AND a round is now open (newRoundOpen = true)
            // AND they haven't submitted for that new round yet (!data.playerSubmitted)
            // This fires exactly once: when the educator advances to the next round
            if (playerHasSubmitted && data.newRoundOpen && !data.playerSubmitted) {
                reloadPending = true;
                window.location.reload();
            }

        } catch (err) {
            console.error('[round.js] Poll error:', err);
        }
    }

    setInterval(checkRoundStatus, 3000);
    checkRoundStatus();
}

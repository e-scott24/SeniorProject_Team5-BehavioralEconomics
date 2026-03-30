// round.js - Polling for students to detect round changes and game completion

const isStudent = document.querySelector('[data-role="student"]') !== null;

if (!isStudent) {
    console.log('Educator detected - no round polling');
} else {
    let gameSessionId = document.querySelector('[data-session-id]')?.getAttribute('data-session-id');
    let lastRoundNumber = document.querySelector('[data-round-id]') ?
        parseInt(document.querySelector('h2')?.textContent.match(/ROUND (\d+)/)?.[1] || '0') : 0;

    async function checkRoundStatus() {
        try {
            const response = await fetch(`/Round?handler=CheckRoundStatus&gameSessionId=${gameSessionId}`);
            const data = await response.json();

            // Game completed - go to results
            if (data.gameCompleted) {
                window.location.href = '/Results';
                return;
            }

            // If we're on a waiting screen and a new round opened, reload
            const isWaiting = document.body.textContent.includes('Waiting for the educator');
            if (isWaiting && data.newRoundOpen && !data.currentRoundClosed) {
                console.log('New round opened, reloading...');
                window.location.reload();
                return;
            }

        } catch (err) {
            console.error('Error checking round status:', err);
        }
    }

    // Poll every 2 seconds
    setInterval(checkRoundStatus, 2000);
    checkRoundStatus();
}
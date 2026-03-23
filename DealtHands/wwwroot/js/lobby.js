// Fetch and update player list
async function updatePlayers() {
    const sessionCode = document.getElementById('sessionCode').value;

    try {
        // Check if game started
        const checkResponse = await fetch(`/Lobby?handler=CheckGameStarted&sessionCode=${sessionCode}`);
        const started = await checkResponse.json();

        if (started) {
            const urlParams = new URLSearchParams(window.location.search);
            const playerId = urlParams.get('playerId');

            // Only redirect students (those with playerId)
            if (playerId) {
                window.location.href = `/Round1Career?playerId=${playerId}`;
            }
            // Educator stays on lobby page
            return;
        }

        // Update player list
        const response = await fetch(`/Lobby?handler=GetPlayers&sessionCode=${sessionCode}`);
        const players = await response.json();

        // Update player count
        document.getElementById('playerCount').textContent = players.length;

        // Update player cards
        const playerContainer = document.getElementById('playerContainer');
        playerContainer.innerHTML = '';

        players.forEach(player => {
            const col = document.createElement('div');
            col.className = 'col-md-2 mb-3';
            col.innerHTML = `
                <div class="card">
                    <div class="card-body text-center py-2">
                        <p class="mb-0 fw-bold">${player.name}</p>
                    </div>
                </div>
            `;
            playerContainer.appendChild(col);
        });

    } catch (error) {
        console.error('Error fetching players:', error);
    }
}

// Poll every 2 seconds
setInterval(updatePlayers, 2000);

// Initial load
updatePlayers();
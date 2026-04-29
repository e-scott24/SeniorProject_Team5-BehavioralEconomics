const sessionCode = document.getElementById('sessionCode').value;
const isEducator = document.getElementById('isEducator').value === 'true';

// ─── EDUCATOR ONLY: poll round status and update control panel ────────────────

async function updateRoundStatus() {
    if (!isEducator) return;

    try {
        const response = await fetch(`/Lobby?handler=RoundStatus&sessionCode=${sessionCode}`);
        const status = await response.json();

        const roundLabel = document.getElementById('roundLabel');
        const submissionStatus = document.getElementById('submissionStatus');
        const bar = document.getElementById('submissionProgress');
        const btn = document.getElementById('closeRoundBtn');

        if (!status || !status.roundOpen) {
            if (roundLabel) roundLabel.textContent = 'No round currently open.';
            if (submissionStatus) submissionStatus.textContent = '';
            if (bar) { bar.style.width = '0%'; bar.textContent = '0%'; }
            if (btn) btn.disabled = true;
            return;
        }

        if (roundLabel)
            roundLabel.textContent = `Round ${status.roundNumber}: ${status.roundType}`;

        if (submissionStatus)
            submissionStatus.textContent = `${status.submitted} / ${status.total} players submitted`;

        if (bar) {
            const pct = status.total > 0
                ? Math.round((status.submitted / status.total) * 100)
                : 0;
            bar.style.width = `${pct}%`;
            bar.textContent = `${pct}%`;
        }

        if (btn) {
            btn.dataset.allSubmitted = status.allSubmitted;
            btn.dataset.submitted = status.submitted;
            btn.dataset.total = status.total;
        }

    } catch (err) {
        console.error('Error fetching round status:', err);
    }
}

// ─── SHARED: update the player list display ───────────────────────────────────

async function updatePlayers() {
    try {
        const response = await fetch(`/Lobby?handler=GetPlayers&sessionCode=${sessionCode}`);
        const players = await response.json();

        const countEl = document.getElementById('playerCount');
        if (countEl) countEl.textContent = players.length;

        const container = document.getElementById('playerContainer');
        if (container) {
            container.innerHTML = '';
            players.forEach(player => {
                const col = document.createElement('div');
                col.className = 'col-md-2 mb-3';
                const codeLine = isEducator
                    ? `<small class="text-muted">Code: ${player.id}</small>`
                    : '';
                col.innerHTML = `
                    <div class="card">
                        <div class="card-body text-center py-2">
                            <p class="mb-0 fw-bold">${player.name}</p>
                            ${codeLine}
                        </div>
                    </div>`;
                container.appendChild(col);
            });
        }
    } catch (err) {
        console.error('Error fetching players:', err);
    }
}

// ─── STUDENT ONLY: check if game started or session changed state ─────────────

async function checkSessionStatus() {
    if (isEducator) return; // Educators never get redirected from this page

    try {
        const startedRes = await fetch(`/Lobby?handler=CheckGameStarted&sessionCode=${sessionCode}`);
        const started = await startedRes.json();

        if (started) {
            window.location.href = '/Round';
            return;
        }

        const statusRes = await fetch(`/Lobby?handler=CheckSessionStatus&sessionCode=${sessionCode}`);
        const { status, isActive } = await statusRes.json();

        if (!isActive || status === 'Completed') {
            alert('This session has been cancelled by the educator.');
            window.location.href = '/';
            return;
        }

        if (status === 'Paused') {
            alert('The session has been paused. Use your Player Code to rejoin when it resumes.');
            window.location.href = '/JoinSession';
            return;
        }

    } catch (err) {
        console.error('Error checking session status:', err);
    }
}

function confirmCloseRound(btn) {
    if (btn.dataset.allSubmitted === 'true') return true;
    const submitted = btn.dataset.submitted ?? '?';
    const total = btn.dataset.total ?? '?';
    return confirm(`Only ${submitted} of ${total} players have submitted. Close the round anyway?`);
}

// ─── POLL LOOP ────────────────────────────────────────────────────────────────

async function poll() {
    await updatePlayers();
    if (isEducator) {
        await updateRoundStatus();
    } else {
        await checkSessionStatus();
    }
}

setInterval(poll, 2000);
poll();

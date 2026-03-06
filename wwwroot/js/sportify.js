// ── Live polling ──────────────────────────────────────────────────────────────
const POLL_INTERVAL = 60000; // 60 seconds

function updateTimestamp() {
    const el = document.getElementById('last-updated-time');
    if (el) {
        const now = new Date();
        el.textContent = now.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    }
}

// Update live matches section if it exists on the page
async function pollLiveMatches() {
    const container = document.getElementById('live-matches-container');
    if (!container) return;

    try {
        const res = await fetch('/api/sport/live');
        if (!res.ok) return;
        const json = await res.json();
        const matches = json.data;

        if (matches.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">🎽</div>
                    <p>Şu an canlı maç bulunmuyor</p>
                </div>`;
            return;
        }

        container.innerHTML = matches.map(m => buildMatchCard(m)).join('');
        updateTimestamp();
    } catch (e) {
        console.warn('Live poll failed:', e);
    }
}

// Update today matches section
async function pollTodayMatches() {
    const container = document.getElementById('today-matches-container');
    if (!container) return;

    try {
        const res = await fetch('/api/sport/today');
        if (!res.ok) return;
        const json = await res.json();
        const matches = json.data;

        if (matches.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">📅</div>
                    <p>Bugün için maç bulunmuyor</p>
                </div>`;
            return;
        }

        container.innerHTML = matches.slice(0, 10).map(m => buildMatchCard(m)).join('');
        updateTimestamp();
    } catch (e) {
        console.warn('Today poll failed:', e);
    }
}

function buildMatchCard(m) {
    const isLive = m.status === '1H' || m.status === '2H' || m.status === 'HT' || m.status === 'ET';
    const isFT = m.status === 'FT' || m.status === 'AET' || m.status === 'PEN';
    const isNS = m.status === 'NS' || m.status === 'TBD';

    const statusLabel = isLive ? `<span class="match-status-badge live">🔴 CANLI</span>`
        : isFT ? `<span class="match-status-badge ft">Bitti</span>`
        : `<span class="match-status-badge ns">${formatMatchTime(m.date)}</span>`;

    const scoreOrTime = isNS
        ? `<div class="score-num" style="font-size:14px;color:var(--text-secondary)">${formatMatchTime(m.date)}</div>`
        : `<div class="score-num">${m.homeScore ?? '-'}</div>
           <div class="score-sep">:</div>
           <div class="score-num">${m.awayScore ?? '-'}</div>`;

    const minute = isLive && m.minute ? `<div class="match-minute">⏱ ${m.minute}'</div>` : '';

    return `
    <div class="match-card ${isLive ? 'live' : ''} fade-in">
        <div class="match-league">
            ${m.leagueLogo ? `<img src="${m.leagueLogo}" alt="" onerror="this.style.display='none'">` : ''}
            ${m.leagueName}
            ${statusLabel}
        </div>
        <div class="match-teams">
            <div class="match-team">
                <img src="${m.homeTeamLogo}" alt="${m.homeTeam}" onerror="this.src='/images/team-placeholder.png'">
                <div class="match-team-name">${m.homeTeam}</div>
            </div>
            <div class="match-score">${scoreOrTime}</div>
            <div class="match-team">
                <img src="${m.awayTeamLogo}" alt="${m.awayTeam}" onerror="this.src='/images/team-placeholder.png'">
                <div class="match-team-name">${m.awayTeam}</div>
            </div>
        </div>
        ${minute}
    </div>`;
}

function formatMatchTime(dateStr) {
    if (!dateStr) return '';
    try {
        const d = new Date(dateStr);
        return d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    } catch { return ''; }
}

// ── Init ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    updateTimestamp();

    // Start polling if containers exist
    if (document.getElementById('live-matches-container')) {
        setInterval(pollLiveMatches, POLL_INTERVAL);
    }
    if (document.getElementById('today-matches-container')) {
        setInterval(pollTodayMatches, POLL_INTERVAL);
    }

    // Update clock every second
    setInterval(updateTimestamp, 1000);

    // Animate numbers
    document.querySelectorAll('[data-count]').forEach(el => {
        const target = parseInt(el.dataset.count);
        let current = 0;
        const step = Math.ceil(target / 40);
        const timer = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = current;
            if (current >= target) clearInterval(timer);
        }, 30);
    });
});

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameRoomHub")
    .build();

const roomCode = window.roomConfig.roomCode;
let currentRoom = null;

connection.on("OnError", function (errorMessage) {
    showErrorToast("Ошибка сессии", errorMessage);
});

connection.on("GameStarted", function () {
    setTimeout(() => {
        if (currentRoom && currentRoom.gameName === "Tank-A-Catch") {
            window.location.href = "/Games/Bounce/" + roomCode;
        }
    }, 2000);
});

connection.on("UpdateRoom", function (room) {
    currentRoom = room;

    const countEl = document.getElementById("player-count");
    if (countEl) countEl.textContent = room.players.length;

    const gameNameEl = document.getElementById("current-game-name");
    if (gameNameEl) gameNameEl.textContent = room.gameName;

    function countPlayersInText(list) {
        function isConsecutive(list) {
            for (let i = 1; i < list.length; i++) {
                if (list[i] !== (list[i - 1] + 1)) return false;
            }
            return true;
        }

        const sorted = [...list].sort((a, b) => a - b);
        let numbers_text = '';
        if (sorted.length === 1) {
            numbers_text = sorted[0];
        } else if (sorted.length === 2) {
            numbers_text = sorted[0] + ' или ' + sorted[1];
        } else {
            numbers_text = isConsecutive(sorted) ? sorted.join(',') : sorted[0] + '-' + sorted[sorted.length - 1];
        }

        return 'для ' + numbers_text + ' игроков';
    }

    const countPlayersText = document.getElementById("player-count-text");
    if (countPlayersText) countPlayersText.textContent = countPlayersInText(room.playersCounts);

    const selector = document.getElementById("game-selector");
    if (selector && parseInt(selector.value) !== room.gameId) {
        selector.value = room.gameId;
    }

    // --- БЕЗОПАСНЫЙ ПОИСК ТЕКУЩЕГО ИГРОКА (УЧЕТ CASING) ---
    const me = room.players.find(p => {
        const pId = p.id ?? p.Id;
        const pName = p.name ?? p.Name;
        const curId = window.roomConfig.currentUserId;
        const curName = window.roomConfig.currentUserName;

        return (curId && pId && pId.toString().toLowerCase() === curId.toLowerCase()) ||
            (curName && pName === curName);
    });

    const readyBtn = document.getElementById("btn-toggle-ready");
    const readyBtnText = document.getElementById("ready-btn-text");

    if (readyBtn && me) {
        const isReady = me.isReady ?? me.IsReady ?? false;
        if (isReady) {
            readyBtn.classList.remove("white");
            readyBtn.classList.add("green");
            if (readyBtnText) readyBtnText.textContent = "ГОТОВ!";
        } else {
            readyBtn.classList.remove("green");
            readyBtn.classList.add("white");
            if (readyBtnText) readyBtnText.textContent = "ГОТОВ";
        }
    }

    const playerList = document.getElementById("player-list");
    if (playerList) {
        playerList.innerHTML = "";
        room.players.forEach(player => {
            const li = document.createElement("li");
            li.className = "retro-list-item";

            const pIsHost = player.isHost ?? player.IsHost;
            const pIsReady = player.isReady ?? player.IsReady;
            const pIsGuest = player.isGuest ?? player.IsGuest;
            const pName = player.name ?? player.Name;

            let statusText = pIsHost ? "[HOST]" : (pIsReady ? "[ГОТОВ]" : "[НЕ ГОТОВ]");
            let statusColorClass = (pIsReady || pIsHost) ? "status-ready" : "status-not-ready";
            let guestBadge = pIsGuest ? `<span class="guest-tag">[GUEST]</span>` : "";

            li.innerHTML = `
                <div class="player-info-side">
                    <span class="terminal-icon">🖥️</span>
                    <strong class="player-name-text">${pName}</strong>
                    ${guestBadge}
                </div>
                <div class="player-status-side">
                    <span class="ready-status-badge ${statusColorClass}">
                        ${statusText}
                    </span>
                </div>
            `;
            playerList.appendChild(li);
        });
    }
});

connection.on("RoomDelete", function () {
    setTimeout(() => {
        if (window.roomConfig.isGuest) {
            window.location.href = "/Hub/LogoutClean";
        } else {
            window.location.href = "/Hub/Index";
        }
    }, 2000);
});

connection.start().then(function () {
    connection.invoke("JoinRoom", roomCode);
}).catch(err => {
    console.error("SignalR Connection Error: ", err);
});

document.addEventListener("DOMContentLoaded", function () {
    const gameSelector = document.getElementById("game-selector");
    if (gameSelector) {
        gameSelector.addEventListener("change", function () {
            const gameId = parseInt(this.value);
            connection.invoke("ChangeGame", roomCode, gameId).catch(err => console.error(err));
        });
    }

    const startBtn = document.getElementById("btn-start-game");
    if (startBtn) {
        startBtn.addEventListener("click", function () {
            connection.invoke("StartGame", roomCode).catch(err => console.error(err));
        });
    }

    const readyBtn = document.getElementById("btn-toggle-ready");
    if (readyBtn) {
        readyBtn.addEventListener("click", function () {
            connection.invoke("ToggleReady", roomCode).catch(err => console.error(err));
        });
    }
});

function showErrorToast(title, description, duration = 5000) {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container-fixed';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = 'toast-item toast-animate-in';
    toast.innerHTML = `
        <div class="error-alert-toast">
            <div style="display: flex; align-items: center;">
                <div class="error-icon-box">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 3.75h.008v.008H12v-.008Z"></path>
                    </svg>
                </div>
                <div class="error-body">
                    <p class="error-title">${title}</p>
                    <p class="error-desc">${description}</p>
                </div>
            </div>
            <button type="button" class="toast-close-btn">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12"></path>
                </svg>
            </button>
        </div>
    `;

    const closeBtn = toast.querySelector('.toast-close-btn');
    const removeToast = () => {
        toast.classList.remove('toast-animate-in');
        toast.classList.add('toast-animate-out');
        setTimeout(() => toast.remove(), 250);
    };

    closeBtn.addEventListener('click', removeToast);

    if (duration > 0) {
        setTimeout(removeToast, duration);
    }

    container.appendChild(toast);
}
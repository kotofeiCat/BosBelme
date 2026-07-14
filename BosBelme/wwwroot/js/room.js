const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameRoomHub")
    .build();

const roomCode = window.roomConfig.roomCode;
let lastLoggedGameId = null;

function logToTerminal(message, type = "info") {
    const consoleEl = document.getElementById("room-console");
    const promptEl = document.getElementById("console-prompt");
    if (!consoleEl) return;

    const time = new Date().toLocaleTimeString();
    const logRow = document.createElement("div");
    logRow.style.marginBottom = "4px";

    if (type === "error") {
        logRow.style.color = "#ff3333";
        logRow.textContent = `[CRITICAL ${time}] >> ${message}`;
    } else if (type === "success") {
        logRow.style.color = "#00ff55";
        logRow.textContent = `[SUCCESS ${time}] >> ${message}`;
    } else if (type === "warn") {
        logRow.style.color = "#ffff00";
        logRow.textContent = `[WARNING ${time}] >> ${message}`;
    } else {
        logRow.style.color = "#33ff33";
        logRow.textContent = `[LOG ${time}] >> ${message}`;
    }

    if (promptEl) {
        consoleEl.insertBefore(logRow, promptEl);
    } else {
        consoleEl.appendChild(logRow);
    }

    consoleEl.scrollTop = consoleEl.scrollHeight;
}

connection.on("OnError", function (errorMessage) {
    logToTerminal(errorMessage, "error");
});

connection.on("GameStarted", function () {
    logToTerminal("Пакет запуска принят. Развертывание игрового интерфейса...", "success");
    const promptEl = document.getElementById("console-prompt");
    if (promptEl) promptEl.innerHTML = `[EXEC] Запуск процесса сессии...<span class="blink-cursor">_</span>`;
    setTimeout(() => {
        // window.location.href = "/Game/Play?code=" + roomCode; 
    }, 2000);
});

connection.on("UpdateRoom", function (room) {
    // Обновление счетчиков
    const countEl = document.getElementById("player-count");
    if (countEl) countEl.textContent = room.players.length;

    const gameNameEl = document.getElementById("current-game-name");
    if (gameNameEl) gameNameEl.textContent = room.gameName;

    const minPlEl = document.getElementById("min-players");
    const maxPlEl = document.getElementById("max-players");
    if (minPlEl) minPlEl.textContent = room.minPlayers;
    if (maxPlEl) maxPlEl.textContent = room.maxPlayers;

    // Синхронизация селектора хоста
    const selector = document.getElementById("game-selector");
    if (selector) {
        if (parseInt(selector.value) !== room.gameId) {
            selector.value = room.gameId;
        }
    }

    if (lastLoggedGameId !== room.gameId) {
        logToTerminal(`Загружена конфигурация подсистемы: [${room.gameName}]`);
        lastLoggedGameId = room.gameId;
    }

    // Обновление списка игроков по новой ретро-модели CSS
    const playerList = document.getElementById("player-list");
    if (playerList) {
        playerList.innerHTML = "";
        room.players.forEach(player => {
            const li = document.createElement("li");
            li.className = "retro-list-item";

            let statusText = player.isHost ? "[HOST]" : (player.isReady ? "[ГОТОВ]" : "[НЕ ГОТОВ]");
            let statusColorClass = (player.isReady || player.isHost) ? "status-ready" : "status-not-ready";
            let guestBadge = player.isGuest ? `<span class="guest-tag">[GUEST]</span>` : "";

            li.innerHTML = `
                <div class="player-info-side">
                    <span class="terminal-icon">🖥️</span>
                    <strong class="player-name-text">${player.name}</strong>
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

connection.start().then(function () {
    logToTerminal("Канал SignalR успешно синхронизирован с сервером.");
    connection.invoke("JoinRoom", roomCode);
}).catch(err => {
    logToTerminal("Ошибка рукопожатия сети: " + err.toString(), "error");
});

document.addEventListener("DOMContentLoaded", function () {
    const gameSelector = document.getElementById("game-selector");
    if (gameSelector) {
        gameSelector.addEventListener("change", function () {
            const gameId = parseInt(this.value);
            logToTerminal("Запрос на переключение конфигурации ПО...");
            connection.invoke("ChangeGame", roomCode, gameId)
                .catch(err => logToTerminal("Сбой отправки команды смены ПО", "error"));
        });
    }

    const startBtn = document.getElementById("btn-start-game");
    if (startBtn) {
        startBtn.addEventListener("click", function () {
            logToTerminal("Посылка мастер-пакета инициализации старта...");
            connection.invoke("StartGame", roomCode)
                .catch(err => logToTerminal("Ошибка запуска ядра игры", "error"));
        });
    }

    const readyBtn = document.getElementById("btn-toggle-ready");
    if (readyBtn) {
        readyBtn.addEventListener("click", function () {
            connection.invoke("ToggleReady", roomCode)
                .catch(err => logToTerminal("Сбой изменения флага готовности", "error"));
        });
    }
});
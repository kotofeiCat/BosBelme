const COLS = 20;
const ROWS = 15;
const BLOCK_SIZE = 40;

const canvas = document.getElementById("gameCanvas");
canvas.width = COLS * BLOCK_SIZE;
canvas.height = ROWS * BLOCK_SIZE;
const ctx = canvas.getContext("2d");

// Offscreen Canvas
const mapCanvas = document.createElement("canvas");
mapCanvas.width = canvas.width;
mapCanvas.height = canvas.height;
const mapCtx = mapCanvas.getContext("2d");
let isMapDirty = true;

const roomCode = window.gameRoomConfig.roomCode;
const currentUserName = window.gameRoomConfig.currentUserName;

let gameState = null;
let lastInputVector = { x: 0, y: 0 };
let myConnectionId = null;
let isGameEnded = false;

let prevPositions = { p1: null, p2: null };
let bodyAngles = { p1: 0, p2: Math.PI };

const keys = { KeyW: false, KeyA: false, KeyS: false, KeyD: false };

// Вспомогательная функция для надёжного определения локального игрока и соперника
function getLocalPlayerInfo() {
    if (!gameState) return { me: null, opponent: null, isP1: true };
    const p1 = gameState.player1 || gameState.Player1;
    const p2 = gameState.player2 || gameState.Player2;

    const p1Id = p1?.id || p1?.Id;
    const p2Id = p2?.id || p2?.Id;
    const p1Name = p1?.name || p1?.Name;
    const p2Name = p2?.name || p2?.Name;

    let isP1 = true;
    if (myConnectionId) {
        if (p1Id === myConnectionId) isP1 = true;
        else if (p2Id === myConnectionId) isP1 = false;
        else if (p1Name && p1Name === currentUserName && p1Name !== "Гость") isP1 = true;
        else if (p2Name && p2Name === currentUserName && p2Name !== "Гость") isP1 = false;
    } else if (currentUserName && currentUserName !== "Гость") {
        if (p1Name === currentUserName) isP1 = true;
        else if (p2Name === currentUserName) isP1 = false;
    }

    return {
        me: isP1 ? p1 : p2,
        opponent: isP1 ? p2 : p1,
        isP1: isP1
    };
}

// SignalR Подключение
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/bouncehub")
    .withAutomaticReconnect()
    .build();

window.addEventListener("beforeunload", () => {
    if (connection) {
        connection.stop();
    }
});

// Тихое подключение
connection.start().then(() => {
    myConnectionId = connection.connectionId;
    connection.invoke("JoinRoom", roomCode);
}).catch(err => {
    showErrorToast("Сбой рукопожатия", err.toString());
});

// Первичная инициализация состояния
connection.on("InitGame", (state) => {
    gameState = state;
    if (gameState) {
        gameState.player1 = state.player1 || state.Player1;
        gameState.player2 = state.player2 || state.Player2;
    }
    isMapDirty = true;
});

// Прием легких обновлений состояния
connection.on("UpdateState", (state) => {
    if (isGameEnded) return;

    if (!gameState) {
        gameState = state;
    } else {
        gameState.player1 = state.player1 || state.Player1;
        gameState.player2 = state.player2 || state.Player2;
        gameState.activeBullets = state.activeBullets || state.ActiveBullets;
        gameState.status = state.status !== undefined ? state.status : state.Status;
        gameState.statusTimer = state.statusTimer !== undefined ? state.statusTimer : state.StatusTimer;
        gameState.scores = gameState.scores || gameState.Scores;

        const newGrid = state.grid || state.Grid;
        if (newGrid) {
            if (!gameState.currentMap) gameState.currentMap = {};
            gameState.currentMap.grid = newGrid;
            gameState.currentMap.Grid = newGrid;
            isMapDirty = true;
        }
    }
    updateHUD();
});

connection.on("OnError", (err) => {
    showErrorToast("Ошибка игры", err);
});

connection.on("OpponentDisconnected", () => {
    isGameEnded = true;
    showInfoToast("Сосоединение разорвано", "Соперник покинул игру. Сессия завершена.");

    if (gameState) {
        gameState.status = 4;
        gameState.Status = 4;
        updateHUD();
    }

    setTimeout(() => {
        leaveGameAndRedirect();
    }, 1500);
});

connection.on("GameOver", (scores) => {
    isGameEnded = true;
    showInfoToast("Игра окончена", "МАТЧ ЗАВЕРШЕН! Финальный счет зафиксирован.");

    if (gameState) {
        gameState.status = 4;
        gameState.Status = 4;
        updateHUD();
    }

    setTimeout(() => {
        leaveGameAndRedirect();
    }, 4000);
});

function leaveGameAndRedirect() {
    const leaveForm = document.getElementById("mobile-leave-form");
    if (leaveForm && typeof leaveForm.submit === "function") {
        leaveForm.submit();
    } else {
        window.location.href = "/";
    }
}

// Обработка ввода (Клавиатура & Мышь)
window.addEventListener("keydown", (e) => {
    if (["KeyW", "KeyA", "KeyS", "KeyD"].includes(e.code)) {
        keys[e.code] = true;
        sendMovementIfNeeded();
    }
    if (e.code === "Space") { e.preventDefault(); triggerShield(); }
});

window.addEventListener("keyup", (e) => {
    if (["KeyW", "KeyA", "KeyS", "KeyD"].includes(e.code)) {
        keys[e.code] = false;
        sendMovementIfNeeded();
    }
});

window.addEventListener("mousedown", (e) => {
    if (e.button === 0 && gameState && !isGameEnded) {
        const rect = canvas.getBoundingClientRect();
        if (e.clientX >= rect.left && e.clientX <= rect.right &&
            e.clientY >= rect.top && e.clientY <= rect.bottom) {

            const { me: localPlayer } = getLocalPlayerInfo();

            if (localPlayer) {
                const isAlive = localPlayer.isAlive ?? localPlayer.IsAlive;
                const hasBullet = localPlayer.hasBullet ?? localPlayer.HasBullet;

                if (isAlive && hasBullet) {
                    // Учитываем возможные стили рамки Canvas для точного позиционирования
                    const style = window.getComputedStyle(canvas);
                    const borderLeft = parseFloat(style.borderLeftWidth) || 0;
                    const borderTop = parseFloat(style.borderTopWidth) || 0;

                    const visibleWidth = rect.width - borderLeft * 2;
                    const visibleHeight = rect.height - borderTop * 2;

                    const clickX = (e.clientX - rect.left - borderLeft) * (canvas.width / visibleWidth);
                    const clickY = (e.clientY - rect.top - borderTop) * (canvas.height / visibleHeight);

                    const pos = localPlayer.position || localPlayer.Position;
                    if (pos) {
                        const px = pos.x !== undefined ? pos.x : pos.X;
                        const py = pos.y !== undefined ? pos.y : pos.Y;

                        const angle = Math.atan2(clickY - py, clickX - px);

                        // Оптимистичное местное обновление угла пушки для плавности
                        localPlayer.rotationAngle = angle;
                        localPlayer.RotationAngle = angle;

                        connection.invoke("Shoot", roomCode, angle);
                    }
                }
            }
        }
    }
});

// Мобильное управление
function setupMobileControls() {
    const bindDir = (id, codeStr) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.addEventListener("touchstart", (e) => { e.preventDefault(); keys[codeStr] = true; sendMovementIfNeeded(); });
        el.addEventListener("touchend", (e) => { e.preventDefault(); keys[codeStr] = false; sendMovementIfNeeded(); });
    };
    bindDir("m-up", "KeyW"); bindDir("m-down", "KeyS"); bindDir("m-left", "KeyA"); bindDir("m-right", "KeyD");

    document.getElementById("m-shield")?.addEventListener("touchstart", (e) => { e.preventDefault(); triggerShield(); });
    document.getElementById("m-shoot")?.addEventListener("touchstart", (e) => {
        e.preventDefault();
        if (!gameState || isGameEnded) return;
        const { me: localPlayer } = getLocalPlayerInfo();

        if (localPlayer) {
            const isAlive = localPlayer.isAlive ?? localPlayer.IsAlive;
            const hasBullet = localPlayer.hasBullet ?? localPlayer.HasBullet;

            if (isAlive && hasBullet) {
                const rot = localPlayer.rotationAngle ?? localPlayer.RotationAngle ?? 0;
                connection.invoke("Shoot", roomCode, rot);
            }
        }
    });

    document.getElementById("m-exit")?.addEventListener("touchstart", (e) => {
        e.preventDefault();
        leaveGameAndRedirect();
    });
}

function triggerShield() {
    if (!isGameEnded) connection.invoke("ActivateShield", roomCode);
}

function sendMovementIfNeeded() {
    if (isGameEnded) return;
    let dx = 0; let dy = 0;
    if (keys["KeyW"]) dy -= 1;
    if (keys["KeyS"]) dy += 1;
    if (keys["KeyA"]) dx -= 1;
    if (keys["KeyD"]) dx += 1;

    if (dx !== lastInputVector.x || dy !== lastInputVector.y) {
        lastInputVector.x = dx; lastInputVector.y = dy;
        connection.invoke("Move", roomCode, dx, dy);
    }
}

// Расчет углов и обновление интерфейса
function updateBodyAngle(p, key) {
    if (!p) return;
    const pos = p.position || p.Position;
    if (!pos) return;
    const px = pos.x !== undefined ? pos.x : pos.X;
    const py = pos.y !== undefined ? pos.y : pos.Y;

    if (prevPositions[key]) {
        const dx = px - prevPositions[key].x;
        const dy = py - prevPositions[key].y;

        if (Math.abs(dx) > 0.1 || Math.abs(dy) > 0.1) {
            bodyAngles[key] = Math.atan2(dy, dx);
        }
    }
    prevPositions[key] = { x: px, y: py };
}

function updateHUD() {
    if (!gameState) return;
    const { me, opponent } = getLocalPlayerInfo();

    let scoreStr = "0 : 0";
    const scores = gameState.scores || gameState.Scores;
    if (scores && me && opponent) {
        const myId = me.id ?? me.Id;
        const oppId = opponent.id ?? opponent.Id;
        scoreStr = `${scores[myId] || 0} : ${scores[oppId] || 0}`;
        if (document.getElementById("score-text")) document.getElementById("score-text").textContent = scoreStr;
    }
    if (document.getElementById("mb-score")) document.getElementById("mb-score").textContent = scoreStr;

    const status = gameState.status !== undefined ? gameState.status : gameState.Status;
    const timer = gameState.statusTimer !== undefined ? gameState.statusTimer : gameState.StatusTimer;

    let st = "ОЖИДАНИЕ...";
    if (status === 0) st = "ОЖИДАНИЕ ИГРОКОВ";
    else if (status === 1) st = `СТАРТ: ${Math.ceil(timer)}С`;
    else if (status === 2) st = "LIVE";
    else if (status === 3) st = "РАУНД ОКОНЧЕН";
    else if (status === 4) st = "МАТЧ ОКОНЧЕН";

    if (document.getElementById("warmup-timer")) document.getElementById("warmup-timer").textContent = st;
    if (document.getElementById("mb-timer")) document.getElementById("mb-timer").textContent = st;

    const hasBullet = me && (me.hasBullet !== undefined ? me.hasBullet : me.HasBullet);
    if (document.getElementById("bullet-state")) {
        document.getElementById("bullet-state").className = hasBullet ? "bullet-indicator" : "bullet-indicator bullet-empty";
    }
    if (document.getElementById("mb-bullet")) {
        document.getElementById("mb-bullet").textContent = hasBullet ? "1/1" : "0/1";
        document.getElementById("mb-bullet").style.color = hasBullet ? "#00ff55" : "#ff3333";
    }
}

// Отрисовка Canvas
function renderMapToBuffer() {
    mapCtx.fillStyle = "#9bbc0f";
    mapCtx.fillRect(0, 0, mapCanvas.width, mapCanvas.height);

    if (!gameState) return;
    const map = gameState.currentMap || gameState.CurrentMap;
    const grid = map?.grid || map?.Grid;
    if (!grid) return;

    for (let c = 0; c < COLS; c++) {
        if (!grid[c]) continue;
        for (let r = 0; r < ROWS; r++) {
            if (grid[c][r] === 1) {
                mapCtx.fillStyle = "#0f380f";
                mapCtx.fillRect(c * BLOCK_SIZE, r * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
                mapCtx.strokeStyle = "#9bbc0f"; mapCtx.lineWidth = 1;
                mapCtx.strokeRect(c * BLOCK_SIZE + 1, r * BLOCK_SIZE + 1, BLOCK_SIZE - 2, BLOCK_SIZE - 2);
            } else if (grid[c][r] === 2) {
                mapCtx.fillStyle = "#306230";
                mapCtx.fillRect(c * BLOCK_SIZE + 2, r * BLOCK_SIZE + 2, BLOCK_SIZE - 4, BLOCK_SIZE - 4);
                mapCtx.strokeStyle = "#0f380f"; mapCtx.lineWidth = 1;
                mapCtx.strokeRect(c * BLOCK_SIZE + 2, r * BLOCK_SIZE + 2, BLOCK_SIZE - 4, BLOCK_SIZE - 4);
            }
        }
    }
    isMapDirty = false;
}

function draw() {
    if (isMapDirty) {
        renderMapToBuffer();
    }

    ctx.drawImage(mapCanvas, 0, 0);

    if (!gameState) { requestAnimationFrame(draw); return; }

    const p1Data = gameState.player1 || gameState.Player1;
    const p2Data = gameState.player2 || gameState.Player2;

    updateBodyAngle(p1Data, 'p1');
    updateBodyAngle(p2Data, 'p2');

    drawPlayer(p1Data, "#0f380f", 'p1');
    drawPlayer(p2Data, "#306230", 'p2');

    const bullets = gameState.activeBullets || gameState.ActiveBullets;
    if (bullets) {
        ctx.fillStyle = "#0f380f";
        for (let i = 0; i < bullets.length; i++) {
            const pos = bullets[i].position || bullets[i].Position;
            if (!pos) continue;
            const bx = pos.x !== undefined ? pos.x : pos.X;
            const by = pos.y !== undefined ? pos.y : pos.Y;
            ctx.beginPath();
            ctx.arc(bx, by, 7, 0, Math.PI * 2);
            ctx.fill();
        }
    }
    requestAnimationFrame(draw);
}

function drawPlayer(p, col, key) {
    if (!p) return;
    const pos = p.position || p.Position;
    if (!pos) return;

    const px = pos.x !== undefined ? pos.x : pos.X;
    const py = pos.y !== undefined ? pos.y : pos.Y;
    const isAlive = p.isAlive !== undefined ? p.isAlive : p.IsAlive;
    const turretAngle = p.rotationAngle !== undefined ? p.rotationAngle : p.RotationAngle;

    if (!isAlive) {
        ctx.save(); ctx.translate(px, py);
        ctx.strokeStyle = "#0f380f"; ctx.lineWidth = 4;
        ctx.beginPath();
        ctx.moveTo(-16, -16); ctx.lineTo(16, 16);
        ctx.moveTo(16, -16); ctx.lineTo(-16, 16);
        ctx.stroke();
        ctx.fillStyle = "#0f380f"; ctx.fillRect(-6, -6, 12, 12);
        ctx.restore();
        return;
    }

    // Гусеницы и корпус
    ctx.save();
    ctx.translate(px, py);
    ctx.rotate(bodyAngles[key] || 0);

    ctx.fillStyle = "#0f380f";
    ctx.fillRect(-17, -19, 34, 6);
    ctx.fillRect(-17, 13, 34, 6);

    ctx.strokeStyle = "#9bbc0f"; ctx.lineWidth = 1;
    for (let offset = -14; offset <= 14; offset += 6) {
        ctx.beginPath(); ctx.moveTo(offset, -19); ctx.lineTo(offset, -13); ctx.stroke();
        ctx.beginPath(); ctx.moveTo(offset, 13); ctx.lineTo(offset, 19); ctx.stroke();
    }

    ctx.fillStyle = col;
    ctx.fillRect(-14, -13, 28, 26);
    ctx.strokeStyle = "#0f380f"; ctx.lineWidth = 2;
    ctx.strokeRect(-14, -13, 28, 26);

    ctx.fillStyle = "#0f380f";
    ctx.fillRect(-13, -7, 3, 14);
    ctx.restore();

    // Башня и дуло
    ctx.save();
    ctx.translate(px, py);
    ctx.rotate(turretAngle || 0);

    ctx.fillStyle = "#0f380f";
    ctx.fillRect(0, -3, 24, 6);
    ctx.fillStyle = col;
    ctx.fillRect(21, -5, 5, 10);
    ctx.strokeStyle = "#0f380f"; ctx.strokeRect(21, -5, 5, 10);

    ctx.fillStyle = col;
    ctx.beginPath(); ctx.arc(0, 0, 10, 0, Math.PI * 2); ctx.fill();
    ctx.strokeStyle = "#0f380f"; ctx.lineWidth = 2; ctx.stroke();

    ctx.fillStyle = "#0f380f";
    ctx.beginPath(); ctx.arc(-2, 0, 3, 0, Math.PI * 2); ctx.fill();
    ctx.restore();

    // Отрисовка щита
    const isShieldActive = p.isShieldActive !== undefined ? p.isShieldActive : p.IsShieldActive;
    if (isShieldActive) {
        ctx.save(); ctx.translate(px, py);
        ctx.rotate((Date.now() / 150) % (Math.PI * 2));
        ctx.strokeStyle = "#0f380f"; ctx.lineWidth = 3; ctx.setLineDash([8, 4]);
        ctx.beginPath(); ctx.arc(0, 0, 27, 0, Math.PI * 2); ctx.stroke();
        ctx.restore();
    }
}

// Вспомогательные функции для всплывающих сообщений
function getOrCreateToastContainer() {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container-fixed';
        document.body.appendChild(container);
    }
    return container;
}

function showInfoToast(title, description, duration = 5000) {
    const container = getOrCreateToastContainer();
    const toast = document.createElement('div');
    toast.className = 'toast-item toast-animate-in mb-2';
    toast.innerHTML = `
        <div class="info-alert-toast">
            <div style="display: flex; align-items: center;">
                <div class="info-icon-box">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.8" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 20.25c4.97 0 9-3.694 9-8.25s-4.03-8.25-9-8.25S3 7.444 3 12c0 2.104.859 4.023 2.273 5.48.432.447.74 1.04.586 1.641a4.483 4.483 0 0 1-.923 1.785A5.969 5.969 0 0 0 6 21c1.282 0 2.47-.402 3.445-1.087.81.22 1.668.337 2.555.337Z"></path>
                    </svg>
                </div>
                <div class="info-body">
                    <p class="info-title">${title}</p>
                    <p class="info-desc">${description}</p>
                </div>
            </div>
            <button type="button" class="toast-close-btn">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12"></path>
                </svg>
            </button>
        </div>
    `;

    bindToastClose(toast, container, duration);
}

function showErrorToast(title, description, duration = 5000) {
    const container = getOrCreateToastContainer();
    const toast = document.createElement('div');
    toast.className = 'toast-item toast-animate-in mb-2';
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

    bindToastClose(toast, container, duration);
}

function bindToastClose(toast, container, duration) {
    const closeBtn = toast.querySelector('.toast-close-btn');
    const removeToast = () => {
        toast.classList.remove('toast-animate-in');
        toast.classList.add('toast-animate-out');
        setTimeout(() => toast.remove(), 250);
    };

    if (closeBtn) {
        closeBtn.addEventListener('click', removeToast);
    }

    if (duration > 0) {
        setTimeout(removeToast, duration);
    }

    container.appendChild(toast);
}

setupMobileControls();
requestAnimationFrame(draw);
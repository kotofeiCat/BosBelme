document.addEventListener("DOMContentLoaded", function () {

    const roomCode = window.roomConfig.roomCode;
    const redirectUrl = window.roomConfig.redirectUrl;

    const roomConnection = new signalR.HubConnectionBuilder()
        .withUrl("/gameRoomHub")
        .withAutomaticReconnect()
        .build();

    roomConnection.on("UpdateRoom", function (roomDetails) {
        console.log("Состав комнаты обновился в реальном времени:", roomDetails);

        const countElement = document.getElementById("player-count");
        if (countElement && roomDetails.players) {
            countElement.textContent = roomDetails.players.length;
        }

        const listElement = document.getElementById("player-list");
        if (listElement && roomDetails.players) {
            listElement.innerHTML = "";

            roomDetails.players.forEach(function (player) {
                const li = document.createElement("li");
                li.style.cssText = "padding: 10px; border-bottom: 1px dashed var(--color-gray-medium); font-family: 'Courier Prime', monospace; font-size: 14px; display: flex; justify-content: space-between; align-items: center;";

                const leftDiv = document.createElement("div");
                leftDiv.style.cssText = "display: flex; align-items: center; gap: 10px;";

                const iconSpan = document.createElement("span");
                iconSpan.textContent = "👤";
                leftDiv.appendChild(iconSpan);

                const nameStrong = document.createElement("strong");
                nameStrong.textContent = player.name;
                leftDiv.appendChild(nameStrong);

                if (player.isGuest) {
                    const guestSpan = document.createElement("span");
                    guestSpan.style.cssText = "color: var(--color-gray-medium); font-size: 11px;";
                    guestSpan.textContent = " [Временный гость]";
                    leftDiv.appendChild(guestSpan);
                }

                const rightDiv = document.createElement("div");
                rightDiv.style.cssText = "display: flex; gap: 5px;";

                const badgeSpan = document.createElement("span");
                badgeSpan.className = "status-badge";

                if (player.isHost) {
                    badgeSpan.style.cssText = "background-color: var(--color-black); color: var(--color-white);";
                    badgeSpan.textContent = "HOST";
                } else {
                    badgeSpan.style.cssText = "background-color: var(--color-gray-light); color: var(--color-black); border: 1px solid var(--color-black)";
                    badgeSpan.textContent = "PLAYER";
                }
                rightDiv.appendChild(badgeSpan);

                li.appendChild(leftDiv);
                li.appendChild(rightDiv);
                listElement.appendChild(li);
            });
        }
    });

    roomConnection.on("RoomDelete", function () {
        alert("Комната была закрыта создателем.");
        window.location.href = redirectUrl;
    });

    roomConnection.start()
        .then(function () {
            roomConnection.invoke("JoinRoom", roomCode)
                .catch(function (err) {
                    console.error("Ошибка при вступлении в группу SignalR:", err.toString());
                });
        })
        .catch(function (err) {
            console.error("Не удалось запустить SignalR подключение:", err.toString());
        });
});
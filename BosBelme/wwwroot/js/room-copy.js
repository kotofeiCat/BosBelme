function copyTextToClipboard(textToCopy, element, type) {
    if (!navigator.clipboard) {
        var textArea = document.createElement("textarea");
        textArea.value = textToCopy;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            triggerSuccessVisual(element, type);
        } catch (err) {
            console.error('Ошибка копирования: ', err);
        }
        document.body.removeChild(textArea);
        return;
    }

    navigator.clipboard.writeText(textToCopy).then(function () {
        triggerSuccessVisual(element, type);
    }, function (err) {
        console.error('Не удалось скопировать: ', err);
    });
}

function triggerSuccessVisual(element, type) {
    if (type === 'code') {
        var icon = document.getElementById("code-icon");
        var codeText = document.getElementById("display-room-code");

        var oldText = codeText.textContent;
        var oldIcon = icon ? icon.textContent : "";

        codeText.textContent = "СКОПИРОВАНО!";
        if (icon) icon.textContent = "✔️";
        element.style.pointerEvents = "none";
        element.style.background = "var(--color-black)";
        element.style.color = "var(--color-white)";
        if (icon) icon.style.color = "var(--color-white)";

        setTimeout(function () {
            codeText.textContent = oldText;
            if (icon) icon.textContent = oldIcon;
            element.style.pointerEvents = "auto";
            element.style.background = "var(--color-white)";
            element.style.color = "var(--color-black)";
            if (icon) icon.style.color = "var(--color-gray-medium)";
        }, 1300);

    } else if (type === 'link') {
        var originalText = element.textContent;

        element.textContent = "СКОПИРОВАНО!";
        element.style.background = "var(--color-black)";
        element.style.color = "var(--color-white)";
        element.style.boxShadow = "0px 0px 0px var(--color-black)";
        element.style.transform = "translate(3px, 3px)";
        element.style.pointerEvents = "none";

        setTimeout(function () {
            element.textContent = originalText;
            element.style.background = "var(--color-white)";
            element.style.color = "var(--color-black)";
            element.style.boxShadow = "3px 3px 0px var(--color-black)";
            element.style.transform = "none";
            element.style.pointerEvents = "auto";
        }, 1500);
    }
}
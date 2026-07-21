async function handleCopy(btn) {
    const text = btn.getAttribute('data-copy-text');
    if (!text) return;

    let success = false;

    // 1. Попытка скопировать через modern API (работает только на HTTPS / localhost)
    if (navigator.clipboard && window.isSecureContext) {
        try {
            await navigator.clipboard.writeText(text);
            success = true;
        } catch (err) {
            console.warn('Clipboard API недоступен, переключаемся на fallback:', err);
        }
    }

    // 2. Резервный метод (работает по HTTP на боевом сервере)
    if (!success) {
        try {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.select();
            success = document.execCommand('copy');
            document.body.removeChild(textarea);
        } catch (err) {
            console.error('Ошибка копирования:', err);
        }
    }

    // 3. Визуальный отклик без разрушения HTML-структуры кнопки
    if (success) {
        const icon = btn.querySelector('i');
        if (icon) {
            const originalClass = icon.className;
            icon.className = 'fa-solid fa-check';
            setTimeout(() => icon.className = originalClass, 1500);
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll(".copy-container").forEach(btn => {
        btn.addEventListener('click', () => handleCopy(btn));
    });
});
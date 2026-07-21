async function handleCopy(btn) {
    const text = btn.getAttribute('data-copy-text');

    try {
        await navigator.clipboard.writeText(text);

        if (btn.copyTimeout) {
            clearTimeout(btn.copyTimeout);
        } else {
            btn.dataset.originalText = btn.textContent;
            btn.style.minWidth = `${btn.offsetWidth}px`;
            btn.style.minHeight = `${btn.offsetHeight}px`;
        }

        btn.classList.add('copy-container-active');
        btn.textContent = 'Скопировано!';

        btn.copyTimeout = setTimeout(() => {
            btn.classList.remove('copy-container-active');
            btn.textContent = btn.dataset.originalText;

            btn.style.minWidth = '';
            btn.style.minHeight = '';

            btn.copyTimeout = null;
        }, 3000);

    } catch (err) {
        console.error('Не удалось скопировать текст:', err);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const btns = document.querySelectorAll(".copy-container");

    btns.forEach(btn => {
        btn.addEventListener('click', () => handleCopy(btn));
    });
});

async function copyTextFromButton(text) {
    try {
        await navigator.clipboard.writeText(text);
    } catch { }
}

function activateCopyButton(btn) {
    const width = btn.offsetWidth;
    const height = btn.offsetHeight;

    btn.style.minWidth = width + 'px';
    btn.style.minHeight = height + 'px';

    btn.classList.add('copy-container-active');
    btn.textContent = 'Скопировано!';
}

document.addEventListener('DOMContentLoaded', function() {
    const btns = document.querySelectorAll(".copy-container");

    btns.forEach(btn => {
        const text = btn.getAttribute('data-copy-text');
    
        btn.addEventListener('click', () => copyTextFromButton(text));
        btn.addEventListener('click', () => activateCopyButton(btn));
    });
});
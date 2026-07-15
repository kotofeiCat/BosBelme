
async function copyTextFromButton(text) {
    try {
        await navigator.clipboard.writeText(text);
    } catch {

    }
}

document.addEventListener('DOMContentLoaded', function() {
    const btn = document.querySelector(".copy-container");
    const text = btn.getAttribute('data-copy-text');
    
    btn.addEventListener('click', () => copyTextFromButton(text));
});
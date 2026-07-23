document.addEventListener("DOMContentLoaded", () => {
    const path = window.location.pathname.toLowerCase();
    const dockItems = document.querySelectorAll('.dock-item');

    dockItems.forEach(item => item.classList.remove('active'));

    if (path.includes('/hub')) {
        document.querySelector('.dock-item[data-tab="hub"]')?.classList.add('active');
    } else if (path.includes('/account')) {
        document.querySelector('.dock-item[data-tab="profile"]')?.classList.add('active');
    } else if (path.includes('/home/help')) {
        document.querySelector('.dock-item[data-tab="help"]')?.classList.add('active');
    } else {
        document.querySelector('.dock-item[data-tab="home"]')?.classList.add('active');
    }
});
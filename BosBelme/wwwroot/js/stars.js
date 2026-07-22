function createStars(container, count, size) {
    if (!container) return;

    const width = window.innerWidth;
    const height = window.innerHeight * 2.5;
    const shadows = [];

    for (let i = 0; i < count; i++) {
        const x = Math.floor(Math.random() * width)
        const y = Math.floor(Math.random() * height)

        shadows.push(`${x}px ${y}px white`)
    }

    container.style.width = size + 'px';
    container.style.height = size + 'px';
    container.style.boxShadow = shadows.join(', ');

    const style = document.createElement('style')
    style.textContent = `
        #${container.id}::after {
            content: "";
            top: ${height}px;
            width: ${size}px;
            height: ${size}px;
            position: absolute;
            box-shadow: ${shadows.join(', ')};
        }
    `
    document.head.appendChild(style);
}

document.addEventListener('DOMContentLoaded', () => {
    small_stars = document.getElementById('stars1');
    medium_stars = document.getElementById('stars2');
    large_stars = document.getElementById('stars3');

    createStars(small_stars, 300, 1);
    createStars(medium_stars, 200, 2);
    createStars(large_stars, 100, 3);
})
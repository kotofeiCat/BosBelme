const draggable = document.querySelector('.window');
const handle = document.querySelector('.empty-square');

const initialTop = getComputedStyle(draggable).top;
const initialLeft = getComputedStyle(draggable).left;

let isDragging = false;
let startX, startY;      
let startLeft, startTop;

function startDrag(clientX, clientY) {
    isDragging = true;

    startX = clientX;
    startY = clientY;

    startLeft = draggable.offsetLeft;
    startTop = draggable.offsetTop;

    document.body.style.cursor = 'grabbing';
}

function doDrag(clientX, clientY) {
    if (!isDragging) return;

    const deltaX = clientX - startX;
    const deltaY = clientY - startY;

    draggable.style.left = (startLeft + deltaX) + 'px';
    draggable.style.top = (startTop + deltaY) + 'px';
}

function stopDrag() {
    if (isDragging) {
        isDragging = false;
        document.body.style.cursor = '';
    }
}

function resetPosition() {
    draggable.style.top = initialTop;
    draggable.style.left = initialLeft;
}

handle.addEventListener('mousedown', function(event) {
    event.preventDefault();
    event.stopPropagation();
    startDrag(event.clientX, event.clientY);
});

document.addEventListener('mousemove', function(event) {
    doDrag(event.clientX, event.clientY);
});

document.addEventListener('mouseup', function(event) {
    stopDrag();
});

handle.addEventListener('dblclick', (e) => {
    e.preventDefault();
    resetPosition();
});

let lastTapTime = 0;

handle.addEventListener('touchstart', (e) => {
    const currentTime = new Date().getTime();
    const tapGap = currentTime - lastTapTime;

    if (tapGap < 300 && tapGap > 0) {
        e.preventDefault();
        resetPosition();
    } else {
        e.preventDefault();
        const touch = e.touches[0];
        startDrag(touch.clientX, touch.clientY);
    }

    lastTapTime = currentTime;
}, { passive: false });

document.addEventListener('touchmove', function(event) {
    if (!isDragging) return;
    const touch = event.touches[0];
    if (touch) {
        doDrag(touch.clientX, touch.clientY);
    }
}, { passive: false });

document.addEventListener('touchend', function(event) {
    stopDrag();
});

window.addEventListener('mouseleave', function(event) {
    stopDrag();
});
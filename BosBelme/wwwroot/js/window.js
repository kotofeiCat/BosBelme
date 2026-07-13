const draggable = document.querySelector('.window');
const handle = document.querySelector('.empty-square');

const initialTop = getComputedStyle(draggable).top;
const initialLeft = getComputedStyle(draggable).left;

// Проверяем, что элементы найдены
if (!draggable || !handle) {
    console.error('Элементы не найдены! Проверь id и классы.');
}

// Флаги и переменные для хранения состояния
let isDragging = false;
let startX, startY;      // Координаты клика по handle
let startLeft, startTop; // Изначальная позиция элемента

// --- Общая функция начала перетаскивания ---
function startDrag(clientX, clientY) {
    console.log('Начало перетаскивания:', clientX, clientY);
    isDragging = true;

    // Запоминаем, где был курсор/палец в момент начала
    startX = clientX;
    startY = clientY;

    // Запоминаем текущую позицию элемента
    startLeft = draggable.offsetLeft;
    startTop = draggable.offsetTop;

    // Меняем курсор для всего документа на время перетаскивания
    document.body.style.cursor = 'grabbing';
}

// --- Общая функция движения ---
function doDrag(clientX, clientY) {
    if (!isDragging) return;

    // Считаем смещение курсора от начальной точки
    const deltaX = clientX - startX;
    const deltaY = clientY - startY;

    // Применяем смещение к позиции элемента
    draggable.style.left = (startLeft + deltaX) + 'px';
    draggable.style.top = (startTop + deltaY) + 'px';
}

// --- Общая функция завершения перетаскивания ---
function stopDrag() {
    if (isDragging) {
        console.log('Конец перетаскивания');
        isDragging = false;
        document.body.style.cursor = '';
    }
}

function resetPosition() {
    draggable.style.top = initialTop;
    draggable.style.left = initialLeft;
}

// ==========================================
// 1. СОБЫТИЯ МЫШИ
// ==========================================

handle.addEventListener('mousedown', function(event) {
    // event — объект, который браузер передаёт в эту функцию
    event.preventDefault(); // Запрещаем стандартное выделение
    event.stopPropagation(); // Чтобы событие не всплывало выше
    startDrag(event.clientX, event.clientY);
});

document.addEventListener('mousemove', function(event) {
    // Здесь тоже event приходит от браузера
    doDrag(event.clientX, event.clientY);
});

document.addEventListener('mouseup', function(event) {
    stopDrag();
});

handle.addEventListener('dblclick', (e) => {
    e.preventDefault();
    resetPosition();
});

// ==========================================
// 2. СОБЫТИЯ ПАЛЬЦА (ТАЧСКРИН)
// ==========================================

let lastTapTime = 0;

handle.addEventListener('touchstart', (e) => {
    const currentTime = new Date().getTime();
    const tapGap = currentTime - lastTapTime;

    if (tapGap < 300 && tapGap > 0) {
        // Двойной тап обнаружен
        e.preventDefault();
        resetPosition();
    } else {
        // Одинарный тап — начинаем перетаскивание
        e.preventDefault();
        const touch = e.touches[0];
        startDrag(touch.clientX, touch.clientY);
    }

    lastTapTime = currentTime;
}, { passive: false });

document.addEventListener('touchmove', function(event) {
    if (!isDragging) return;
    // Не вызываем preventDefault здесь, чтобы не блокировать скролл полностью
    const touch = event.touches[0];
    if (touch) {
        doDrag(touch.clientX, touch.clientY);
    }
}, { passive: false });

document.addEventListener('touchend', function(event) {
    stopDrag();
});

// Дополнительно: обработчик на случай, если мышь отпущена вне окна
window.addEventListener('mouseleave', function(event) {
    stopDrag();
});

console.log('Скрипт загружен и готов к работе');
(function () {
    'use strict';
    var support = document.getElementById('food-image-support');
    if (!support) return;
    function fallback(image) {
        if (!image || image.tagName !== 'IMG' || !image.hasAttribute('data-food-image') || image.dataset.fallbackApplied === image.src) return;
        image.src = support.dataset.placeholder;
        image.dataset.fallbackApplied = image.src;
    }
    document.addEventListener('error', function (event) { fallback(event.target); }, true);
    document.querySelectorAll('img[data-food-image]').forEach(function (image) {
        if (image.complete && image.naturalWidth === 0) fallback(image);
    });
})();

(function () {
    'use strict';
    document.querySelectorAll('[data-menu-catalog]').forEach(function (root) {
        var search = root.querySelector('[data-menu-search]');
        var form = root.querySelector('[data-menu-form]');
        var filters = root.querySelectorAll('[data-menu-filter]');
        var items = root.querySelectorAll('[data-menu-item]');
        var count = root.querySelector('[data-menu-count]');
        var empty = root.querySelector('[data-menu-empty]');
        var selected = root.dataset.selectedCategory || 'all';
        function render() {
            var query = search.value.trim().toLocaleLowerCase('vi-VN'), visible = 0;
            items.forEach(function (item) {
                var matchesCategory = selected === 'all' || (selected === 'combo' ? item.dataset.kind === 'combo' : item.dataset.kind === 'dish' && item.dataset.category === selected);
                item.hidden = !matchesCategory || item.dataset.search.toLocaleLowerCase('vi-VN').indexOf(query) < 0;
                if (!item.hidden) visible++;
            });
            filters.forEach(function (filter) {
                var active = filter.dataset.menuFilter === selected;
                filter.classList.toggle('active', active);
                filter.setAttribute('aria-pressed', active ? 'true' : 'false');
            });
            count.textContent = 'Hiển thị ' + visible + ' món / suất ăn';
            empty.hidden = visible > 0;
        }
        filters.forEach(function (filter) { filter.addEventListener('click', function () { selected = filter.dataset.menuFilter; render(); }); });
        search.addEventListener('input', render);
        form.addEventListener('submit', function (event) { event.preventDefault(); render(); });
        render();

        var modal = root.querySelector('[data-menu-modal]');
        var body = root.querySelector('[data-menu-modal-body]');
        var title = root.querySelector('[data-menu-modal-title]');
        var request = 0;
        root.querySelectorAll('[data-menu-detail]').forEach(function (trigger) {
            trigger.addEventListener('click', function () {
                var version = ++request;
                title.textContent = trigger.dataset.detailTitle;
                body.textContent = 'Đang tải chi tiết…';
                if (window.jQuery) window.jQuery(modal).modal('show');
                fetch(trigger.dataset.menuDetail, { credentials: 'same-origin', cache: 'no-store', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                    .then(function (response) { if (!response.ok) throw new Error('request'); return response.text(); })
                    .then(function (html) { if (version === request) body.innerHTML = html; })
                    .catch(function () { if (version === request) body.textContent = 'Không thể tải chi tiết. Vui lòng đóng cửa sổ và thử lại.'; });
            });
        });
    });
})();

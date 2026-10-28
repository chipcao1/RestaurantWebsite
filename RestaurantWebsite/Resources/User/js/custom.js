(function () {
    'use strict';
    var year = document.querySelector('#displayYear');
    if (year) year.textContent = new Date().getFullYear();

    // Trang thực đơn tự quản lý bộ lọc; không đăng ký Isotope hai lần ở đây.
    var $ = window.jQuery;
    if (!$) return;
    $(function () {
        if ($.fn.niceSelect) {
            $('select.nice-select-custom').not('#OrderType').niceSelect();
        }
        if ($.fn.owlCarousel && $('.client_owl-carousel').length) {
            $('.client_owl-carousel').owlCarousel({
                loop: true,
                margin: 0,
                dots: false,
                nav: true,
                autoplay: true,
                autoplayHoverPause: true,
                navText: [
                    '<i class="fa fa-angle-left" aria-label="Trước"></i>',
                    '<i class="fa fa-angle-right" aria-label="Sau"></i>'
                ],
                responsive: { 0: { items: 1 }, 768: { items: 2 }, 1000: { items: 2 } }
            });
        }
    });
})();

(function () {
    'use strict';
    var form = document.getElementById('checkoutForm');
    if (!form) return;
    var code = document.getElementById('promotionCode');
    var message = document.getElementById('promotionMessage');
    var summary = document.getElementById('promotionSummary');
    var description = document.getElementById('promotionDescription');
    var discount = document.getElementById('discountPreview');
    var total = document.getElementById('checkoutTotal');
    var subtotalLabel = document.getElementById('summarySubtotal');
    var submit = document.getElementById('placeOrderButton');
    var subtotal = Number(form.dataset.subtotal);
    var revision = 0, timer, validatedCode = '', valid = true;
    var money = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 });
    function currency(value) { return money.format(value) + ' ₫'; }
    function clearDiscount() {
        summary.hidden = true;
        description.textContent = '';
        discount.textContent = currency(0);
        total.textContent = currency(subtotal);
    }
    function showMessage(text, error) {
        message.textContent = text;
        message.className = 'form-text ' + (error ? 'text-danger' : 'text-success');
    }
    function check() {
        window.clearTimeout(timer);
        var current = code.value.trim(), requestRevision = revision;
        if (!current) return;
        var body = new URLSearchParams();
        body.set('__RequestVerificationToken', form.querySelector('input[name="__RequestVerificationToken"]').value);
        body.set('promotionCode', current);
        fetch(form.dataset.previewUrl, {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8', 'Accept': 'application/json' },
            body: body.toString()
        }).then(function (response) {
            if (!response.ok || response.redirected) throw new Error('request');
            return response.json();
        }).then(function (quote) {
            if (revision !== requestRevision || code.value.trim() !== current) return;
            if (!quote.success) {
                valid = false; submit.disabled = true;
                code.setCustomValidity(quote.message);
                showMessage(quote.message, true);
                return;
            }
            subtotal = Number(quote.subtotal);
            subtotalLabel.textContent = currency(subtotal);
            discount.textContent = currency(Number(quote.discount));
            total.textContent = currency(Number(quote.total));
            description.textContent = quote.code + ' · ' + quote.description;
            summary.hidden = !quote.code;
            valid = true; validatedCode = current; submit.disabled = false;
            code.setCustomValidity('');
            showMessage(quote.code ? 'Đã áp dụng mã ' + quote.code + '. Giảm ' + currency(Number(quote.discount)) + '.' : '', false);
        }).catch(function () {
            if (revision !== requestRevision || code.value.trim() !== current) return;
            valid = false; submit.disabled = true;
            code.setCustomValidity('Chưa kiểm tra được mã khuyến mãi.');
            showMessage('Không thể kiểm tra mã. Vui lòng thử lại hoặc xóa mã để tiếp tục; nếu phiên đã hết hạn, hãy đăng nhập lại.', true);
        });
    }
    function changed(immediate) {
        revision++;
        window.clearTimeout(timer);
        clearDiscount();
        validatedCode = ''; code.setCustomValidity('');
        valid = !code.value.trim(); submit.disabled = !valid;
        showMessage(valid ? '' : 'Đang kiểm tra mã khuyến mãi…', false);
        if (!valid) { if (immediate) check(); else timer = window.setTimeout(check, 350); }
    }
    code.addEventListener('input', function () { changed(false); });
    code.addEventListener('change', function () {
        if (!valid || code.value.trim() !== validatedCode) changed(true);
    });
    code.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && code.value.trim()) { event.preventDefault(); changed(true); }
    });
    form.addEventListener('submit', function (event) {
        if (!valid || code.value.trim() !== validatedCode) { event.preventDefault(); changed(true); }
    });
    var type = document.getElementById('OrderType');
    var reservation = document.getElementById('ReservationId');
    var table = document.getElementById('TableId');
    function updateType() {
        var delivery = type.value === 'Delivery', dineIn = type.value === 'Dine-in';
        document.getElementById('deliveryAddressGroup').hidden = !delivery;
        document.getElementById('DeliveryAddress').required = delivery;
        document.getElementById('dineInGroup').hidden = !dineIn;
        reservation.disabled = !dineIn;
        table.disabled = !dineIn || Boolean(reservation.value);
        table.required = dineIn && !reservation.value;
    }
    type.addEventListener('change', updateType);
    reservation.addEventListener('change', updateType);
    updateType(); changed(Boolean(code.value.trim()));
})();

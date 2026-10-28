(function(){
    'use strict';
    document.addEventListener('invalid',function(event){var field=event.target;if(!field.setCustomValidity)return;if(field.validity.valueMissing)field.setCustomValidity('Vui lòng nhập hoặc chọn thông tin này.');else if(field.validity.typeMismatch)field.setCustomValidity('Vui lòng nhập đúng định dạng.');else if(field.validity.rangeUnderflow||field.validity.rangeOverflow)field.setCustomValidity('Giá trị nằm ngoài giới hạn cho phép.');else if(field.validity.stepMismatch)field.setCustomValidity('Vui lòng nhập giá trị theo bước số được yêu cầu.');},true);
    document.addEventListener('input',function(event){if(event.target.setCustomValidity)event.target.setCustomValidity('');},true);
    document.addEventListener('change',function(event){if(event.target.setCustomValidity)event.target.setCustomValidity('');},true);
    var accountButton = document.getElementById('accountMenu');
    if (accountButton) {
        var accountPanel = accountButton.parentElement.querySelector('.dropdown-menu');
        accountButton.addEventListener('click', function (event) { event.stopPropagation(); var open = accountPanel.classList.toggle('show'); accountButton.setAttribute('aria-expanded', open ? 'true' : 'false'); });
        function closeAccount() { accountPanel.classList.remove('show'); accountButton.setAttribute('aria-expanded', 'false'); }
        document.addEventListener('click', closeAccount);
        document.addEventListener('keydown', function (event) { if (event.key === 'Escape') closeAccount(); });
    }
    document.querySelectorAll('form[method=post],form[method=POST]').forEach(function(form){form.addEventListener('submit',function(event){if(event.defaultPrevented)return;window.setTimeout(function(){form.querySelectorAll('button[type=submit],button:not([type])').forEach(function(button){button.disabled=true;});},0);});});
})();

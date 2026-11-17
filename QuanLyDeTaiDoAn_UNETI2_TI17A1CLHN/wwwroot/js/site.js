
(function () {
    "use strict";

    // =====================================================
    // XỬ LÝ FORM TOÀN HỆ THỐNG
    // =====================================================

    document.addEventListener("submit", function (event) {

        const form = event.target;

        // Chỉ xử lý form HTML
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        const method = (form.method || "get").toLowerCase();

        // Chỉ áp dụng với POST
        if (method !== "post") {
            return;
        }

        // Không tự động hiển thị window.confirm().
        // Các bảng xác nhận Bootstrap riêng của từng chức năng
        // vẫn hoạt động bình thường.

        // Không preventDefault()
        // Không stopImmediatePropagation()
        // Không requestSubmit() lại form.

    }, true);

})();

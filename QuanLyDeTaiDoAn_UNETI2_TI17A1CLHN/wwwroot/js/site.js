
(function () {
    "use strict";

    document.addEventListener("submit", function (event) {

        const form = event.target;

        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        const method = (form.method || "get").toLowerCase();

        if (method !== "post") {
            return;
        }

        const action = (
            form.getAttribute("action") || ""
        ).toLowerCase();

        // FORM ĐĂNG XUẤT:
        // Đã có modal Bootstrap trong _Layout.cshtml
        // Không hiển thị thêm window.confirm của trình duyệt.
        if (
            action.includes("/dangnhap/dangxuat") ||
            action.includes("/dangnhap/logout")
        ) {
            return;
        }

        // FORM ĐÃ CÓ XÁC NHẬN RIÊNG
        if (form.hasAttribute("data-no-confirm")) {
            return;
        }

        // FORM ĐÃ XÁC NHẬN
        if (form.dataset.unetiConfirmed === "1") {
            return;
        }

        if (!form.checkValidity()) {
            return;
        }

        const submitter = event.submitter;

        const label = (
            submitter?.textContent ||
            submitter?.value ||
            ""
        ).trim().toLowerCase();

        let message =
            form.dataset.confirmMessage ||
            "Bạn có chắc chắn muốn lưu thay đổi này?";

        if (!form.dataset.confirmMessage) {

            const operation = action + " " + label;

            if (/xoa|delete/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn xóa dữ liệu này?";
            }
            else if (/huy|cancel/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn hủy đăng ký này?";
            }
            else if (/tu.?choi|reject/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn từ chối đăng ký?";
            }
            else if (/duyet|approve/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn duyệt đăng ký?";
            }
            else if (/phan.?cong|thay.?doi/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn lưu phân công hoặc thay đổi giảng viên?";
            }
            else if (/dang.?xuat|logout/.test(operation)) {
                message =
                    "Bạn có chắc chắn muốn đăng xuất?";
            }
        }

        event.preventDefault();
        event.stopImmediatePropagation();

        if (!window.confirm(message)) {
            return;
        }

        form.dataset.unetiConfirmed = "1";

        if (
            submitter &&
            submitter.form === form &&
            !submitter.disabled
        ) {
            form.requestSubmit(submitter);
        }
        else {
            form.requestSubmit();
        }

        window.setTimeout(function () {
            delete form.dataset.unetiConfirmed;
        }, 1500);

    }, true);

})();


(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const modalElement = document.getElementById("modalXacNhanChung");
        const noiDungElement = document.getElementById("noiDungXacNhanChung");
        const btnDongY = document.getElementById("btnDongYXacNhanChung");

        if (!modalElement || !noiDungElement || !btnDongY) {
            console.error("Không tìm thấy modal xác nhận chung.");
            return;
        }

        if (typeof bootstrap === "undefined") {
            console.error("Bootstrap chưa được tải.");
            return;
        }

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

        let formDangCho = null;
        let submitterDangCho = null;
        let chapNhanGuiForm = false;
        let dangDongModal = false;

        function boDau(value) {
            return String(value || "")
                .toLowerCase()
                .normalize("NFD")
                .replace(/[\u0300-\u036f]/g, "")
                .replace(/đ/g, "d")
                .replace(/\s+/g, "")
                .trim();
        }

        function layThongTin(form, submitter) {
            return boDau([
                form.getAttribute("action"),
                form.getAttribute("asp-action"),
                submitter?.getAttribute("formaction"),
                submitter?.formAction,
                submitter?.textContent,
                submitter?.value,
                submitter?.getAttribute("title"),
                submitter?.getAttribute("aria-label")
            ].join(" "));
        }

        function laDangXuat(form, submitter) {
            const thongTin = layThongTin(form, submitter);

            return /dangxuat|logout|signout/.test(thongTin);
        }

        function layNoiDungXacNhan(form, submitter) {
            if (form.dataset.confirmMessage) {
                return form.dataset.confirmMessage;
            }

            const thongTin = layThongTin(form, submitter);

            // Đăng xuất luôn được ưu tiên nhận diện.
            if (laDangXuat(form, submitter)) {
                return "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?";
            }

            if (/xoa|delete|remove/.test(thongTin)) {
                return "Bạn có chắc chắn muốn xóa dữ liệu này không?";
            }

            if (/tuchoi|reject/.test(thongTin)) {
                return "Bạn có chắc chắn muốn từ chối yêu cầu này không?";
            }

            if (/duyet|approve/.test(thongTin)) {
                return "Bạn có chắc chắn muốn duyệt yêu cầu này không?";
            }

            if (/huy|cancel/.test(thongTin)) {
                return "Bạn có chắc chắn muốn hủy thao tác này không?";
            }

            if (/phancong|assign/.test(thongTin)) {
                return "Bạn có chắc chắn muốn lưu thông tin phân công này không?";
            }

            if (/them|create|add/.test(thongTin)) {
                return "Bạn có chắc chắn muốn thêm dữ liệu này không?";
            }

            if (/sua|edit|update|capnhat/.test(thongTin)) {
                return "Bạn có chắc chắn muốn cập nhật dữ liệu này không?";
            }

            return "Bạn có chắc chắn muốn thực hiện thao tác này không?";
        }

        function boQuaXacNhan(form, submitter) {
            if (
                form.dataset.confirm === "false" ||
                form.dataset.noConfirm === "true"
            ) {
                return true;
            }

            // Không can thiệp form thuộc modal nghiệp vụ riêng.
            if (
                form.closest(".modal") &&
                !form.closest("#modalXacNhanChung")
            ) {
                return true;
            }

            const thongTin = layThongTin(form, submitter);

            // Đăng xuất không được bỏ qua xác nhận.
            if (/dangxuat|logout|signout/.test(thongTin)) {
                return false;
            }

            // Đăng nhập không cần xác nhận.
            if (/dangnhap|login|signin/.test(thongTin)) {
                return true;
            }

            return false;
        }

        document.addEventListener("submit", function (event) {
            const form = event.target;

            if (!(form instanceof HTMLFormElement)) {
                return;
            }

            const submitter = event.submitter || null;

            const method = (
                submitter?.formMethod ||
                form.method ||
                "get"
            ).toLowerCase();

            if (method !== "post") {
                return;
            }

            if (boQuaXacNhan(form, submitter)) {
                return;
            }

            // Cho phép gửi thật sau khi đã xác nhận.
            if (form.dataset.unetiConfirmed === "1") {
                delete form.dataset.unetiConfirmed;
                return;
            }

            event.preventDefault();

            if (formDangCho || dangDongModal) {
                return;
            }

            formDangCho = form;
            submitterDangCho = submitter;

            noiDungElement.textContent =
                layNoiDungXacNhan(form, submitter);

            modal.show();
        }, true);

        btnDongY.addEventListener("click", function () {
            if (!formDangCho || dangDongModal) {
                return;
            }

            chapNhanGuiForm = true;
            dangDongModal = true;
            btnDongY.disabled = true;

            modal.hide();
        });

        modalElement.addEventListener("hidden.bs.modal", function () {
            const form = formDangCho;
            const submitter = submitterDangCho;
            const duocChapNhan = chapNhanGuiForm;

            formDangCho = null;
            submitterDangCho = null;
            chapNhanGuiForm = false;
            dangDongModal = false;

            btnDongY.disabled = false;

            if (!duocChapNhan || !form || !form.isConnected) {
                return;
            }

            const boQuaValidate =
                form.noValidate ||
                (submitter && submitter.formNoValidate);

            if (
                !boQuaValidate &&
                typeof form.reportValidity === "function" &&
                !form.reportValidity()
            ) {
                return;
            }

            form.dataset.unetiConfirmed = "1";

            try {
                if (
                    submitter &&
                    submitter.form === form &&
                    !submitter.disabled
                ) {
                    form.requestSubmit(submitter);
                } else {
                    form.requestSubmit();
                }
            } catch (error) {
                delete form.dataset.unetiConfirmed;
                console.error("Lỗi gửi form:", error);
            }
        });
    });
})();

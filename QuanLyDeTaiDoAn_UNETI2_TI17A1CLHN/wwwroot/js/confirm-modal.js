
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const modalElement = document.getElementById("modalXacNhanChung");
        const noiDungElement = document.getElementById("noiDungXacNhanChung");
        const btnDongY = document.getElementById("btnDongYXacNhanChung");

        if (!modalElement || !noiDungElement || !btnDongY) {
            console.error("Không tìm thấy thành phần của modal xác nhận chung.");
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
                .trim();
        }

        // Chỉ lấy URL được khai báo trong HTML.
        // Không sử dụng form.action vì nó có thể trả về URL trang hiện tại.
        function layThongTinForm(form, submitter) {
            const actionForm = form.getAttribute("action") || "";
            const actionButton =
                submitter?.getAttribute("formaction") || "";

            const noiDungNut = [
                submitter?.textContent || "",
                submitter?.value || "",
                submitter?.getAttribute("title") || "",
                submitter?.getAttribute("aria-label") || "",
                submitter?.getAttribute("name") || ""
            ].join(" ");

            return {
                action: boDau(actionButton || actionForm),
                button: boDau(noiDungNut),
                all: boDau([
                    actionButton,
                    actionForm,
                    noiDungNut
                ].join(" "))
            };
        }

        function layNoiDungXacNhan(form, submitter) {
            if (form.dataset.confirmMessage) {
                return form.dataset.confirmMessage;
            }

            const thongTin = layThongTinForm(form, submitter);

            // 1. Đăng xuất: ưu tiên nhận diện trước tất cả thao tác khác.
            if (
                /dangxuat|logout|signout/.test(thongTin.all) ||
                /dang-xuat|sign-out/.test(thongTin.all)
            ) {
                return "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?";
            }

            // 2. Xóa dữ liệu.
            if (/xoa|delete|remove/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn xóa dữ liệu này không?";
            }

            // 3. Từ chối.
            if (/tuchoi|reject/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn từ chối yêu cầu này không?";
            }

            // 4. Duyệt.
            if (/duyet|approve/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn duyệt yêu cầu này không?";
            }

            // 5. Hủy.
            if (/huy|cancel/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn hủy thao tác này không?";
            }

            // 6. Phân công.
            if (/phancong|assign/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn lưu thông tin phân công này không?";
            }

            // 7. Thêm mới.
            if (/them|create|add/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn thêm dữ liệu này không?";
            }

            // 8. Cập nhật.
            if (/sua|edit|update|capnhat/.test(thongTin.all)) {
                return "Bạn có chắc chắn muốn cập nhật dữ liệu này không?";
            }

            return "Bạn có chắc chắn muốn thực hiện thao tác này không?";
        }

        function boQuaXacNhan(form) {
            if (
                form.dataset.confirm === "false" ||
                form.dataset.noConfirm === "true"
            ) {
                return true;
            }

            // Không can thiệp các form nằm trong modal nghiệp vụ riêng.
            if (
                form.closest(".modal") &&
                !form.closest("#modalXacNhanChung")
            ) {
                return true;
            }

            // Đăng nhập không cần xác nhận.
            const action = boDau(form.getAttribute("action") || "");

            if (/dangnhap|login|sign-in/.test(action)) {
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

            if (boQuaXacNhan(form)) {
                return;
            }

            // Cho phép lần gửi thật sau khi đã xác nhận.
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

                console.error(
                    "Không thể gửi biểu mẫu sau khi xác nhận:",
                    error
                );
            }
        });
    });
})();

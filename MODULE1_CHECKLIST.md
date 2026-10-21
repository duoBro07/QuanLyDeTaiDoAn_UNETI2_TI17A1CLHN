# MODULE 1 - Checklist hoàn thiện

Người thực hiện: Phạm Tùng Dương  
MSSV: **[ĐIỀN MSSV TRƯỚC KHI NỘP]**

## Đã triển khai
- Entity TaiKhoan: MaTaiKhoan, TenDangNhap, MatKhau, HoTen, Email, VaiTro, TrangThai.
- Đăng nhập EF Core/LINQ; chặn tài khoản không tồn tại, sai mật khẩu, tài khoản bị khóa.
- Session: MaTaiKhoan, HoTen, VaiTro.
- Đăng xuất xóa Session.
- Phân quyền Controller bằng SessionAuthorizeAttribute.
- Admin quản lý tài khoản: danh sách, tìm/lọc, thêm, sửa, chi tiết, khóa/mở khóa.
- Kiểm tra tên đăng nhập không trùng; Email validation; role chỉ Admin/SinhVien.
- Không xóa tài khoản; khóa/mở khóa để giữ lịch sử cho Module 3/4.
- Entity LinhVuc và CRUD đầy đủ: Index, Create, Details, Edit, Delete, TrangThai.
- Tên lĩnh vực bắt buộc, không trùng; MoTa dùng textarea.
- Demo ràng buộc tích hợp: khi có bảng GiangVien/DeTai, Delete LinhVuc tự kiểm tra MaLinhVuc trước khi xóa.
- Giao diện Module 1: Login, Dashboard Admin/Sinh viên, tài khoản, lĩnh vực, AccessDenied, menu theo role.

## Phụ thuộc Module khác - đang demo/chờ tích hợp
1. Module 2:
   - LinhVuc 1-N GiangVien.
   - LinhVuc 1-N DeTai.
   - Khi merge Module 2, xác nhận tên bảng/khóa ngoại và thay phần kiểm tra tích hợp nếu nhóm đặt tên khác chuẩn.
2. Module 3/4:
   - SinhVien phải xác định từ TaiKhoan/Session.
   - Trang đăng ký/phân công cá nhân phải không cho đổi ID trên URL để xem dữ liệu người khác.
   - Khóa TaiKhoan chỉ đổi TrangThai, không xóa lịch sử DangKyDeTai/PhanCongHuongDan.

## Test trước khi báo cáo
- Login đúng.
- Sai mật khẩu.
- Tài khoản không tồn tại.
- Tài khoản bị khóa.
- Logout rồi truy cập lại URL yêu cầu đăng nhập.
- SinhVien gõ trực tiếp /TaiKhoan và /LinhVuc -> AccessDenied.
- Thêm tài khoản trùng TenDangNhap -> báo lỗi.
- Email sai định dạng -> báo lỗi.
- Khóa tài khoản -> không login được; mở khóa -> login lại được.
- Tự khóa Admin đang đăng nhập -> bị chặn.
- Create LinhVuc bỏ trống tên -> báo lỗi.
- Create/Edit LinhVuc tên trùng -> báo lỗi.
- Details ID không tồn tại -> NotFound.
- Delete LinhVuc chưa liên quan -> xóa được.
- Sau khi merge M2: Delete LinhVuc có GiangVien/DeTai -> phải bị chặn.

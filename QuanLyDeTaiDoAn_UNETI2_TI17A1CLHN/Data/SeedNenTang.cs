// FILE TẠM - dữ liệu nền (tài khoản Admin, lĩnh vực, giảng viên, đề tài) để test Module 3. Xóa khi ghép nhóm.
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

public static class SeedNenTang
{
    public static void Run(AppDbContext db)
    {
        if (db.LinhVucs.Any()) return;

        db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "admin", MatKhau = "admin123", HoTen = "Quản trị viên", Email = "admin@uneti.edu.vn", VaiTro = VaiTroHeThong.Admin });

        string[] lv = { "Công nghệ thông tin", "Điện - Điện tử", "Cơ khí", "Kinh tế", "Nông nghiệp" };
        db.LinhVucs.AddRange(lv.Select(t => new LinhVuc { TenLinhVuc = t }));
        db.SaveChanges();

        string[] gv = { "Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Thị D", "Hoàng Văn E", "Vũ Thị F", "Đặng Văn G", "Bùi Thị H", "Đỗ Văn I", "Ngô Thị K", "Dương Văn L", "Lý Thị M" };
        for (int i = 0; i < gv.Length; i++)
            db.GiangViens.Add(new GiangVien
            {
                HoTen = gv[i],
                HocVi = i % 2 == 0 ? "Thạc sĩ" : "Tiến sĩ",
                Email = $"gv{i + 1}@uneti.edu.vn",
                SoDienThoai = "0987" + (123000 + i).ToString(),
                MaLinhVuc = i % 5 + 1,
                SoSinhVienToiDa = 2 + i % 4,
                TrangThai = i == 11 ? TrangThaiGiangVien.TamNgung : TrangThaiGiangVien.DangCongTac
            });

        string[] dt = {
            "Phát triển website thương mại điện tử", "Ứng dụng AI nhận diện hình ảnh", "Hệ thống IoT giám sát môi trường",
            "Thiết kế cơ khí máy cắt tự động", "Quản lý bán hàng cho cửa hàng nhỏ", "Nghiên cứu hệ thống IoT trong nông nghiệp",
            "Ứng dụng di động quản lý chi tiêu", "Hệ thống điều khiển đèn thông minh", "Phân tích dữ liệu bán hàng bằng Power BI",
            "Thiết kế mạch nguồn xung công suất nhỏ", "Website quản lý thư viện", "Chatbot hỗ trợ tư vấn tuyển sinh",
            "Mô hình dự báo doanh thu doanh nghiệp", "Hệ thống tưới tiêu tự động", "Thiết kế đồ gá gia công cơ khí",
            "Ứng dụng quản lý lịch học", "Hệ thống nhận diện biển số xe", "Phân tích hành vi khách hàng thương mại điện tử",
            "Thiết kế hệ thống năng lượng mặt trời mini", "Quản lý kho hàng bằng mã QR" };
        for (int i = 0; i < dt.Length; i++)
            db.DeTais.Add(new DeTai
            {
                TenDeTai = dt[i],
                MaLinhVuc = i % 5 + 1,
                MoTa = "Xây dựng và triển khai hệ thống theo yêu cầu thực tế của đề tài.",
                YeuCau = "Có kiến thức nền tảng liên quan, làm việc nhóm tốt.",
                SoSinhVienToiDa = 1 + i % 5,
                NamHoc = i < 16 ? "2026-2027" : "2025-2026",
                HocKy = i % 3 == 0 && i < 16 ? 2 : 1,
                TrangThai = i % 7 == 3 ? TrangThaiDeTai.TamKhoa : (i % 9 == 8 ? TrangThaiDeTai.KetThuc : TrangThaiDeTai.MoDangKy)
            });
        db.SaveChanges();
    }
}

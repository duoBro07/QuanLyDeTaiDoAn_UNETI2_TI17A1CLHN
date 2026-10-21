// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Dữ liệu mẫu Module 3: 30 sinh viên + 30 tài khoản, ~30 đăng ký đủ 4 trạng thái, có lịch sử đổi giảng viên
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

public static class SeedModule3
{
    public static void Run(AppDbContext db)
    {
        if (db.SinhViens.Any()) return;   // đã có dữ liệu thì thôi

        string[] ho = { "Ngô", "Trần", "Lê", "Phạm", "Hoàng", "Nguyễn", "Vũ", "Đặng" };
        string[] ten = { "Văn An", "Thị Bích", "Văn Cường", "Thị Dung", "Văn Em", "Thị Phương", "Văn Giang", "Thị Hoa", "Văn Hùng", "Thị Lan" };
        string[] lop = { "TI17A1HN", "TI17A2HN", "KT17A1HN" };

        // 1) 30 tài khoản sinh viên (tên đăng nhập = mã SV, mật khẩu mẫu 123456)
        var taiKhoans = new List<TaiKhoan>();
        for (int i = 0; i < 30; i++)
        {
            taiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = $"SV{i + 1:000}",
                MatKhau = "123456",
                HoTen = $"{ho[i % 8]} {ten[i % 10]}",
                Email = $"sv{i + 1:000}@uneti.edu.vn",
                VaiTro = VaiTroHeThong.SinhVien,
                TrangThai = i != 29          // SV030 bị khóa để demo
            });
        }
        db.TaiKhoans.AddRange(taiKhoans);
        db.SaveChanges();

        // 2) 30 sinh viên
        var sinhViens = new List<SinhVien>();
        for (int i = 0; i < 30; i++)
        {
            var hoTen = $"{ho[i % 8]} {ten[i % 10]}";
            sinhViens.Add(new SinhVien
            {
                MaSinhVien = $"SV{i + 1:000}",
                MaTaiKhoan = taiKhoans[i].MaTaiKhoan,
                HoTen = hoTen,
                NgaySinh = new DateTime(2003 + i % 3, 1 + i % 12, 1 + i % 27),
                GioiTinh = hoTen.Contains("Thị") ? "Nữ" : "Nam",
                Lop = lop[i % 3],
                Khoa = "Công nghệ thông tin",
                ChuyenNganh = "Kỹ thuật phần mềm",
                Email = $"sv{i + 1:000}@uneti.edu.vn",
                SoDienThoai = "09" + (10000000 + i * 1111).ToString(),
                TrangThai = i == 29 ? TrangThaiSinhVien.Khoa : TrangThaiSinhVien.HoatDong
            });
        }
        db.SinhViens.AddRange(sinhViens);
        db.SaveChanges();

        // 3) Đăng ký: 27 sinh viên đầu mỗi người 1 đăng ký
        //    20 SV đầu: trạng thái xoay vòng Đã duyệt / Chờ duyệt / Từ chối / Đã hủy
        //    SV 21..27: Chờ duyệt hoặc Đã hủy (không Đã duyệt -> số chỗ không bao giờ vượt tối đa)
        string[] trangThais = { TrangThaiDangKy.DaDuyet, TrangThaiDangKy.ChoDuyet, TrangThaiDangKy.TuChoi, TrangThaiDangKy.DaHuy };
        var dangKys = new List<DangKyDeTai>();
        for (int i = 0; i < 27; i++)
        {
            string tt = i < 20 ? trangThais[i % 4] : (i % 2 == 0 ? TrangThaiDangKy.ChoDuyet : TrangThaiDangKy.DaHuy);
            dangKys.Add(new DangKyDeTai
            {
                MaSinhVien = sinhViens[i].MaSinhVien,
                MaDeTai = (i % 20) + 1,
                NgayDangKy = new DateTime(2026, 9, 1).AddDays(i),
                LyDoDangKy = "Em quan tâm đến lĩnh vực này và muốn phát triển kỹ năng thực tế.",
                TrangThai = tt,
                GhiChuDuyet = tt == TrangThaiDangKy.DaDuyet ? "Đồng ý hướng dẫn."
                            : tt == TrangThaiDangKy.TuChoi ? "Hồ sơ chưa phù hợp với yêu cầu đề tài." : null
            });
        }
        // SV001 có thêm 2 đăng ký cũ: 1 bị từ chối, 1 đã hủy (để demo danh sách nhiều dòng)
        dangKys.Add(new DangKyDeTai { MaSinhVien = "SV001", MaDeTai = 7, NgayDangKy = new DateTime(2026, 8, 20), LyDoDangKy = "Em muốn làm ứng dụng di động.", TrangThai = TrangThaiDangKy.TuChoi, GhiChuDuyet = "Đề tài đã đủ số lượng." });
        dangKys.Add(new DangKyDeTai { MaSinhVien = "SV001", MaDeTai = 9, NgayDangKy = new DateTime(2026, 8, 25), LyDoDangKy = "Em muốn làm phân tích dữ liệu.", TrangThai = TrangThaiDangKy.DaHuy });
        db.DangKyDeTais.AddRange(dangKys);
        db.SaveChanges();

        // 4) Phân công cho các đăng ký đã duyệt
        //    SV001 có lịch sử đổi giảng viên: GV2 (Đã thay đổi) -> GV1 (Đang hướng dẫn)
        var phanCongs = new List<PhanCongHuongDan>();
        foreach (var dk in dangKys.Where(d => d.TrangThai == TrangThaiDangKy.DaDuyet))
        {
            if (dk.MaSinhVien == "SV001")
            {
                phanCongs.Add(new PhanCongHuongDan { MaDangKy = dk.MaDangKy, MaGiangVien = 2, NgayBatDau = new DateTime(2026, 9, 20), NgayKetThuc = new DateTime(2026, 10, 5), TrangThai = TrangThaiPhanCong.DaThayDoi, GhiChu = "Đổi giảng viên theo nguyện vọng." });
                phanCongs.Add(new PhanCongHuongDan { MaDangKy = dk.MaDangKy, MaGiangVien = 1, NgayBatDau = new DateTime(2026, 10, 5), TrangThai = TrangThaiPhanCong.DangHuongDan });
            }
            else
            {
                int maGv = int.Parse(dk.MaSinhVien.Substring(2)) % 11 + 1;   // tránh GV thứ 12 (tạm ngừng)
                phanCongs.Add(new PhanCongHuongDan { MaDangKy = dk.MaDangKy, MaGiangVien = maGv, NgayBatDau = dk.NgayDangKy.AddDays(14), TrangThai = TrangThaiPhanCong.DangHuongDan });
            }
        }
        db.PhanCongHuongDans.AddRange(phanCongs);
        db.SaveChanges();
    }
}

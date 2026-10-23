
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn.Models;

namespace QuanLyDeTaiDoAn.Data
{
    public static class Module2SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.LinhVucs.AnyAsync())
                return;

            var linhVucs = new List<LinhVuc>
            {
                new() { TenLinhVuc = "Công nghệ phần mềm", TrangThai = "Hoạt động" },
                new() { TenLinhVuc = "Trí tuệ nhân tạo", TrangThai = "Hoạt động" },
                new() { TenLinhVuc = "An toàn thông tin", TrangThai = "Hoạt động" },
                new() { TenLinhVuc = "Mạng máy tính", TrangThai = "Hoạt động" },
                new() { TenLinhVuc = "Hệ thống thông tin", TrangThai = "Hoạt động" }
            };

            context.LinhVucs.AddRange(linhVucs);
            await context.SaveChangesAsync();

            var giangViens = new List<GiangVien>();

            for (int i = 1; i <= 12; i++)
            {
                giangViens.Add(new GiangVien
                {
                    HoTen = $"Giảng viên mẫu {i}",
                    HocVi = i % 3 == 0 ? "Tiến sĩ" : "Thạc sĩ",
                    Email = $"giangvien{i}@example.com",
                    SoDienThoai = $"090000{i:0000}",
                    MaLinhVuc = linhVucs[(i - 1) % 5].MaLinhVuc,
                    SoSinhVienHuongDanToiDa = 5,
                    TrangThai = "Đang công tác"
                });
            }

            context.GiangViens.AddRange(giangViens);
            await context.SaveChangesAsync();

            var deTais = new List<DeTai>();

            for (int i = 1; i <= 20; i++)
            {
                deTais.Add(new DeTai
                {
                    TenDeTai = $"Đề tài đồ án mẫu {i}",
                    MaLinhVuc = linhVucs[(i - 1) % 5].MaLinhVuc,
                    MoTa = $"Mô tả đề tài số {i}",
                    YeuCau = "Sinh viên có kiến thức lập trình cơ bản",
                    SoSinhVienToiDa = 3,
                    NamHoc = "2026-2027",
                    HocKy = i % 2 == 0 ? 2 : 1,
                    TrangThai = "Mở đăng ký"
                });
            }

            context.DeTais.AddRange(deTais);
            await context.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data
{
    public static class Module2SeedData
    {
        public static async Task SeedAsync(
            ApplicationDbContext db)
        {
            if (!await db.LinhVucs.AnyAsync())
            {
                db.LinhVucs.AddRange(
                    new LinhVuc
                    {
                        TenLinhVuc = "Công nghệ phần mềm",
                        TrangThai = true
                    },
                    new LinhVuc
                    {
                        TenLinhVuc = "Trí tuệ nhân tạo",
                        TrangThai = true
                    },
                    new LinhVuc
                    {
                        TenLinhVuc = "An toàn thông tin",
                        TrangThai = true
                    },
                    new LinhVuc
                    {
                        TenLinhVuc = "Mạng máy tính",
                        TrangThai = true
                    },
                    new LinhVuc
                    {
                        TenLinhVuc = "Hệ thống thông tin",
                        TrangThai = true
                    }
                );

                await db.SaveChangesAsync();
            }

            var linhVucs = await db.LinhVucs
                .OrderBy(x => x.MaLinhVuc)
                .Take(5)
                .ToListAsync();

            if (linhVucs.Count == 0)
                return;

            if (!await db.GiangViens.AnyAsync())
            {
                for (int i = 1; i <= 12; i++)
                {
                    db.GiangViens.Add(new GiangVien
                    {
                        HoTen = $"Giảng viên mẫu {i}",
                        HocVi = i % 3 == 0
                            ? "Tiến sĩ" : "Thạc sĩ",
                        Email = $"gvmodule2_{i}@example.com",
                        SoDienThoai = $"090000{i:0000}",
                        MaLinhVuc = linhVucs[
                            (i - 1) % linhVucs.Count].MaLinhVuc,
                        SoSinhVienHuongDanToiDa = 5,
                        TrangThai = "Đang công tác"
                    });
                }

                await db.SaveChangesAsync();
            }

            if (!await db.DeTais.AnyAsync())
            {
                for (int i = 1; i <= 20; i++)
                {
                    db.DeTais.Add(new DeTai
                    {
                        TenDeTai = $"Đề tài mẫu {i}",
                        MaLinhVuc = linhVucs[
                            (i - 1) % linhVucs.Count].MaLinhVuc,
                        MoTa = $"Mô tả đề tài {i}",
                        YeuCau = "Kiến thức lập trình cơ bản",
                        SoSinhVienToiDa = 3,
                        NamHoc = "2026-2027",
                        HocKy = i % 2 == 0 ? 2 : 1,
                        TrangThai = "Mở đăng ký"
                    });
                }

                await db.SaveChangesAsync();
            }
        }
    }
}

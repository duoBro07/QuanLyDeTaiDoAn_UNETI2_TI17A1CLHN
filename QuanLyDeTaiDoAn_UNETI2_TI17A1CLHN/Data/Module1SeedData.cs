
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data
{
    public static class Module1SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.TaiKhoans.AnyAsync())
                return;

            var admin = new TaiKhoan
            {
                TenDangNhap = "admin",
                HoTen = "Quản trị viên",
                Email = "admin@example.com",
                VaiTro = "Admin",
                TrangThai = true
            };

            admin.MatKhau = new PasswordHasher<TaiKhoan>()
                .HashPassword(admin, "Admin@123456");

            db.TaiKhoans.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}

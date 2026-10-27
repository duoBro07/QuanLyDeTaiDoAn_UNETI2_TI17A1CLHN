
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuanLyDeTaiDoAn.Models;

namespace QuanLyDeTaiDoAn.Data
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
                TrangThai = "Hoạt động"
            };

            admin.MatKhau = new PasswordHasher<TaiKhoan>()
                .HashPassword(admin, "Admin@123456");

            db.TaiKhoans.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}

// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Cấu hình quan hệ cho SinhVien, DangKyDeTai (gọi 1 dòng trong AppDbContext.OnModelCreating)
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

public static class Module3Config
{
    public static void CauHinhModule3(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SinhVien>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
            e.HasOne(s => s.TaiKhoan)
             .WithOne()
             .HasForeignKey<SinhVien>(s => s.MaTaiKhoan)
             .OnDelete(DeleteBehavior.Restrict);
        });

      
        modelBuilder.Entity<DangKyDeTai>(e =>
        {
            e.HasOne(d => d.SinhVien)
             .WithMany(s => s.DangKyDeTais)
             .HasForeignKey(d => d.MaSinhVien)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(d => d.DeTai)
             .WithMany(t => t.DangKyDeTais)
             .HasForeignKey(d => d.MaDeTai)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

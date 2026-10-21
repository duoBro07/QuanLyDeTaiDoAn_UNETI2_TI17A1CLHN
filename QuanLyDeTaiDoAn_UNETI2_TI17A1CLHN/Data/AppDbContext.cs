// FILE TẠM - khi ghép nhóm dùng AppDbContext của nhóm, chỉ cần thêm:
//   public DbSet<SinhVien> SinhViens { get; set; }
//   public DbSet<DangKyDeTai> DangKyDeTais { get; set; }
//   và trong OnModelCreating:  modelBuilder.CauHinhModule3();
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- Module 3 ---
    public DbSet<SinhVien> SinhViens { get; set; }
    public DbSet<DangKyDeTai> DangKyDeTais { get; set; }

    // --- Của người khác (tạm) ---
    public DbSet<TaiKhoan> TaiKhoans { get; set; }
    public DbSet<LinhVuc> LinhVucs { get; set; }
    public DbSet<GiangVien> GiangViens { get; set; }
    public DbSet<DeTai> DeTais { get; set; }
    public DbSet<PhanCongHuongDan> PhanCongHuongDans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.CauHinhModule3();

        // Tạm: tất cả khóa ngoại Restrict để tránh lỗi "multiple cascade paths" của SQL Server
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}

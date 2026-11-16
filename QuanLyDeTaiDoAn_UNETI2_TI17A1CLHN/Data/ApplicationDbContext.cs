
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // MODULE 1
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<LinhVuc> LinhVucs { get; set; } = null!;

        // MODULE 2
        public DbSet<GiangVien> GiangViens { get; set; } = null!;
        public DbSet<DeTai> DeTais { get; set; } = null!;

        // MODULE 3
        public DbSet<SinhVien> SinhViens { get; set; } = null!;
        public DbSet<DangKyDeTai> DangKyDeTais { get; set; } = null!;

        // MODULE 4
        public DbSet<PhanCongHuongDan> PhanCongHuongDans { get; set; }
            = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TAI KHOAN
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.HasKey(x => x.MaTaiKhoan);

                entity.HasIndex(x => x.TenDangNhap)
                    .IsUnique();

                entity.Property(x => x.TenDangNhap)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.VaiTro)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.TrangThai)
                    .HasDefaultValue(true);

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_TaiKhoan_VaiTro",
                        "[VaiTro] IN ('Admin', 'SinhVien')"));
            });

            // LINH VUC
            modelBuilder.Entity<LinhVuc>(entity =>
            {
                entity.HasKey(x => x.MaLinhVuc);

                entity.HasIndex(x => x.TenLinhVuc)
                    .IsUnique();

                entity.Property(x => x.TenLinhVuc)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // GIANG VIEN
            modelBuilder.Entity<GiangVien>(entity =>
            {
                entity.HasKey(x => x.MaGiangVien);

                entity.HasOne(x => x.LinhVuc)
                    .WithMany(x => x.GiangViens)
                    .HasForeignKey(x => x.MaLinhVuc)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DE TAI
            modelBuilder.Entity<DeTai>(entity =>
            {
                entity.HasKey(x => x.MaDeTai);

                entity.HasOne(x => x.LinhVuc)
                    .WithMany(x => x.DeTais)
                    .HasForeignKey(x => x.MaLinhVuc)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SINH VIEN
            modelBuilder.Entity<SinhVien>(entity =>
            {
                entity.HasKey(x => x.MaSinhVien);

                entity.HasIndex(x => x.MaTaiKhoan)
                    .IsUnique();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.HasOne(x => x.TaiKhoan)
                    .WithOne()
                    .HasForeignKey<SinhVien>(x => x.MaTaiKhoan)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DANG KY DE TAI
            modelBuilder.Entity<DangKyDeTai>(entity =>
            {
                entity.HasKey(x => x.MaDangKy);

                // SU DUNG DUNG NAVIGATION PROPERTY
                entity.HasOne(x => x.SinhVien)
                    .WithMany(x => x.DangKyDeTais)
                    .HasForeignKey(x => x.MaSinhVien)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DeTai)
                    .WithMany(x => x.DangKyDeTais)
                    .HasForeignKey(x => x.MaDeTai)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PHAN CONG HUONG DAN
            modelBuilder.Entity<PhanCongHuongDan>(entity =>
            {
                entity.HasOne(x => x.DangKyDeTai)
                    .WithMany()
                    .HasForeignKey(x => x.MaDangKy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.GiangVien)
                    .WithMany()
                    .HasForeignKey(x => x.MaGiangVien)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.MaDangKy)
                    .IsUnique()
                    .HasFilter("[DangHieuLuc] = 1");
            });
        }
    }
}

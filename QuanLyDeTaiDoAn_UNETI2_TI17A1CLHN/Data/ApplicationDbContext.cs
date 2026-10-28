
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

        public DbSet<TaiKhoan> TaiKhoans { get; set; }

        public DbSet<LinhVuc> LinhVucs { get; set; }

        public DbSet<GiangVien> GiangViens { get; set; }

        public DbSet<DeTai> DeTais { get; set; }

        public DbSet<DangKyDeTai> DangKyDeTais { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LinhVuc>()
                .HasIndex(x => x.TenLinhVuc)
                .IsUnique();

            modelBuilder.Entity<GiangVien>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<DeTai>()
                .HasIndex(x => new
                {
                    x.TenDeTai,
                    x.NamHoc,
                    x.HocKy
                })
                .IsUnique();

            modelBuilder.Entity<GiangVien>()
                .HasOne(x => x.LinhVuc)
                .WithMany(x => x.GiangViens)
                .HasForeignKey(x => x.MaLinhVuc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeTai>()
                .HasOne(x => x.LinhVuc)
                .WithMany(x => x.DeTais)
                .HasForeignKey(x => x.MaLinhVuc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DangKyDeTai>()
                .HasOne(x => x.DeTai)
                .WithMany(x => x.DangKyDeTais)
                .HasForeignKey(x => x.MaDeTai)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

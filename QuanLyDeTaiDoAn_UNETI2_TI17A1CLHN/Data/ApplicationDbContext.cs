using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn.Models;

namespace QuanLyDeTaiDoAn.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LinhVuc> LinhVucs { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<DeTai> DeTais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<GiangVien>()
                .HasOne(g => g.LinhVuc)
                .WithMany(l => l.GiangViens)
                .HasForeignKey(g => g.MaLinhVuc)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<DeTai>()
                .HasOne(d => d.LinhVuc)
                .WithMany(l => l.DeTais)
                .HasForeignKey(d => d.MaLinhVuc)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GiangVien>()
                .HasIndex(g => g.Email)
                .IsUnique();
        }
    }
}
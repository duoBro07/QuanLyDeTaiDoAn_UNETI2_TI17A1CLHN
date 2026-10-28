
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

        // MODULE 1 - Giữ nguyên
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LinhVuc> LinhVucs { get; set; }

        // MODULE 2 - Thêm mới
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<DeTai> DeTais { get; set; }
    }
}

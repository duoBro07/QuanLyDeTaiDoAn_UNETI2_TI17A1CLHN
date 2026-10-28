
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class DangKyDeTai
    {
        [Key]
        public int MaDangKy { get; set; }

        [Range(1, int.MaxValue)]
        public int MaSinhVien { get; set; }

        [Range(1, int.MaxValue)]
        public int MaDeTai { get; set; }

        public DateTime NgayDangKy { get; set; }
            = DateTime.Now;

        public string? LyDoDangKy { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }
            = "Chờ duyệt";

        public string? GhiChuDuyet { get; set; }

        [StringLength(30)]
        public string TrangThaiThucHien { get; set; }
            = "Chưa bắt đầu";

        public DeTai? DeTai { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class DangKyDeTai
    {
        [Key]
        public int MaDangKy { get; set; }

        public int MaSinhVien { get; set; }
        public SinhVien? SinhVien { get; set; }

        public int MaDeTai { get; set; }
        public DeTai? DeTai { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập lý do đăng ký")]
        [StringLength(1000)]
        public string LyDoDangKy { get; set; } = "";

        public string TrangThai { get; set; } = "Chờ duyệt";

        [StringLength(1000)]
        public string? GhiChuDuyet { get; set; }
    }
}

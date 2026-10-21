
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn.Models
{
    // Model tạm phục vụ kiểm thử Module 2
    public class DangKyDeTaiModule2
    {
        [Key]
        public int MaDangKy { get; set; }

        [Required]
        public int MaDeTai { get; set; }

        [Required]
        public int MaSinhVien { get; set; }

        [Required]
        public string TrangThai { get; set; } = "Chờ duyệt";

        public DeTai? DeTai { get; set; }
    }
}

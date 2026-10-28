
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string MatKhau { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = "";

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(20)]
        public string VaiTro { get; set; } = "SinhVien";

        public bool TrangThai { get; set; } = true;
    }
}

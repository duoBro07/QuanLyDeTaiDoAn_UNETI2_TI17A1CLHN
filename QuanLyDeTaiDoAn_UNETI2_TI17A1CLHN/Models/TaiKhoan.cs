
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required]
        public string MatKhau { get; set; } = "";

        [Required, StringLength(100)]
        public string HoTen { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string VaiTro { get; set; } = "Sinh viên";

        [Required]
        public string TrangThai { get; set; } = "Hoạt động";
    }
}

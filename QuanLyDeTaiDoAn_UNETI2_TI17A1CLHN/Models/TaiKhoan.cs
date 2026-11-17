
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string HoTen { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [RegularExpression(
            "^(Admin|SinhVien)$",
            ErrorMessage = "Vai trò chỉ được là Admin hoặc SinhVien.")]
        [StringLength(20)]
        public string VaiTro { get; set; } = "SinhVien";

        public bool TrangThai { get; set; } = true;
    }
}

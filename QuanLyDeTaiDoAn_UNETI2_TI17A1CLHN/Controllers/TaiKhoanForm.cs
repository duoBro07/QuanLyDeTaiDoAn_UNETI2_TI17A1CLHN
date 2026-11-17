
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class TaiKhoanForm
    {
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string HoTen { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [RegularExpression(
            "^(Admin|SinhVien)$",
            ErrorMessage = "Chỉ được chọn Admin hoặc SinhVien.")]
        public string VaiTro { get; set; } = "SinhVien";

        public bool TrangThai { get; set; } = true;

        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }
    }
}

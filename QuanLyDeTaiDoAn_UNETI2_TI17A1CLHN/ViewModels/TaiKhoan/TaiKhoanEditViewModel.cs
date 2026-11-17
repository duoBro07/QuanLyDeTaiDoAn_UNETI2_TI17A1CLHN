using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels.TaiKhoan
{
    public class TaiKhoanEditViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [RegularExpression("^(Admin|SinhVien)$", ErrorMessage = "Vai trò không hợp lệ")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "SinhVien";

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string? MatKhauMoi { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu mới không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? XacNhanMatKhauMoi { get; set; }

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; }
    }
}

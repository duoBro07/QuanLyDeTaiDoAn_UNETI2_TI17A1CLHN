using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels.TaiKhoan
{
    public class TaiKhoanCreateViewModel
    {
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

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare(nameof(MatKhau), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;
    }
}

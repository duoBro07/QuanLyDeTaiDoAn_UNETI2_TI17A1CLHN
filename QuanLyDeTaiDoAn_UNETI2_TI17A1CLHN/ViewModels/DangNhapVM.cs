// FILE TẠM - xóa khi ghép với Module 1
using System.ComponentModel.DataAnnotations;
namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

public class DangNhapVM
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")] public string? TenDangNhap { get; set; }
    [Required(ErrorMessage = "Nhập mật khẩu")] public string? MatKhau { get; set; }
}

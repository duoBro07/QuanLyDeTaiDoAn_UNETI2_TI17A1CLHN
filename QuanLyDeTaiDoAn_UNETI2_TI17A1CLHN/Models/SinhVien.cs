// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Entity SinhVien (Module 3 - Quản lý sinh viên và đăng ký đề tài)
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

public class SinhVien
{
    [Key]
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã sinh viên tối đa 20 ký tự")]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = "";

    // FK -> TaiKhoan (quan hệ 1-1). Không cho form gửi giá trị này lên
    [BindNever]
    public int MaTaiKhoan { get; set; }

    [ValidateNever]
    public TaiKhoan? TaiKhoan { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [StringLength(10)]
    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [Required(ErrorMessage = "Lớp là bắt buộc")]
    [StringLength(30)]
    [Display(Name = "Lớp")]
    public string Lop { get; set; } = "";

    [Required(ErrorMessage = "Khoa là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Khoa")]
    public string Khoa { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Chuyên ngành")]
    public string? ChuyenNganh { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (ví dụ 0987654321)")]
    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiSinhVien.HoatDong;

    [ValidateNever]
    public ICollection<DangKyDeTai> DangKyDeTais { get; set; } = new List<DangKyDeTai>();
}

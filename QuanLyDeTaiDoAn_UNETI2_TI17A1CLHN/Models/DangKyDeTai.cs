// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Entity DangKyDeTai (Module 3 - hồ sơ đăng ký, tách riêng với PhanCongHuongDan)
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

public class DangKyDeTai
{
    [Key]
    public int MaDangKy { get; set; }

    [Required, StringLength(20)]
    public string MaSinhVien { get; set; } = "";
    [ValidateNever]
    public SinhVien? SinhVien { get; set; }

    public int MaDeTai { get; set; }
    [ValidateNever]
    public DeTai? DeTai { get; set; }

   
    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do đăng ký")]
    [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
    [Display(Name = "Lý do đăng ký")]
    public string LyDoDangKy { get; set; } = "";

    [Required, StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

    [StringLength(500)]
    [Display(Name = "Ghi chú duyệt")]
    public string? GhiChuDuyet { get; set; }

    [ValidateNever]
    public ICollection<PhanCongHuongDan> PhanCongs { get; set; } = new List<PhanCongHuongDan>();
}

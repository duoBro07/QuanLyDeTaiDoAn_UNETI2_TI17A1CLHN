// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: ViewModel cho các màn hình Module 3
using System.ComponentModel.DataAnnotations;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

public class PhanTrangVM
{
    public int Trang { get; set; } = 1;
    public int TongTrang { get; set; } = 1;
    public int Tong { get; set; }
    public int SoHienThi { get; set; }
}

// Một dòng đề tài trong danh sách tra cứu. SoChoCon được TÍNH, không lưu trong database
public class DeTaiTraCuuVM
{
    public int MaDeTai { get; set; }
    public string TenDeTai { get; set; } = "";
    public string TenLinhVuc { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public int HocKy { get; set; }
    public int SoSinhVienToiDa { get; set; }
    public int SoDaDangKy { get; set; }
    public string TrangThai { get; set; } = "";
    public int SoChoCon => SoSinhVienToiDa - SoDaDangKy;
}

public class DeTaiChiTietVM
{
    public DeTai DeTai { get; set; } = null!;
    public string TenLinhVuc { get; set; } = "";
    public int SoDaDangKy { get; set; }
    public int SoChoCon => DeTai.SoSinhVienToiDa - SoDaDangKy;
    public List<string> GiangVienCungLinhVuc { get; set; } = new();
    public string? LyDoKhongDangKy { get; set; }
    public bool CoTheDangKy => LyDoKhongDangKy == null;
}

public class TrangChuSinhVienVM
{
    public SinhVien SinhVien { get; set; } = null!;
    public int TongDeTaiCoTheDangKy { get; set; }
    public int DaDangKy { get; set; }
    public int ChoDuyet { get; set; }
    public int DaDuyet { get; set; }
    public List<DeTaiTraCuuVM> DeTaiNoiBat { get; set; } = new();
}

public class DangKyFormVM
{
    public int MaDeTai { get; set; }
    public string? TenDeTai { get; set; }
    public string? TenLinhVuc { get; set; }
    public string? ThongBaoKhongDangKy { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do đăng ký")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do đăng ký từ 10 đến 500 ký tự")]
    [Display(Name = "Lý do đăng ký")]
    public string? LyDoDangKy { get; set; }
}

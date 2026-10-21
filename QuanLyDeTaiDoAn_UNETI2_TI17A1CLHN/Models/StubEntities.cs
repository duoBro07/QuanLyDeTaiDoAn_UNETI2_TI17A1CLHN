// ============================================================================
// FILE TẠM: chỉ để Module 3 chạy độc lập. KHI GHÉP NHÓM: XÓA FILE NÀY và dùng Entity thật của
// SV1 (TaiKhoan, LinhVuc), SV2 (GiangVien, DeTai), SV4 (PhanCongHuongDan).
// Chỉ cần đảm bảo Entity thật có các thuộc tính mà code Module 3 đang dùng (xem README).
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

public class TaiKhoan
{
    [Key] public int MaTaiKhoan { get; set; }
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required, StringLength(200)] public string MatKhau { get; set; } = "";
    [StringLength(100)] public string HoTen { get; set; } = "";
    [StringLength(100)] public string Email { get; set; } = "";
    [Required, StringLength(20)] public string VaiTro { get; set; } = "";
    public bool TrangThai { get; set; } = true;     // true = hoạt động
}

public class LinhVuc
{
    [Key] public int MaLinhVuc { get; set; }
    [Required, StringLength(100)] public string TenLinhVuc { get; set; } = "";
    public string? MoTa { get; set; }
}

public class GiangVien
{
    [Key] public int MaGiangVien { get; set; }
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [StringLength(50)] public string? HocVi { get; set; }
    [StringLength(100)] public string? Email { get; set; }
    [StringLength(15)] public string? SoDienThoai { get; set; }
    public int MaLinhVuc { get; set; }
    public LinhVuc? LinhVuc { get; set; }
    public int SoSinhVienToiDa { get; set; } = 3;
    [StringLength(30)] public string TrangThai { get; set; } = "Đang công tác";
}

public class DeTai
{
    [Key] public int MaDeTai { get; set; }
    [Required, StringLength(200)] public string TenDeTai { get; set; } = "";
    public int MaLinhVuc { get; set; }
    public LinhVuc? LinhVuc { get; set; }
    public string? MoTa { get; set; }
    public string? YeuCau { get; set; }
    public int SoSinhVienToiDa { get; set; } = 1;
    [Required, StringLength(20)] public string NamHoc { get; set; } = "";
    public int HocKy { get; set; } = 1;
    [Required, StringLength(30)] public string TrangThai { get; set; } = "Mở đăng ký";
    public ICollection<DangKyDeTai> DangKyDeTais { get; set; } = new List<DangKyDeTai>();
}

public class PhanCongHuongDan
{
    [Key] public int MaPhanCong { get; set; }
    public int MaDangKy { get; set; }
    public DangKyDeTai? DangKyDeTai { get; set; }
    public int MaGiangVien { get; set; }
    public GiangVien? GiangVien { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    [Required, StringLength(30)] public string TrangThai { get; set; } = "Đang hướng dẫn";
    [StringLength(500)] public string? GhiChu { get; set; }
}

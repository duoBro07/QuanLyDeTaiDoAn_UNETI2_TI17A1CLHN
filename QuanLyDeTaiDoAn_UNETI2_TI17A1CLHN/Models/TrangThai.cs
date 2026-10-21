// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Hằng số trạng thái dùng chung (Module 3 - cần thống nhất với cả nhóm)
namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

public static class VaiTroHeThong
{
    public const string Admin = "Admin";
    public const string SinhVien = "SinhVien";
}

public static class TrangThaiSinhVien
{
    public const string HoatDong = "Hoạt động";
    public const string Khoa = "Khóa";
}

public static class TrangThaiDangKy
{
    public const string ChoDuyet = "Chờ duyệt";
    public const string DaDuyet = "Đã duyệt";
    public const string TuChoi = "Từ chối";
    public const string DaHuy = "Đã hủy";
}

public static class TrangThaiDeTai
{
    public const string MoDangKy = "Mở đăng ký";
    public const string DuSoLuong = "Đủ số lượng";
    public const string TamKhoa = "Tạm khóa";
    public const string KetThuc = "Kết thúc";
}

public static class TrangThaiGiangVien
{
    public const string DangCongTac = "Đang công tác";
    public const string TamNgung = "Tạm ngừng hướng dẫn";
}

public static class TrangThaiPhanCong
{
    public const string DangHuongDan = "Đang hướng dẫn";
    public const string DaThayDoi = "Đã thay đổi";
    public const string KetThuc = "Kết thúc";
}

public static class UiHelper
{
   
    public static string Badge(string? trangThai) => trangThai switch
    {
        "Hoạt động" or "Đã duyệt" or "Mở đăng ký" or "Đang hướng dẫn" => "bg-success",
        "Chờ duyệt" => "bg-warning text-dark",
        "Từ chối" or "Khóa" or "Tạm khóa" => "bg-danger",
        "Đã hủy" or "Kết thúc" or "Đủ số lượng" or "Đã thay đổi" => "bg-secondary",
        _ => "bg-light text-dark"
    };
}

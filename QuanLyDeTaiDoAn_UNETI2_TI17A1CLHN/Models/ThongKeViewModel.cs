
using System.Collections.Generic;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

public class ThongKeViewModel
{
    public string NamHoc { get; set; } = "";

    public int? HocKy { get; set; }

    public List<string> DanhSachNamHoc { get; set; } = new();

    // Các chỉ số tổng quan
    public int TongSinhVien { get; set; }
    public int TongDeTai { get; set; }
    public int TongGiangVien { get; set; }
    public int TongDangKy { get; set; }

    // Trạng thái đăng ký
    public int ChoDuyet { get; set; }
    public int DaDuyet { get; set; }
    public int TuChoi { get; set; }

    // Phân công và số chỗ
    public int DangHuongDan { get; set; }
    public int DeTaiConCho { get; set; }
    public int DeTaiHetCho { get; set; }
    public int GiangVienDayTai { get; set; }

    // Dữ liệu biểu đồ
    public List<ThongKeDong> TheoLinhVuc { get; set; } = new();
    public List<ThongKeDong> TheoTrangThai { get; set; } = new();
    public List<ThongKeDong> TheoThang { get; set; } = new();
    public List<ThongKeDong> TheoNamHoc { get; set; } = new();

    // Dữ liệu bảng
    public List<ThongKeDeTaiDong> TheoDeTai { get; set; } = new();
    public List<ThongKeGiangVienDong> TheoGiangVien { get; set; } = new();
}

public class ThongKeDong
{
    public string Ten { get; set; } = "";
    public int SoLuong { get; set; }
}

public class ThongKeDeTaiDong
{
    public string Ten { get; set; } = "";
    public string LinhVuc { get; set; } = "";
    public string NamHoc { get; set; } = "";

    public int HocKy { get; set; }
    public int SoSinhVien { get; set; }
    public int SoChoCon { get; set; }
    public int ToiDa { get; set; }
}

public class ThongKeGiangVienDong
{
    public string Ten { get; set; } = "";
    public string LinhVuc { get; set; } = "";

    public int DangHuongDan { get; set; }
    public int ToiDa { get; set; }
}

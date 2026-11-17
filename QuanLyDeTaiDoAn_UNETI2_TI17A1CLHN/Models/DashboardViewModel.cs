
using System.Collections.Generic;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class DashboardViewModel
    {
        // Tổng quan hệ thống
        public int TongTaiKhoan { get; set; }
        public int TongLinhVuc { get; set; }
        public int TongSinhVien { get; set; }
        public int TongGiangVien { get; set; }
        public int TongDeTai { get; set; }

        // Đăng ký đề tài
        public int DangKyChoDuyet { get; set; }
        public int DangKyDaDuyet { get; set; }

        // Sức chứa đề tài
        public int DeTaiConCho { get; set; }
        public int DeTaiDuSoLuong { get; set; }

        // Phân công giảng viên
        public int SinhVienCoGVHD { get; set; }
        public int GiangVienDatGioiHan { get; set; }

        // Dữ liệu biểu đồ
        public List<ThongKeLinhVuc> DeTaiTheoLinhVuc
        { get; set; } = new();

        public List<ThongKeNamHoc> DangKyTheoNamHoc
        { get; set; } = new();

        // Danh sách đề tài mới nhất
        public List<DeTaiMoiNhat> DeTaiMoiNhat
        { get; set; } = new();
    }

    public class ThongKeLinhVuc
    {
        public string TenLinhVuc { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class ThongKeNamHoc
    {
        public string NamHoc { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class DeTaiMoiNhat
    {
        public int MaDeTai { get; set; }
        public string TenDeTai { get; set; } = "";
        public string TenLinhVuc { get; set; } = "";
        public string NamHoc { get; set; } = "";
        public int HocKy { get; set; }
        public string TrangThai { get; set; } = "";
    }
}


using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class PhanCongHuongDan
    {
        [Key]
        public int MaPhanCong { get; set; }

        public int MaDangKy { get; set; }
        public DangKyDeTai? DangKyDeTai { get; set; }

        public int MaGiangVien { get; set; }
        public GiangVien? GiangVien { get; set; }

        public DateTime NgayPhanCong { get; set; } = DateTime.Now;

        public DateTime? NgayKetThuc { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public string TrangThaiThucHien { get; set; } = "Chưa bắt đầu";

        public bool DangHieuLuc { get; set; } = true;
    }
}

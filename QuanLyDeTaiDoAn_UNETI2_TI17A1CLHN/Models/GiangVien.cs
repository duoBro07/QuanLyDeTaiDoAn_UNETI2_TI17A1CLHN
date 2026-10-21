using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn.Models
{
    public class GiangVien
    {
        [Key]
        [Display(Name = "Mã giảng viên")]
        public int MaGiangVien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập học vị")]
        [StringLength(50)]
        [Display(Name = "Học vị")]
        public string HocVi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn lĩnh vực")]
        [Display(Name = "Lĩnh vực")]
        public int MaLinhVuc { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage =
                "Số sinh viên hướng dẫn tối đa phải lớn hơn 0")]
        [Display(Name = "Số sinh viên hướng dẫn tối đa")]
        public int SoSinhVienHuongDanToiDa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang công tác";

        public LinhVuc? LinhVuc { get; set; }
    }
}
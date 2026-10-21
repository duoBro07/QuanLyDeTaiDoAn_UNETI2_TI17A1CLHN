using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn.Models
{
    public class DeTai
    {
        [Key]
        [Display(Name = "Mã đề tài")]
        public int MaDeTai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đề tài")]
        [StringLength(200)]
        [Display(Name = "Tên đề tài")]
        public string TenDeTai { get; set; } = "";

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn lĩnh vực")]
        [Display(Name = "Lĩnh vực")]
        public int MaLinhVuc { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Yêu cầu")]
        public string? YeuCau { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage =
                "Số sinh viên tối đa phải lớn hơn 0")]
        [Display(Name = "Số sinh viên tối đa")]
        public int SoSinhVienToiDa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm học")]
        [StringLength(20)]
        [Display(Name = "Năm học")]
        public string NamHoc { get; set; } = "";

        [Range(1, 3, ErrorMessage = "Vui lòng chọn học kỳ")]
        [Display(Name = "Học kỳ")]
        public int HocKy { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Mở đăng ký";

        public LinhVuc? LinhVuc { get; set; }
    }
}
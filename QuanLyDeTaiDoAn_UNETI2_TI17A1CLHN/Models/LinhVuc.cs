using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn.Models
{
    public class LinhVuc
    {
        [Key]
        public int MaLinhVuc { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên lĩnh vực")]
        public string TenLinhVuc { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        public ICollection<GiangVien>? GiangViens { get; set; }

        public ICollection<DeTai>? DeTais { get; set; }
    }
}
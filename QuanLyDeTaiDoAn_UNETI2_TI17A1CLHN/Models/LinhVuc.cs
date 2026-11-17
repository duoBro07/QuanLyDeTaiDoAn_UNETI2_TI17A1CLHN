
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class LinhVuc
    {
        [Key]
        public int MaLinhVuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lĩnh vực")]
        [StringLength(100)]
        [Display(Name = "Tên lĩnh vực")]
        public string TenLinhVuc { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<GiangVien> GiangViens { get; set; }
            = new List<GiangVien>();

        public ICollection<DeTai> DeTais { get; set; }
            = new List<DeTai>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class LinhVuc
    {
        [Key]
        public int MaLinhVuc { get; set; }

        [Required(ErrorMessage = "Tên lĩnh vực không được để trống")]
        [StringLength(100)]
        public string TenLinhVuc { get; set; } = null!;

        [StringLength(500)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}
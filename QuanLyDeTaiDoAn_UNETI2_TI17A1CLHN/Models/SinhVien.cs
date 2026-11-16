
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public class SinhVien
    {
        [Key]
        public int MaSinhVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài khoản.")]
        [Display(Name = "Tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = "";

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lớp.")]
        [StringLength(50)]
        public string Lop { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập khoa.")]
        [StringLength(100)]
        public string Khoa { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập chuyên ngành.")]
        [StringLength(100)]
        public string ChuyenNganh { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<DangKyDeTai> DangKyDeTais { get; set; }
            = new List<DangKyDeTai>();
    }
}

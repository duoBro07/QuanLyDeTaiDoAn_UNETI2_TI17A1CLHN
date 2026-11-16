
namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models
{
    public static class VaiTroHeThong
    {
        public const string Admin = "Admin";
        public const string SinhVien = "SinhVien";

        public static bool HopLe(string? vaiTro)
        {
            return vaiTro == Admin || vaiTro == SinhVien;
        }
    }
}

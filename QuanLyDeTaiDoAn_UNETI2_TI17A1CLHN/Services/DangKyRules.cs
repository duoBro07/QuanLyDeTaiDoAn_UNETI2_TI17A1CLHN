// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Quy tắc nghiệp vụ đăng ký đề tài (kiểm tra bằng LINQ) và truy vấn tính số chỗ còn
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Services;

public static class DangKyRules
{
    
    public const string NamHocHienTai = "2026-2027";
    public const int HocKyHienTai = 1;

    
    public static Task<int> DemDaDuyet(AppDbContext db, int maDeTai) =>
        db.DangKyDeTais.CountAsync(d => d.MaDeTai == maDeTai && d.TrangThai == TrangThaiDangKy.DaDuyet);

    
    public static async Task<string?> KiemTra(AppDbContext db, SinhVien sv, DeTai dt)
    {
        if (sv.TrangThai != TrangThaiSinhVien.HoatDong)
            return "Tài khoản sinh viên đang bị khóa, không thể đăng ký.";

        if (dt.TrangThai != TrangThaiDeTai.MoDangKy)
            return $"Đề tài hiện không mở đăng ký (trạng thái: {dt.TrangThai}).";

        if (dt.NamHoc != NamHocHienTai || dt.HocKy != HocKyHienTai)
            return $"Đề tài thuộc năm học {dt.NamHoc}, học kỳ {dt.HocKy}, không nằm trong đợt đăng ký hiện tại ({NamHocHienTai}, học kỳ {HocKyHienTai}).";

        int daNhan = await DemDaDuyet(db, dt.MaDeTai);
        if (daNhan >= dt.SoSinhVienToiDa)
            return "Đề tài đã đủ số lượng sinh viên.";

        bool daCoDeTai = await db.DangKyDeTais.AnyAsync(d =>
            d.MaSinhVien == sv.MaSinhVien
            && d.TrangThai == TrangThaiDangKy.DaDuyet
            && d.DeTai!.NamHoc == dt.NamHoc
            && d.DeTai.HocKy == dt.HocKy);
        if (daCoDeTai)
            return "Bạn đã có một đề tài được duyệt trong đợt này, không thể đăng ký thêm.";

        bool trung = await db.DangKyDeTais.AnyAsync(d =>
            d.MaSinhVien == sv.MaSinhVien
            && d.MaDeTai == dt.MaDeTai
            && d.TrangThai == TrangThaiDangKy.ChoDuyet);
        if (trung)
            return "Bạn đã đăng ký đề tài này và đang chờ duyệt.";

        return null;
    }

    
    public static IQueryable<DeTaiTraCuuVM> ToTraCuuVM(this IQueryable<DeTai> q) =>
        q.Select(d => new DeTaiTraCuuVM
        {
            MaDeTai = d.MaDeTai,
            TenDeTai = d.TenDeTai,
            TenLinhVuc = d.LinhVuc!.TenLinhVuc,
            NamHoc = d.NamHoc,
            HocKy = d.HocKy,
            SoSinhVienToiDa = d.SoSinhVienToiDa,
            TrangThai = d.TrangThai,
            SoDaDangKy = d.DangKyDeTais.Count(x => x.TrangThai == TrangThaiDangKy.DaDuyet)
        });
}

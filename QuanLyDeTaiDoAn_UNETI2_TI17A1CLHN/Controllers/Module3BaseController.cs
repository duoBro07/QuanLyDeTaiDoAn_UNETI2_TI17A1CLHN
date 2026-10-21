// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Lớp nền dùng chung cho các Controller của Module 3 (lấy sinh viên đang đăng nhập, phân trang)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers;

public abstract class Module3BaseController : Controller
{
    protected const int KichThuocTrang = 5;
    protected readonly AppDbContext _db;

    protected Module3BaseController(AppDbContext db) => _db = db;

    // Sinh viên đang đăng nhập, lấy từ Session. KHÔNG nhận mã sinh viên từ URL hay form
    protected async Task<SinhVien?> LaySinhVienHienTai()
    {
        var maTK = HttpContext.Session.GetInt32("MaTaiKhoan");
        if (maTK == null) return null;
        return await _db.SinhViens.FirstOrDefaultAsync(s => s.MaTaiKhoan == maTK.Value);
    }

    protected PhanTrangVM PhanTrang(int tong, int trang)
    {
        int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)KichThuocTrang));
        return new PhanTrangVM { Tong = tong, TongTrang = tongTrang, Trang = Math.Clamp(trang, 1, tongTrang) };
    }
}

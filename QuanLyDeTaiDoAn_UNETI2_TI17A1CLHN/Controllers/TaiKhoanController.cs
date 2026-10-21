// FILE TẠM - đăng nhập đơn giản để test Module 3. Xóa khi ghép với Module 1 (SV1 làm đăng nhập thật).
// Session key bắt buộc khớp với Module 1: "MaTaiKhoan" (int), "HoTen", "VaiTro"
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers;

public class TaiKhoanController : Controller
{
    private readonly AppDbContext _db;
    public TaiKhoanController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult DangNhap() => View(new DangNhapVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var tk = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == vm.TenDangNhap && t.MatKhau == vm.MatKhau);
        if (tk == null) { ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu."); return View(vm); }
        if (!tk.TrangThai) { ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên."); return View(vm); }

        HttpContext.Session.SetInt32("MaTaiKhoan", tk.MaTaiKhoan);
        HttpContext.Session.SetString("HoTen", tk.HoTen);
        HttpContext.Session.SetString("VaiTro", tk.VaiTro);

        return tk.VaiTro == VaiTroHeThong.Admin
            ? RedirectToAction("Index", "SinhVien")
            : RedirectToAction("TrangChu", "SinhVien");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(DangNhap));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập thì không cần vào lại Login
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(tk =>
                    tk.TenDangNhap == model.TenDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            if (taiKhoan.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa.");
                return View(model);
            }

            HttpContext.Session.SetInt32(
                "MaTaiKhoan",
                taiKhoan.MaTaiKhoan
            );

            HttpContext.Session.SetString(
                "HoTen",
                taiKhoan.HoTen
            );

            HttpContext.Session.SetString(
                "VaiTro",
                taiKhoan.VaiTro
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
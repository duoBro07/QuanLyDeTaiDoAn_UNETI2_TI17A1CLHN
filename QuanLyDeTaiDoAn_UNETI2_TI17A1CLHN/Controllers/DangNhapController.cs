
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangNhapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            if (maTaiKhoan.HasValue)
            {
                var taiKhoan = await _context.TaiKhoans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan.Value);

                if (taiKhoan != null &&
                    taiKhoan.TrangThai &&
                    LaVaiTroHopLe(taiKhoan.VaiTro))
                {
                    CapNhatSession(taiKhoan);
                    return ChuyenTheoVaiTro(taiKhoan.VaiTro);
                }

                HttpContext.Session.Clear();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string tenDangNhap,
            string matKhau,
            bool ghiNho = false)
        {
            tenDangNhap = tenDangNhap?.Trim() ?? "";
            matKhau ??= "";

            if (string.IsNullOrWhiteSpace(tenDangNhap) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                ViewBag.Loi = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

            if (taiKhoan == null)
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            if (!taiKhoan.TrangThai)
            {
                ViewBag.Loi = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.";
                return View();
            }

            var hasher = new PasswordHasher<TaiKhoan>();
            PasswordVerificationResult ketQua;

            try
            {
                ketQua = hasher.VerifyHashedPassword(
                    taiKhoan, taiKhoan.MatKhau, matKhau);
            }
            catch (FormatException)
            {
                ketQua = PasswordVerificationResult.Failed;
            }
            catch (ArgumentException)
            {
                ketQua = PasswordVerificationResult.Failed;
            }

            if (ketQua == PasswordVerificationResult.Failed)
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            if (!LaVaiTroHopLe(taiKhoan.VaiTro))
            {
                ViewBag.Loi = "Tài khoản chưa được phân quyền hợp lệ.";
                return View();
            }

            if (ketQua == PasswordVerificationResult.SuccessRehashNeeded)
            {
                taiKhoan.MatKhau = hasher.HashPassword(taiKhoan, matKhau);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Clear();
            CapNhatSession(taiKhoan);

            return ChuyenTheoVaiTro(taiKhoan.VaiTro);
        }

        private void CapNhatSession(TaiKhoan taiKhoan)
        {
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
        }

        private bool LaVaiTroHopLe(string? vaiTro)
        {
            return vaiTro == "Admin" || vaiTro == "SinhVien";
        }

        private IActionResult ChuyenTheoVaiTro(string vaiTro)
        {
            return vaiTro switch
            {
                "Admin" => RedirectToAction("Index", "Home"),
                "SinhVien" => RedirectToAction("Index", "CongSinhVien"),
                _ => RedirectToAction("Index", "DangNhap")
            };
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            // Xoa tat ca thong tin dang nhap
            HttpContext.Session.Clear();

            // Chuyen ve trang dang nhap
            return RedirectToAction("Index", "DangNhap");
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels.TaiKhoan;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    // Người thực hiện: Phạm Tùng Dương - MSSV: [điền MSSV]
    // Module 1: tài khoản, đăng nhập/đăng xuất, phân quyền.
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly string[] VaiTroHopLe = ["Admin", "SinhVien"];

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
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

            var tenDangNhap = model.TenDangNhap.Trim();

            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(tk => tk.TenDangNhap == tenDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                return View(model);
            }

            if (taiKhoan.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
                return View(model);
            }

            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [SessionAuthorize("Admin")]
        public async Task<IActionResult> Index(string? q, string? vaiTro, string? trangThai)
        {
            var query = _context.TaiKhoans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.TenDangNhap.Contains(q) ||
                    x.HoTen.Contains(q) ||
                    (x.Email != null && x.Email.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro) && VaiTroHopLe.Contains(vaiTro))
            {
                query = query.Where(x => x.VaiTro == vaiTro);
            }

            if (trangThai == "active")
            {
                query = query.Where(x => x.TrangThai);
            }
            else if (trangThai == "locked")
            {
                query = query.Where(x => !x.TrangThai);
            }

            ViewBag.Q = q;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;

            return View(await query.OrderBy(x => x.MaTaiKhoan).ToListAsync());
        }

        [SessionAuthorize("Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        [SessionAuthorize("Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TaiKhoanCreateViewModel());
        }

        [SessionAuthorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanCreateViewModel model)
        {
            if (!VaiTroHopLe.Contains(model.VaiTro))
            {
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ.");
            }

            var tenDangNhap = model.TenDangNhap.Trim();

            if (await _context.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                VaiTro = model.VaiTro,
                TrangThai = model.TrangThai
            };

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize("Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            return View(new TaiKhoanEditViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai
            });
        }

        [SessionAuthorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanEditViewModel model)
        {
            if (id != model.MaTaiKhoan) return NotFound();

            if (!VaiTroHopLe.Contains(model.VaiTro))
            {
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ.");
            }

            var tenDangNhap = model.TenDangNhap.Trim();

            if (await _context.TaiKhoans.AnyAsync(x =>
                    x.TenDangNhap == tenDangNhap && x.MaTaiKhoan != model.MaTaiKhoan))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            var maDangNhap = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maDangNhap == id && !model.TrangThai)
            {
                ModelState.AddModelError(nameof(model.TrangThai), "Bạn không thể tự khóa tài khoản đang đăng nhập.");
                return View(model);
            }

            if (maDangNhap == id && model.VaiTro != "Admin")
            {
                ModelState.AddModelError(nameof(model.VaiTro), "Bạn không thể tự hạ quyền Admin của tài khoản đang đăng nhập.");
                return View(model);
            }

            taiKhoan.TenDangNhap = tenDangNhap;
            taiKhoan.HoTen = model.HoTen.Trim();
            taiKhoan.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            taiKhoan.VaiTro = model.VaiTro;
            taiKhoan.TrangThai = model.TrangThai;

            if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
            {
                taiKhoan.MatKhau = model.MatKhauMoi;
            }

            await _context.SaveChangesAsync();

            if (maDangNhap == id)
            {
                HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
                HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            }

            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            var maDangNhap = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maDangNhap == id && taiKhoan.TrangThai)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ đổi trạng thái tài khoản. Không xóa tài khoản nên lịch sử ở Module 3/4 vẫn được giữ.
            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = taiKhoan.TrangThai
                ? "Đã mở khóa tài khoản."
                : "Đã khóa tài khoản.";

            return RedirectToAction(nameof(Index));
        }
    }
}

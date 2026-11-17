
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<TaiKhoan> _hasher = new();

        public TaiKhoanController(ApplicationDbContext db)
        {
            _db = db;
        }

        // DANH SÁCH - TÌM KIẾM - LỌC - PHÂN TRANG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa, string? vaiTro,
            string? trangThai, int trang = 1)
        {
            const int kichThuocTrang = 8;
            trang = Math.Max(1, trang);

            var query = _db.TaiKhoans.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(x =>
                    x.TenDangNhap.Contains(tuKhoa) ||
                    x.HoTen.Contains(tuKhoa) ||
                    (x.Email != null && x.Email.Contains(tuKhoa)));
            }

            if (vaiTro == "Admin" || vaiTro == "SinhVien")
                query = query.Where(x => x.VaiTro == vaiTro);

            if (trangThai == "HoatDong")
                query = query.Where(x => x.TrangThai);
            else if (trangThai == "DaKhoa")
                query = query.Where(x => !x.TrangThai);

            int tong = await query.CountAsync();
            int tongTrang = Math.Max(1,
                (int)Math.Ceiling(tong / (double)kichThuocTrang));

            trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(x => x.MaTaiKhoan)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.VaiTro = vaiTro ?? "";
            ViewBag.TrangThai = trangThai ?? "";
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.Tong = tong;

            return View(danhSach);
        }

        // THÊM - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TaiKhoanForm());
        }

        // THÊM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanForm form)
        {
            KiemTraVaiTro(form);

            if (string.IsNullOrWhiteSpace(form.MatKhau))
                ModelState.AddModelError(
                    nameof(form.MatKhau), "Vui lòng nhập mật khẩu.");

            if (ModelState.IsValid)
            {
                string ten = form.TenDangNhap.Trim();

                if (await _db.TaiKhoans.AnyAsync(
                    x => x.TenDangNhap == ten))
                {
                    ModelState.AddModelError(
                        nameof(form.TenDangNhap),
                        "Tên đăng nhập đã tồn tại.");
                }
            }

            if (!ModelState.IsValid)
                return View(form);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = form.TenDangNhap.Trim(),
                HoTen = form.HoTen.Trim(),
                Email = string.IsNullOrWhiteSpace(form.Email)
                    ? null : form.Email.Trim(),
                VaiTro = form.VaiTro,
                TrangThai = form.TrangThai
            };

            taiKhoan.MatKhau = _hasher.HashPassword(
                taiKhoan, form.MatKhau!);

            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            TempData["ThongBaoTaiKhoan"] =
                "Thêm tài khoản thành công!";

            return RedirectToAction(nameof(Index));
        }

        // CHI TIẾT
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var taiKhoan = await _db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            return View(taiKhoan);
        }

        // SỬA - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var taiKhoan = await _db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            return View(new TaiKhoanForm
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai,
                MatKhau = null
            });
        }

        // SỬA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanForm form)
        {
            if (id != form.MaTaiKhoan)
                return BadRequest();

            var taiKhoan = await _db.TaiKhoans
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            KiemTraVaiTro(form);

            if (ModelState.IsValid)
            {
                string ten = form.TenDangNhap.Trim();

                if (await _db.TaiKhoans.AnyAsync(x =>
                    x.TenDangNhap == ten && x.MaTaiKhoan != id))
                {
                    ModelState.AddModelError(
                        nameof(form.TenDangNhap),
                        "Tên đăng nhập đã tồn tại.");
                }
            }

            int? maDangNhap =
                HttpContext.Session.GetInt32("MaTaiKhoan");

            if (maDangNhap == id)
            {
                if (!form.TrangThai)
                    ModelState.AddModelError(
                        nameof(form.TrangThai),
                        "Không thể tự khóa tài khoản.");

                if (form.VaiTro != "Admin")
                    ModelState.AddModelError(
                        nameof(form.VaiTro),
                        "Không thể tự hạ quyền Admin.");
            }

            bool dangLaAdmin = taiKhoan.VaiTro == "Admin"
                              && taiKhoan.TrangThai;

            bool vanLaAdmin = form.VaiTro == "Admin"
                              && form.TrangThai;

            if (dangLaAdmin && !vanLaAdmin)
            {
                int soAdmin = await _db.TaiKhoans.CountAsync(
                    x => x.VaiTro == "Admin" && x.TrangThai);

                if (soAdmin <= 1)
                    ModelState.AddModelError("",
                        "Không thể khóa hoặc đổi quyền Admin cuối cùng.");
            }

            if (!ModelState.IsValid)
                return View(form);

            taiKhoan.TenDangNhap = form.TenDangNhap.Trim();
            taiKhoan.HoTen = form.HoTen.Trim();
            taiKhoan.Email = string.IsNullOrWhiteSpace(form.Email)
                ? null : form.Email.Trim();
            taiKhoan.VaiTro = form.VaiTro;
            taiKhoan.TrangThai = form.TrangThai;

            if (!string.IsNullOrWhiteSpace(form.MatKhau))
            {
                taiKhoan.MatKhau = _hasher.HashPassword(
                    taiKhoan, form.MatKhau);
            }

            await _db.SaveChangesAsync();

            if (maDangNhap == id)
            {
                HttpContext.Session.SetString(
                    "HoTen", taiKhoan.HoTen);
                HttpContext.Session.SetString(
                    "VaiTro", taiKhoan.VaiTro);
            }

            TempData["ThongBaoTaiKhoan"] =
                "Cập nhật tài khoản thành công!";

            return RedirectToAction(nameof(Index));
        }

        // KHÓA / MỞ KHÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var taiKhoan = await _db.TaiKhoans
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            int? maDangNhap =
                HttpContext.Session.GetInt32("MaTaiKhoan");

            if (maDangNhap == id && taiKhoan.TrangThai)
            {
                TempData["LoiTaiKhoan"] =
                    "Bạn không thể tự khóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            if (taiKhoan.VaiTro == "Admin" && taiKhoan.TrangThai)
            {
                int soAdmin = await _db.TaiKhoans.CountAsync(
                    x => x.VaiTro == "Admin" && x.TrangThai);

                if (soAdmin <= 1)
                {
                    TempData["LoiTaiKhoan"] =
                        "Không thể khóa Admin cuối cùng.";
                    return RedirectToAction(nameof(Index));
                }
            }

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _db.SaveChangesAsync();

            TempData["ThongBaoTaiKhoan"] = taiKhoan.TrangThai
                ? "Mở khóa tài khoản thành công!"
                : "Khóa tài khoản thành công!";

            return RedirectToAction(nameof(Index));
        }

        private void KiemTraVaiTro(TaiKhoanForm form)
        {
            if (form.VaiTro != "Admin" &&
                form.VaiTro != "SinhVien")
            {
                ModelState.AddModelError(
                    nameof(form.VaiTro),
                    "Vai trò phải là Admin hoặc SinhVien.");
            }
        }
    }
}

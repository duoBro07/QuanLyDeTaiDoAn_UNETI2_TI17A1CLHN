
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SinhVienController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. DANH SÁCH, TÌM KIẾM, LỌC, PHÂN TRANG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 8;
            trang = Math.Max(1, trang);

            var query = _db.SinhViens
                .Include(x => x.TaiKhoan)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.HoTen.Contains(tuKhoa) ||
                    x.Lop.Contains(tuKhoa) ||
                    x.Khoa.Contains(tuKhoa) ||
                    x.Email.Contains(tuKhoa));
            }

            if (trangThai == "HoatDong")
                query = query.Where(x => x.TrangThai);
            else if (trangThai == "DaKhoa")
                query = query.Where(x => !x.TrangThai);

            int tong = await query.CountAsync();
            int tongTrang = Math.Max(1,
                (int)Math.Ceiling(tong / (double)kichThuocTrang));

            trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(x => x.MaSinhVien)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.TrangThai = trangThai ?? "";
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.Tong = tong;

            return View(danhSach);
        }

        // 2. NẠP TÀI KHOẢN SINH VIÊN
        private async Task NapTaiKhoan(int? maDaChon = null)
        {
            var query = _db.TaiKhoans
                .Where(x => x.VaiTro == "SinhVien" &&
                    (x.MaTaiKhoan == maDaChon ||
                     !_db.SinhViens.Any(s =>
                         s.MaTaiKhoan == x.MaTaiKhoan)));

            var taiKhoans = await query
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.MaTaiKhoan,
                    TenHienThi = x.HoTen + " - " + x.TenDangNhap
                })
                .ToListAsync();

            ViewBag.DanhSachTaiKhoan = new SelectList(
                taiKhoans,
                "MaTaiKhoan",
                "TenHienThi",
                maDaChon);
        }

        // 3. THÊM - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapTaiKhoan();

            return View(new SinhVien
            {
                NgaySinh = new DateTime(2004, 1, 1),
                TrangThai = true
            });
        }

        // 4. THÊM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,HoTen,NgaySinh,Lop,Khoa,ChuyenNganh,Email,SoDienThoai,TrangThai")]
            SinhVien sinhVien)
        {
            await KiemTraDuLieu(sinhVien, false);

            if (!ModelState.IsValid)
            {
                await NapTaiKhoan(sinhVien.MaTaiKhoan);
                return View(sinhVien);
            }

            _db.SinhViens.Add(sinhVien);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Không thể lưu. Tài khoản hoặc Email có thể đã tồn tại.");

                await NapTaiKhoan(sinhVien.MaTaiKhoan);
                return View(sinhVien);
            }

            TempData["ThongBaoSinhVien"] =
                "Thêm sinh viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 5. CHI TIẾT
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sinhVien = await _db.SinhViens
                .Include(x => x.TaiKhoan)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            return View(sinhVien);
        }

        // 6. SỬA - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sinhVien = await _db.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            await NapTaiKhoan(sinhVien.MaTaiKhoan);

            return View(sinhVien);
        }

        // 7. SỬA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaSinhVien,MaTaiKhoan,HoTen,NgaySinh,Lop,Khoa,ChuyenNganh,Email,SoDienThoai,TrangThai")]
            SinhVien form)
        {
            if (id != form.MaSinhVien)
                return BadRequest();

            var sinhVien = await _db.SinhViens
                .FirstOrDefaultAsync(x => x.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            // Không cho thay tài khoản đã liên kết
            if (form.MaTaiKhoan != sinhVien.MaTaiKhoan)
            {
                ModelState.AddModelError(
                    nameof(form.MaTaiKhoan),
                    "Không được thay đổi tài khoản đã liên kết.");
            }

            await KiemTraDuLieu(form, true);

            if (!ModelState.IsValid)
            {
                await NapTaiKhoan(sinhVien.MaTaiKhoan);
                return View(form);
            }

            sinhVien.HoTen = form.HoTen.Trim();
            sinhVien.NgaySinh = form.NgaySinh;
            sinhVien.Lop = form.Lop.Trim();
            sinhVien.Khoa = form.Khoa.Trim();
            sinhVien.ChuyenNganh = form.ChuyenNganh.Trim();
            sinhVien.Email = form.Email.Trim();
            sinhVien.SoDienThoai = form.SoDienThoai?.Trim();
            sinhVien.TrangThai = form.TrangThai;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Không thể cập nhật dữ liệu sinh viên.");

                await NapTaiKhoan(sinhVien.MaTaiKhoan);
                return View(form);
            }

            TempData["ThongBaoSinhVien"] =
                "Cập nhật sinh viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 8. KHÓA / MỞ KHÓA HỒ SƠ SINH VIÊN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var sinhVien = await _db.SinhViens
                .FirstOrDefaultAsync(x => x.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            sinhVien.TrangThai = !sinhVien.TrangThai;

            await _db.SaveChangesAsync();

            TempData["ThongBaoSinhVien"] =
                sinhVien.TrangThai
                    ? "Đã kích hoạt hồ sơ sinh viên."
                    : "Đã tạm ngừng hồ sơ sinh viên.";

            return RedirectToAction(nameof(Index));
        }

        // 9. VALIDATION NGHIỆP VỤ
        private async Task KiemTraDuLieu(
            SinhVien sinhVien, bool laSua)
        {
            var taiKhoan = await _db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == sinhVien.MaTaiKhoan);

            if (taiKhoan == null ||
                taiKhoan.VaiTro != "SinhVien")
            {
                ModelState.AddModelError(
                    nameof(sinhVien.MaTaiKhoan),
                    "Phải chọn tài khoản có vai trò SinhVien.");
            }

            bool trungTaiKhoan = await _db.SinhViens
                .AnyAsync(x =>
                    x.MaTaiKhoan == sinhVien.MaTaiKhoan &&
                    (!laSua ||
                     x.MaSinhVien != sinhVien.MaSinhVien));

            if (trungTaiKhoan)
            {
                ModelState.AddModelError(
                    nameof(sinhVien.MaTaiKhoan),
                    "Tài khoản đã liên kết với sinh viên khác.");
            }

            if (!string.IsNullOrWhiteSpace(sinhVien.Email))
            {
                string email = sinhVien.Email.Trim();

                bool trungEmail = await _db.SinhViens
                    .AnyAsync(x =>
                        x.Email == email &&
                        (!laSua ||
                         x.MaSinhVien != sinhVien.MaSinhVien));

                if (trungEmail)
                {
                    ModelState.AddModelError(
                        nameof(sinhVien.Email),
                        "Email đã tồn tại.");
                }
            }

            if (sinhVien.NgaySinh == default ||
                sinhVien.NgaySinh.Date >= DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(sinhVien.NgaySinh),
                    "Ngày sinh phải nhỏ hơn ngày hiện tại.");
            }
        }
    }
}

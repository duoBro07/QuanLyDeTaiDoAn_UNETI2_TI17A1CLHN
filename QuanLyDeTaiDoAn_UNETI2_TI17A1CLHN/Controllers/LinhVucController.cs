
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class LinhVucController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LinhVucController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. DANH SÁCH - TÌM KIẾM - LỌC - PHÂN TRANG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 8;
            trang = Math.Max(1, trang);

            var query = _db.LinhVucs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.TenLinhVuc.Contains(tuKhoa) ||
                    (x.MoTa != null && x.MoTa.Contains(tuKhoa)));
            }

            if (trangThai == "HoatDong")
            {
                query = query.Where(x => x.TrangThai);
            }
            else if (trangThai == "NgungHoatDong")
            {
                query = query.Where(x => !x.TrangThai);
            }

            int tong = await query.CountAsync();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(tong / (double)kichThuocTrang));

            trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(x => x.MaLinhVuc)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.TrangThai = trangThai ?? "";
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.Tong = tong;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongBanGhi = tong;
            ViewBag.KichThuocTrang = kichThuocTrang;

            return View(danhSach);
        }

        // 2. THÊM - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View(new LinhVuc());
        }

        // 3. THÊM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenLinhVuc,MoTa,TrangThai")] LinhVuc linhVuc)
        {
            linhVuc.TenLinhVuc =
                linhVuc.TenLinhVuc?.Trim() ?? "";

            linhVuc.MoTa = string.IsNullOrWhiteSpace(linhVuc.MoTa)
                ? null
                : linhVuc.MoTa.Trim();

            ModelState.Remove(nameof(LinhVuc.TenLinhVuc));
            TryValidateModel(linhVuc);

            if (string.IsNullOrWhiteSpace(linhVuc.TenLinhVuc))
            {
                ModelState.AddModelError(
                    nameof(LinhVuc.TenLinhVuc),
                    "Vui lòng nhập tên lĩnh vực.");
            }

            if (ModelState.IsValid)
            {
                bool trungTen = await _db.LinhVucs
                    .AnyAsync(x => x.TenLinhVuc == linhVuc.TenLinhVuc);

                if (trungTen)
                {
                    ModelState.AddModelError(
                        nameof(LinhVuc.TenLinhVuc),
                        "Tên lĩnh vực đã tồn tại.");
                }
            }

            if (!ModelState.IsValid)
                return View(linhVuc);

            try
            {
                _db.LinhVucs.Add(linhVuc);
                await _db.SaveChangesAsync();

                TempData["ThongBaoLinhVuc"] =
                    "Thêm lĩnh vực thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    nameof(LinhVuc.TenLinhVuc),
                    "Không thể lưu lĩnh vực. Vui lòng kiểm tra lại.");

                return View(linhVuc);
            }
        }

        // 4. CHI TIẾT
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var linhVuc = await _db.LinhVucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
                return NotFound();

            ViewBag.SoGiangVien = await _db.GiangViens
                .CountAsync(x => x.MaLinhVuc == id);

            ViewBag.SoDeTai = await _db.DeTais
                .CountAsync(x => x.MaLinhVuc == id);

            return View(linhVuc);
        }

        // 5. SỬA - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var linhVuc = await _db.LinhVucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
                return NotFound();

            return View(linhVuc);
        }

        // 6. SỬA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaLinhVuc,TenLinhVuc,MoTa,TrangThai")] LinhVuc form)
        {
            if (id != form.MaLinhVuc)
                return BadRequest();

            var linhVuc = await _db.LinhVucs
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
                return NotFound();

            form.TenLinhVuc = form.TenLinhVuc?.Trim() ?? "";

            form.MoTa = string.IsNullOrWhiteSpace(form.MoTa)
                ? null
                : form.MoTa.Trim();

            ModelState.Remove(nameof(LinhVuc.TenLinhVuc));
            TryValidateModel(form);

            if (string.IsNullOrWhiteSpace(form.TenLinhVuc))
            {
                ModelState.AddModelError(
                    nameof(LinhVuc.TenLinhVuc),
                    "Vui lòng nhập tên lĩnh vực.");
            }

            if (ModelState.IsValid)
            {
                bool trungTen = await _db.LinhVucs.AnyAsync(x =>
                    x.TenLinhVuc == form.TenLinhVuc &&
                    x.MaLinhVuc != id);

                if (trungTen)
                {
                    ModelState.AddModelError(
                        nameof(LinhVuc.TenLinhVuc),
                        "Tên lĩnh vực đã tồn tại.");
                }
            }

            if (!ModelState.IsValid)
                return View(form);

            linhVuc.TenLinhVuc = form.TenLinhVuc;
            linhVuc.MoTa = form.MoTa;
            linhVuc.TrangThai = form.TrangThai;

            try
            {
                await _db.SaveChangesAsync();

                TempData["ThongBaoLinhVuc"] =
                    "Cập nhật lĩnh vực thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật lĩnh vực. Vui lòng thử lại.");

                return View(form);
            }
        }

        // 7. XÓA - GET
        // Hiển thị trang xác nhận xóa
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var linhVuc = await _db.LinhVucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
                return NotFound();

            ViewBag.SoGiangVien = await _db.GiangViens
                .CountAsync(x => x.MaLinhVuc == id);

            ViewBag.SoDeTai = await _db.DeTais
                .CountAsync(x => x.MaLinhVuc == id);

            return View(linhVuc);
        }

        // 8. XÓA - POST
        // Chỉ xóa khi người dùng đã xác nhận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            bool xacNhan)
        {
            if (!xacNhan)
            {
                TempData["LoiLinhVuc"] =
                    "Bạn chưa xác nhận xóa lĩnh vực.";

                return RedirectToAction(nameof(Delete), new { id });
            }

            var linhVuc = await _db.LinhVucs
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
                return NotFound();

            bool coGiangVien = await _db.GiangViens
                .AnyAsync(x => x.MaLinhVuc == id);

            bool coDeTai = await _db.DeTais
                .AnyAsync(x => x.MaLinhVuc == id);

            if (coGiangVien || coDeTai)
            {
                TempData["LoiLinhVuc"] =
                    "Không thể xóa lĩnh vực vì đang có giảng viên " +
                    "hoặc đề tài sử dụng.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.LinhVucs.Remove(linhVuc);
                await _db.SaveChangesAsync();

                TempData["ThongBaoLinhVuc"] =
                    "Xóa lĩnh vực thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["LoiLinhVuc"] =
                    "Không thể xóa lĩnh vực vì có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

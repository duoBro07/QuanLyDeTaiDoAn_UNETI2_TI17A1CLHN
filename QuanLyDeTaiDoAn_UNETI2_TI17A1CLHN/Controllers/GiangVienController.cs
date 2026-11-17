
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class GiangVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiangVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task NapLinhVuc(int? maLinhVuc = null)
        {
            var danhSach = await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            ViewBag.MaLinhVuc = new SelectList(
                danhSach,
                "MaLinhVuc",
                "TenLinhVuc",
                maLinhVuc);

            ViewBag.LinhVucs = ViewBag.MaLinhVuc;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 5;
            trang = Math.Max(1, trang);

            var query = _context.GiangViens
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.HoTen.Contains(tuKhoa) ||
                    x.Email.Contains(tuKhoa) ||
                    (x.SoDienThoai != null &&
                     x.SoDienThoai.Contains(tuKhoa)));
            }

            if (maLinhVuc.HasValue)
            {
                query = query.Where(x =>
                    x.MaLinhVuc == maLinhVuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }

            int tong = await query.CountAsync();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(
                    tong / (double)kichThuocTrang));

            trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(x => x.HoTen)
                .ThenBy(x => x.MaGiangVien)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tong;
            ViewBag.KichThuocTrang = kichThuocTrang;
            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.TrangThaiLoc = trangThai ?? "";

            ViewBag.LinhVucs = await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var gv = await _context.GiangViens
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .FirstOrDefaultAsync(x =>
                    x.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapLinhVuc();

            return View(new GiangVien
            {
                SoSinhVienHuongDanToiDa = 5
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien model)
        {
            if (await _context.GiangViens.AnyAsync(x =>
                x.Email == model.Email))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email đã được sử dụng.");
            }

            if (!await _context.LinhVucs.AnyAsync(x =>
                x.MaLinhVuc == model.MaLinhVuc))
            {
                ModelState.AddModelError(
                    nameof(model.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.GiangViens.Add(model);
                await _context.SaveChangesAsync();

                TempData["ThongBaoGiangVien"] =
                    "Đã thêm giảng viên thành công.";

                return RedirectToAction(nameof(Index));
            }

            await NapLinhVuc(model.MaLinhVuc);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gv = await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaGiangVien,HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien model)
        {
            if (id != model.MaGiangVien)
                return BadRequest();

            var gv = await _context.GiangViens
                .FirstOrDefaultAsync(x =>
                    x.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            if (await _context.GiangViens.AnyAsync(x =>
                x.Email == model.Email &&
                x.MaGiangVien != id))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email đã được sử dụng.");
            }

            if (!await _context.LinhVucs.AnyAsync(x =>
                x.MaLinhVuc == model.MaLinhVuc))
            {
                ModelState.AddModelError(
                    nameof(model.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            if (ModelState.IsValid)
            {
                int tai = await _context.PhanCongHuongDans
                    .CountAsync(x =>
                        x.MaGiangVien == id &&
                        x.DangHieuLuc &&
                        x.TrangThaiThucHien != "Hoàn thành");

                if (model.SoSinhVienHuongDanToiDa < tai)
                {
                    ModelState.AddModelError(
                        nameof(model.SoSinhVienHuongDanToiDa),
                        $"Giảng viên đang hướng dẫn {tai} sinh viên; không thể đặt giới hạn thấp hơn.");
                }

                if (model.TrangThai != "Đang công tác" &&
                    tai > 0)
                {
                    ModelState.AddModelError(
                        nameof(model.TrangThai),
                        "Cần xử lý các phân công đang hiệu lực trước khi tạm ngừng giảng viên.");
                }
            }

            if (ModelState.IsValid)
            {
                gv.HoTen = model.HoTen.Trim();
                gv.HocVi = model.HocVi.Trim();
                gv.Email = model.Email.Trim();
                gv.SoDienThoai = model.SoDienThoai?.Trim();
                gv.MaLinhVuc = model.MaLinhVuc;
                gv.SoSinhVienHuongDanToiDa =
                    model.SoSinhVienHuongDanToiDa;
                gv.TrangThai = model.TrangThai;

                await _context.SaveChangesAsync();

                TempData["ThongBaoGiangVien"] =
                    "Đã cập nhật giảng viên thành công.";

                return RedirectToAction(nameof(Index));
            }

            await NapLinhVuc(model.MaLinhVuc);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gv = await _context.GiangViens
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .FirstOrDefaultAsync(x =>
                    x.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gv = await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            int tai = await _context.PhanCongHuongDans
                .CountAsync(x =>
                    x.MaGiangVien == id &&
                    x.DangHieuLuc &&
                    x.TrangThaiThucHien != "Hoàn thành");

            if (tai > 0)
            {
                TempData["LoiGiangVien"] =
                    $"Giảng viên đang hướng dẫn {tai} sinh viên. Hãy xử lý phân công trước khi tạm ngừng.";

                return RedirectToAction(nameof(Index));
            }

            gv.TrangThai = "Tạm ngừng hướng dẫn";

            await _context.SaveChangesAsync();

            TempData["ThongBaoGiangVien"] =
                "Đã tạm ngừng giảng viên, hồ sơ và lịch sử được giữ nguyên.";

            return RedirectToAction(nameof(Index));
        }
    }
}

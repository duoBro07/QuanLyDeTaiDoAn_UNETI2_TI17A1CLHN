using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn.Data;
using QuanLyDeTaiDoAn.Models;

namespace QuanLyDeTaiDoAn.Controllers
{
    public class GiangVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiangVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 5;

            var query = _context.GiangViens
                .Include(g => g.LinhVuc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(g =>
                    g.HoTen.Contains(tuKhoa) ||
                    g.Email.Contains(tuKhoa) ||
                    (g.SoDienThoai != null &&
                     g.SoDienThoai.Contains(tuKhoa)));
            }

            if (maLinhVuc.HasValue)
            {
                query = query.Where(g =>
                    g.MaLinhVuc == maLinhVuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(g =>
                    g.TrangThai == trangThai);
            }

            int tongBanGhi = await query.CountAsync();

            int tongTrang = (int)Math.Ceiling(
                tongBanGhi / (double)kichThuocTrang);

            trang = Math.Max(trang, 1);

            if (tongTrang > 0)
                trang = Math.Min(trang, tongTrang);

            var ds = await query
                .OrderBy(g => g.HoTen)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.LinhVucs =
                await _context.LinhVucs
                    .OrderBy(l => l.TenLinhVuc)
                    .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.TrangThaiLoc = trangThai;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tongBanGhi;
            ViewBag.KichThuocTrang = kichThuocTrang;

            return View(ds);
        }

        // ================= DETAILS =================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var gv = await _context.GiangViens
                .Include(g => g.LinhVuc)
                .FirstOrDefaultAsync(g =>
                    g.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        // ================= CREATE GET =================

        public async Task<IActionResult> Create()
        {
            await NapLinhVuc();
            return View();
        }

        // ================= CREATE POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaGiangVien,HoTen,HocVi,Email," +
                "SoDienThoai,MaLinhVuc," +
                "SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            gv.HoTen = gv.HoTen.Trim();
            gv.HocVi = gv.HocVi.Trim();
            gv.Email = gv.Email.Trim();

            if (!string.IsNullOrWhiteSpace(gv.SoDienThoai))
                gv.SoDienThoai = gv.SoDienThoai.Trim();

            bool trungEmail =
                await _context.GiangViens.AnyAsync(x =>
                    x.Email == gv.Email);

            if (trungEmail)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email đã tồn tại.");
            }

            bool linhVucTonTai =
                await _context.LinhVucs.AnyAsync(x =>
                    x.MaLinhVuc == gv.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    "MaLinhVuc",
                    "Lĩnh vực không tồn tại.");
            }

            if (gv.TrangThai != "Đang công tác" &&
                gv.TrangThai != "Tạm ngừng hướng dẫn")
            {
                ModelState.AddModelError(
                    "TrangThai",
                    "Trạng thái không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(gv);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

        // ================= EDIT GET =================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var gv =
                await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            await NapLinhVuc(gv.MaLinhVuc);

            return View(gv);
        }

        // ================= EDIT POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "MaGiangVien,HoTen,HocVi,Email," +
                "SoDienThoai,MaLinhVuc," +
                "SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            if (id != gv.MaGiangVien)
                return NotFound();

            bool trungEmail =
                await _context.GiangViens.AnyAsync(x =>
                    x.Email == gv.Email &&
                    x.MaGiangVien != gv.MaGiangVien);

            if (trungEmail)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email đã được sử dụng.");
            }

            bool linhVucTonTai =
                await _context.LinhVucs.AnyAsync(x =>
                    x.MaLinhVuc == gv.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    "MaLinhVuc",
                    "Lĩnh vực không tồn tại.");
            }

            if (gv.TrangThai != "Đang công tác" &&
                gv.TrangThai != "Tạm ngừng hướng dẫn")
            {
                ModelState.AddModelError(
                    "TrangThai",
                    "Trạng thái không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gv);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.GiangViens
                        .AnyAsync(x =>
                            x.MaGiangVien == id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await NapLinhVuc(gv.MaLinhVuc);

            return View(gv);
        }

        // ================= DELETE GET =================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var gv = await _context.GiangViens
                .Include(g => g.LinhVuc)
                .FirstOrDefaultAsync(g =>
                    g.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        // ================= DELETE POST =================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int id)
        {
            var gv =
                await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            _context.GiangViens.Remove(gv);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task NapLinhVuc(
            int? selected = null)
        {
            ViewBag.MaLinhVuc =
                new SelectList(
                    await _context.LinhVucs
                        .OrderBy(x => x.TenLinhVuc)
                        .ToListAsync(),
                    "MaLinhVuc",
                    "TenLinhVuc",
                    selected);
        }
    }
}
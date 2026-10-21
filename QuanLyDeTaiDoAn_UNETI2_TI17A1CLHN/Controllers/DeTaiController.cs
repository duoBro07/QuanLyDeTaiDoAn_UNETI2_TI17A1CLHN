using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn.Data;
using QuanLyDeTaiDoAn.Models;

namespace QuanLyDeTaiDoAn.Controllers
{
    public class DeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH + TÌM KIẾM + LỌC + SẮP XẾP + PHÂN TRANG
        // =====================================================
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? namHoc,
            int? hocKy,
            string? trangThai,
            string? sapXep,
            int trang = 1)
        {
            const int kichThuocTrang = 10;

            var query = _context.DeTais
                .Include(d => d.LinhVuc)
                .AsQueryable();

            // =========================
            // TÌM KIẾM
            // =========================
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(d =>
                    d.TenDeTai.Contains(tuKhoa) ||
                    (d.MoTa != null &&
                     d.MoTa.Contains(tuKhoa)) ||
                    (d.YeuCau != null &&
                     d.YeuCau.Contains(tuKhoa)));
            }

            // =========================
            // LỌC LĨNH VỰC
            // =========================
            if (maLinhVuc.HasValue)
            {
                query = query.Where(d =>
                    d.MaLinhVuc == maLinhVuc.Value);
            }

            // =========================
            // LỌC NĂM HỌC
            // =========================
            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(d =>
                    d.NamHoc == namHoc);
            }

            // =========================
            // LỌC HỌC KỲ
            // =========================
            if (hocKy.HasValue)
            {
                query = query.Where(d =>
                    d.HocKy == hocKy.Value);
            }

            // =========================
            // LỌC TRẠNG THÁI
            // =========================
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(d =>
                    d.TrangThai == trangThai);
            }

            // =========================
            // SẮP XẾP
            // =========================
            query = sapXep switch
            {
                "ten_za" =>
                    query.OrderByDescending(d => d.TenDeTai),

                "sv_tang" =>
                    query.OrderBy(d => d.SoSinhVienToiDa),

                "sv_giam" =>
                    query.OrderByDescending(d =>
                        d.SoSinhVienToiDa),

                _ =>
                    query.OrderBy(d => d.TenDeTai)
            };

            // =========================
            // PHÂN TRANG
            // =========================
            int tongBanGhi =
                await query.CountAsync();

            int tongTrang =
                (int)Math.Ceiling(
                    tongBanGhi /
                    (double)kichThuocTrang);

            if (trang < 1)
                trang = 1;

            if (tongTrang > 0 &&
                trang > tongTrang)
            {
                trang = tongTrang;
            }

            var danhSach = await query
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            // =========================
            // DỮ LIỆU CHO BỘ LỌC
            // =========================
            ViewBag.LinhVucs =
                await _context.LinhVucs
                    .OrderBy(l => l.TenLinhVuc)
                    .ToListAsync();

            ViewBag.NamHocs =
                await _context.DeTais
                    .Select(d => d.NamHoc)
                    .Distinct()
                    .OrderByDescending(n => n)
                    .ToListAsync();

            // Giữ lại điều kiện
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.NamHocLoc = namHoc;
            ViewBag.HocKyLoc = hocKy;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.SapXep = sapXep;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tongBanGhi;
            ViewBag.KichThuocTrang = kichThuocTrang;

            return View(danhSach);
        }

        // =====================================================
        // CHI TIẾT
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .Include(d => d.LinhVuc)
                .FirstOrDefaultAsync(d =>
                    d.MaDeTai == id.Value);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }

        // =====================================================
        // THÊM - GET
        // =====================================================
        public async Task<IActionResult> Create()
        {
            await NapDanhSachLinhVuc();

            return View();
        }

        // =====================================================
        // THÊM - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaDeTai,TenDeTai,MaLinhVuc,MoTa,YeuCau," +
                "SoSinhVienToiDa,NamHoc,HocKy,TrangThai")]
            DeTai deTai)
        {
            // Kiểm tra tên đề tài trùng
            // trong cùng năm học + học kỳ
            bool tenDaTonTai =
                await _context.DeTais.AnyAsync(d =>
                    d.TenDeTai == deTai.TenDeTai &&
                    d.NamHoc == deTai.NamHoc &&
                    d.HocKy == deTai.HocKy);

            if (tenDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(DeTai.TenDeTai),
                    "Tên đề tài đã tồn tại trong năm học và học kỳ này.");
            }

            // Kiểm tra lĩnh vực thật sự tồn tại trong DB
            bool linhVucTonTai =
                await _context.LinhVucs.AnyAsync(l =>
                    l.MaLinhVuc == deTai.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    nameof(DeTai.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            // Kiểm tra trạng thái
            string[] trangThaiHopLe =
            {
                "Mở đăng ký",
                "Đủ số lượng",
                "Tạm khóa",
                "Kết thúc"
            };

            if (!trangThaiHopLe.Contains(
                    deTai.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(DeTai.TrangThai),
                    "Trạng thái không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                _context.DeTais.Add(deTai);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    "Thêm đề tài thành công.";

                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachLinhVuc(
                deTai.MaLinhVuc);

            return View(deTai);
        }

        // =====================================================
        // SỬA - GET
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai =
                await _context.DeTais.FindAsync(id.Value);

            if (deTai == null)
                return NotFound();

            await NapDanhSachLinhVuc(
                deTai.MaLinhVuc);

            return View(deTai);
        }

        // =====================================================
        // SỬA - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "MaDeTai,TenDeTai,MaLinhVuc,MoTa,YeuCau," +
                "SoSinhVienToiDa,NamHoc,HocKy,TrangThai")]
            DeTai deTai)
        {
            if (id != deTai.MaDeTai)
                return NotFound();

            bool tenDaTonTai =
                await _context.DeTais.AnyAsync(d =>
                    d.TenDeTai == deTai.TenDeTai &&
                    d.NamHoc == deTai.NamHoc &&
                    d.HocKy == deTai.HocKy &&
                    d.MaDeTai != deTai.MaDeTai);

            if (tenDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(DeTai.TenDeTai),
                    "Tên đề tài đã tồn tại trong năm học và học kỳ này.");
            }

            bool linhVucTonTai =
                await _context.LinhVucs.AnyAsync(l =>
                    l.MaLinhVuc == deTai.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    nameof(DeTai.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            string[] trangThaiHopLe =
            {
                "Mở đăng ký",
                "Đủ số lượng",
                "Tạm khóa",
                "Kết thúc"
            };

            if (!trangThaiHopLe.Contains(
                    deTai.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(DeTai.TrangThai),
                    "Trạng thái không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.DeTais.Update(deTai);

                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] =
                        "Cập nhật đề tài thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool tonTai =
                        await _context.DeTais.AnyAsync(d =>
                            d.MaDeTai ==
                            deTai.MaDeTai);

                    if (!tonTai)
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachLinhVuc(
                deTai.MaLinhVuc);

            return View(deTai);
        }

        // =====================================================
        // XÓA - GET
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .Include(d => d.LinhVuc)
                .FirstOrDefaultAsync(d =>
                    d.MaDeTai == id.Value);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }

        // =====================================================
        // XÓA - POST
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int id)
        {
            var deTai =
                await _context.DeTais.FindAsync(id);

            if (deTai == null)
                return NotFound();

            _context.DeTais.Remove(deTai);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Xóa đề tài thành công.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // NẠP LĨNH VỰC CHO DROPDOWN
        // =====================================================
        private async Task NapDanhSachLinhVuc(
            int? maLinhVucDuocChon = null)
        {
            var danhSach =
                await _context.LinhVucs
                    .OrderBy(l => l.TenLinhVuc)
                    .ToListAsync();

            ViewBag.MaLinhVuc =
                new SelectList(
                    danhSach,
                    "MaLinhVuc",
                    "TenLinhVuc",
                    maLinhVucDuocChon);
        }
    }
}
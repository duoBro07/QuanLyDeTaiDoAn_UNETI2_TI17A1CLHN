
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

        // DANH SÁCH, TÌM KIẾM, LỌC, PHÂN TRANG
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 5;

            var query = _context.GiangViens
                .Include(g => g.LinhVuc)
                .AsNoTracking()
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

            trang = Math.Max(1, trang);

            if (tongTrang > 0)
                trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(g => g.HoTen)
                .ThenBy(g => g.MaGiangVien)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.LinhVucs = await _context.LinhVucs
                .OrderBy(l => l.TenLinhVuc)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.TrangThaiLoc = trangThai;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tongBanGhi;
            ViewBag.KichThuocTrang = kichThuocTrang;

            return View(danhSach);
        }

        // XEM CHI TIẾT
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var gv = await _context.GiangViens
                .Include(g => g.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        // THÊM - GET
        public async Task<IActionResult> Create()
        {
            await NapLinhVuc();
            return View();
        }

        // THÊM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            ChuanHoa(gv);
            await KiemTraDuLieu(gv);

            if (ModelState.IsValid)
            {
                _context.GiangViens.Add(gv);

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] =
                        "Thêm giảng viên thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể lưu giảng viên. Email có thể đã tồn tại hoặc dữ liệu không hợp lệ.");
                }
            }

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

        // SỬA - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var gv = await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

        // SỬA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaGiangVien,HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            if (id != gv.MaGiangVien)
                return NotFound();

            ChuanHoa(gv);
            await KiemTraDuLieu(gv, id);

            if (ModelState.IsValid)
            {
                var gvCu = await _context.GiangViens.FindAsync(id);

                if (gvCu == null)
                    return NotFound();

                gvCu.HoTen = gv.HoTen;
                gvCu.HocVi = gv.HocVi;
                gvCu.Email = gv.Email;
                gvCu.SoDienThoai = gv.SoDienThoai;
                gvCu.MaLinhVuc = gv.MaLinhVuc;
                gvCu.SoSinhVienHuongDanToiDa =
                    gv.SoSinhVienHuongDanToiDa;
                gvCu.TrangThai = gv.TrangThai;

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] =
                        "Cập nhật giảng viên thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật giảng viên. Email có thể đã tồn tại hoặc dữ liệu không hợp lệ.");
                }
            }

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

        // XÁC NHẬN NGỪNG HƯỚNG DẪN - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var gv = await _context.GiangViens
                .Include(g => g.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiangVien == id);

            if (gv == null)
                return NotFound();

            return View(gv);
        }

        // TẠM NGỪNG THAY VÌ XÓA VĨNH VIỄN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gv = await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            if (gv.TrangThai == "Tạm ngừng hướng dẫn")
            {
                TempData["ThongBao"] =
                    "Giảng viên đã ở trạng thái tạm ngừng hướng dẫn.";

                return RedirectToAction(nameof(Index));
            }

            gv.TrangThai = "Tạm ngừng hướng dẫn";

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    "Đã tạm ngừng hướng dẫn. Hồ sơ giảng viên vẫn được giữ lại.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] =
                    "Không thể cập nhật trạng thái giảng viên.";
            }

            return RedirectToAction(nameof(Index));
        }

        // CHUẨN HÓA DỮ LIỆU
        private void ChuanHoa(GiangVien gv)
        {
            gv.HoTen = (gv.HoTen ?? "").Trim();
            gv.HocVi = (gv.HocVi ?? "").Trim();
            gv.Email = (gv.Email ?? "").Trim();

            gv.SoDienThoai =
                string.IsNullOrWhiteSpace(gv.SoDienThoai)
                    ? null
                    : gv.SoDienThoai.Trim();

            if (gv.HoTen.Length == 0)
                ModelState.AddModelError(
                    nameof(gv.HoTen),
                    "Vui lòng nhập họ tên.");

            if (gv.HocVi.Length == 0)
                ModelState.AddModelError(
                    nameof(gv.HocVi),
                    "Vui lòng nhập học vị.");

            if (gv.Email.Length == 0)
                ModelState.AddModelError(
                    nameof(gv.Email),
                    "Vui lòng nhập Email.");
        }

        // KIỂM TRA EMAIL, LĨNH VỰC, TRẠNG THÁI
        private async Task KiemTraDuLieu(
            GiangVien gv,
            int? idLoaiTru = null)
        {
            bool trungEmail = await _context.GiangViens
                .AnyAsync(g =>
                    g.Email == gv.Email &&
                    (!idLoaiTru.HasValue ||
                     g.MaGiangVien != idLoaiTru.Value));

            if (trungEmail)
            {
                ModelState.AddModelError(
                    nameof(gv.Email),
                    "Email đã tồn tại.");
            }

            bool linhVucTonTai = await _context.LinhVucs
                .AnyAsync(l => l.MaLinhVuc == gv.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    nameof(gv.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            if (gv.TrangThai != "Đang công tác" &&
                gv.TrangThai != "Tạm ngừng hướng dẫn")
            {
                ModelState.AddModelError(
                    nameof(gv.TrangThai),
                    "Trạng thái không hợp lệ.");
            }
        }

        // DROPDOWN LĨNH VỰC
        private async Task NapLinhVuc(int? selected = null)
        {
            var danhSach = await _context.LinhVucs
                .OrderBy(l => l.TenLinhVuc)
                .ToListAsync();

            ViewBag.MaLinhVuc = new SelectList(
                danhSach,
                "MaLinhVuc",
                "TenLinhVuc",
                selected);
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class GiangVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiangVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? trangThai,
            int trang = 1)
        {
            const int kichThuocTrang = 5;
            trang = Math.Max(1, trang);

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

            if (tongTrang > 0)
                trang = Math.Min(trang, tongTrang);

            var danhSach = await query
                .OrderBy(g => g.HoTen)
                .ThenBy(g => g.MaGiangVien)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.LinhVucs = await _context.LinhVucs
                .AsNoTracking()
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

        public async Task<IActionResult> Create()
        {
            await NapLinhVuc();

            return View(new GiangVien
            {
                TrangThai = "Đang công tác",
                SoSinhVienHuongDanToiDa = 5
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            ChuanHoa(gv);
            ModelState.Remove(nameof(GiangVien.LinhVuc));

            await KiemTraDuLieu(gv);

            if (ModelState.IsValid)
            {
                _context.GiangViens.Add(gv);

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["ThongBaoGiangVien"] =
                        "Thêm giảng viên thành công!";

                    return RedirectToAction("Index", "GiangVien");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể thêm giảng viên. Email có thể đã tồn tại.");
                }
            }

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaGiangVien,HoTen,HocVi,Email,SoDienThoai,MaLinhVuc,SoSinhVienHuongDanToiDa,TrangThai")]
            GiangVien gv)
        {
            if (id != gv.MaGiangVien)
                return NotFound();

            var gvCu = await _context.GiangViens
                .FirstOrDefaultAsync(x => x.MaGiangVien == id);

            if (gvCu == null)
                return NotFound();

            ChuanHoa(gv);
            ModelState.Remove(nameof(GiangVien.LinhVuc));

            await KiemTraDuLieu(gv, id);

            if (ModelState.IsValid)
            {
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

                    TempData["ThongBaoGiangVien"] =
                        "Cập nhật giảng viên thành công!";

                    return RedirectToAction("Index", "GiangVien");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật. Email có thể đã tồn tại.");
                }
            }

            await NapLinhVuc(gv.MaLinhVuc);
            return View(gv);
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gv = await _context.GiangViens.FindAsync(id);

            if (gv == null)
                return NotFound();

            if (gv.TrangThai == "Tạm ngừng hướng dẫn")
            {
                TempData["ThongBaoGiangVien"] =
                    "Giảng viên đã tạm ngừng hướng dẫn.";

                return RedirectToAction("Index", "GiangVien");
            }

            gv.TrangThai = "Tạm ngừng hướng dẫn";

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBaoGiangVien"] =
                    "Đã tạm ngừng hướng dẫn. Dữ liệu giảng viên được giữ lại.";
            }
            catch (DbUpdateException)
            {
                TempData["LoiGiangVien"] =
                    "Không thể cập nhật trạng thái giảng viên.";
            }

            return RedirectToAction("Index", "GiangVien");
        }

        private void ChuanHoa(GiangVien gv)
        {
            gv.HoTen = (gv.HoTen ?? "").Trim();
            gv.HocVi = (gv.HocVi ?? "").Trim();
            gv.Email = (gv.Email ?? "").Trim();
            gv.TrangThai = (gv.TrangThai ?? "").Trim();

            gv.SoDienThoai =
                string.IsNullOrWhiteSpace(gv.SoDienThoai)
                    ? null
                    : gv.SoDienThoai.Trim();

            ModelState.Remove(nameof(GiangVien.HoTen));
            ModelState.Remove(nameof(GiangVien.HocVi));
            ModelState.Remove(nameof(GiangVien.Email));
            ModelState.Remove(nameof(GiangVien.SoDienThoai));
            ModelState.Remove(nameof(GiangVien.TrangThai));

            if (string.IsNullOrWhiteSpace(gv.HoTen))
                ModelState.AddModelError(
                    nameof(gv.HoTen),
                    "Vui lòng nhập họ tên.");

            if (string.IsNullOrWhiteSpace(gv.HocVi))
                ModelState.AddModelError(
                    nameof(gv.HocVi),
                    "Vui lòng nhập học vị.");

            if (string.IsNullOrWhiteSpace(gv.Email))
                ModelState.AddModelError(
                    nameof(gv.Email),
                    "Vui lòng nhập Email.");
            else if (!new EmailAddressAttribute().IsValid(gv.Email))
                ModelState.AddModelError(
                    nameof(gv.Email),
                    "Email không hợp lệ.");

            if (gv.HoTen.Length > 100)
                ModelState.AddModelError(
                    nameof(gv.HoTen),
                    "Họ tên không được quá 100 ký tự.");

            if (gv.HocVi.Length > 50)
                ModelState.AddModelError(
                    nameof(gv.HocVi),
                    "Học vị không được quá 50 ký tự.");

            if (gv.Email.Length > 100)
                ModelState.AddModelError(
                    nameof(gv.Email),
                    "Email không được quá 100 ký tự.");

            if (gv.SoDienThoai?.Length > 20)
                ModelState.AddModelError(
                    nameof(gv.SoDienThoai),
                    "Số điện thoại không được quá 20 ký tự.");
        }

        private async Task KiemTraDuLieu(
            GiangVien gv,
            int? idLoaiTru = null)
        {
            if (!string.IsNullOrWhiteSpace(gv.Email))
            {
                bool trungEmail = await _context.GiangViens
                    .AnyAsync(g =>
                        g.Email == gv.Email &&
                        (!idLoaiTru.HasValue ||
                         g.MaGiangVien != idLoaiTru.Value));

                if (trungEmail)
                    ModelState.AddModelError(
                        nameof(gv.Email),
                        "Email đã tồn tại.");
            }

            bool linhVucHopLe = await _context.LinhVucs
                .AnyAsync(l =>
                    l.MaLinhVuc == gv.MaLinhVuc &&
                    l.TrangThai);

            if (!linhVucHopLe)
                ModelState.AddModelError(
                    nameof(gv.MaLinhVuc),
                    "Vui lòng chọn lĩnh vực đang hoạt động.");

            if (gv.SoSinhVienHuongDanToiDa <= 0)
                ModelState.AddModelError(
                    nameof(gv.SoSinhVienHuongDanToiDa),
                    "Số sinh viên hướng dẫn tối đa phải lớn hơn 0.");

            if (gv.TrangThai != "Đang công tác" &&
                gv.TrangThai != "Tạm ngừng hướng dẫn")
                ModelState.AddModelError(
                    nameof(gv.TrangThai),
                    "Trạng thái giảng viên không hợp lệ.");
        }

        private async Task NapLinhVuc(int? selected = null)
        {
            var danhSach = await _context.LinhVucs
                .AsNoTracking()
                .Where(l => l.TrangThai ||
                    (selected.HasValue &&
                     l.MaLinhVuc == selected.Value))
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

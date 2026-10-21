
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

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLinhVuc,
            string? namHoc,
            int? hocKy,
            string? trangThai,
            string? choCon,
            string? sapXep,
            int trang = 1)
        {
            const int kichThuocTrang = 10;

            trang = Math.Max(1, trang);

            var duLieu = _context.DeTais
                .AsNoTracking()
                .Select(d => new
                {
                    DeTai = d,
                    SoDaDangKy = _context.DangKyDeTaiModule2s.Count(
                        dk => dk.MaDeTai == d.MaDeTai &&
                        (dk.TrangThai == "Đã duyệt" ||
                         dk.TrangThai == "Đang thực hiện"))
                });

            var truyVan = duLieu.Select(x => new
            {
                x.DeTai,
                x.SoDaDangKy,
                SoChoCon = x.DeTai.SoSinhVienToiDa - x.SoDaDangKy
            });

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                truyVan = truyVan.Where(x =>
                    x.DeTai.TenDeTai.Contains(tuKhoa) ||
                    (x.DeTai.MoTa != null &&
                     x.DeTai.MoTa.Contains(tuKhoa)) ||
                    (x.DeTai.YeuCau != null &&
                     x.DeTai.YeuCau.Contains(tuKhoa)));
            }

            if (maLinhVuc.HasValue)
            {
                truyVan = truyVan.Where(x =>
                    x.DeTai.MaLinhVuc == maLinhVuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                truyVan = truyVan.Where(x =>
                    x.DeTai.NamHoc == namHoc);
            }

            if (hocKy.HasValue)
            {
                truyVan = truyVan.Where(x =>
                    x.DeTai.HocKy == hocKy.Value);
            }

            if (choCon == "con")
            {
                truyVan = truyVan.Where(x => x.SoChoCon > 0);
            }
            else if (choCon == "het")
            {
                truyVan = truyVan.Where(x => x.SoChoCon <= 0);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai == "Mở đăng ký")
                {
                    truyVan = truyVan.Where(x =>
                        x.DeTai.TrangThai != "Tạm khóa" &&
                        x.DeTai.TrangThai != "Kết thúc" &&
                        x.SoChoCon > 0);
                }
                else if (trangThai == "Đủ số lượng")
                {
                    truyVan = truyVan.Where(x =>
                        x.DeTai.TrangThai != "Tạm khóa" &&
                        x.DeTai.TrangThai != "Kết thúc" &&
                        x.SoChoCon <= 0);
                }
                else
                {
                    truyVan = truyVan.Where(x =>
                        x.DeTai.TrangThai == trangThai);
                }
            }

            truyVan = sapXep switch
            {
                "ten_za" => truyVan
                    .OrderByDescending(x => x.DeTai.TenDeTai)
                    .ThenBy(x => x.DeTai.MaDeTai),

                "sv_tang" => truyVan
                    .OrderBy(x => x.DeTai.SoSinhVienToiDa)
                    .ThenBy(x => x.DeTai.MaDeTai),

                "sv_giam" => truyVan
                    .OrderByDescending(x => x.DeTai.SoSinhVienToiDa)
                    .ThenBy(x => x.DeTai.MaDeTai),

                "cho_tang" => truyVan
                    .OrderBy(x => x.SoChoCon)
                    .ThenBy(x => x.DeTai.MaDeTai),

                "cho_giam" => truyVan
                    .OrderByDescending(x => x.SoChoCon)
                    .ThenBy(x => x.DeTai.MaDeTai),

                _ => truyVan
                    .OrderBy(x => x.DeTai.TenDeTai)
                    .ThenBy(x => x.DeTai.MaDeTai)
            };

            int tongBanGhi = await truyVan.CountAsync();

            int tongTrang = (int)Math.Ceiling(
                tongBanGhi / (double)kichThuocTrang);

            if (tongTrang > 0 && trang > tongTrang)
            {
                trang = tongTrang;
            }

            var ketQua = await truyVan
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            var danhSachDeTai = ketQua
                .Select(x => x.DeTai)
                .ToList();

            var maLinhVucs = danhSachDeTai
                .Select(x => x.MaLinhVuc)
                .Distinct()
                .ToList();

            var linhVucs = await _context.LinhVucs
                .AsNoTracking()
                .Where(x => maLinhVucs.Contains(x.MaLinhVuc))
                .ToDictionaryAsync(x => x.MaLinhVuc);

            foreach (var deTai in danhSachDeTai)
            {
                if (linhVucs.TryGetValue(
                    deTai.MaLinhVuc, out var linhVuc))
                {
                    deTai.LinhVuc = linhVuc;
                }
            }

            ViewBag.SoDaDangKy = ketQua.ToDictionary(
                x => x.DeTai.MaDeTai,
                x => x.SoDaDangKy);

            ViewBag.LinhVucs = await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            ViewBag.NamHocs = await _context.DeTais
                .AsNoTracking()
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.NamHocLoc = namHoc;
            ViewBag.HocKyLoc = hocKy;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.ChoCon = choCon;
            ViewBag.SapXep = sapXep ?? "";

            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tongBanGhi;
            ViewBag.KichThuocTrang = kichThuocTrang;

            return View(danhSachDeTai);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .Include(x => x.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }

        public async Task<IActionResult> Create()
        {
            await LoadLinhVuc();
            return View(new DeTai());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenDeTai,MaLinhVuc,MoTa,YeuCau,SoSinhVienToiDa,NamHoc,HocKy,TrangThai")]
            DeTai deTai)
        {
            ModelState.Remove("LinhVuc");

            await KiemTraDeTai(deTai);

            if (ModelState.IsValid)
            {
                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Thêm đề tài thành công.";
                return RedirectToAction(nameof(Index));
            }

            await LoadLinhVuc(deTai.MaLinhVuc);
            return View(deTai);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais.FindAsync(id);

            if (deTai == null)
                return NotFound();

            await LoadLinhVuc(deTai.MaLinhVuc);
            return View(deTai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaDeTai,TenDeTai,MaLinhVuc,MoTa,YeuCau,SoSinhVienToiDa,NamHoc,HocKy,TrangThai")]
            DeTai deTai)
        {
            if (id != deTai.MaDeTai)
                return NotFound();

            ModelState.Remove("LinhVuc");

            await KiemTraDeTai(deTai, id);

            var deTaiCu = await _context.DeTais
                .FirstOrDefaultAsync(x => x.MaDeTai == id);

            if (deTaiCu == null)
                return NotFound();

            int soDaDangKy = await DemDangKyHopLe(id);

            if (deTai.SoSinhVienToiDa < soDaDangKy)
            {
                ModelState.AddModelError(
                    "SoSinhVienToiDa",
                    $"Số sinh viên tối đa không được nhỏ hơn {soDaDangKy}.");
            }

            if (ModelState.IsValid)
            {
                deTaiCu.TenDeTai = deTai.TenDeTai;
                deTaiCu.MaLinhVuc = deTai.MaLinhVuc;
                deTaiCu.MoTa = deTai.MoTa;
                deTaiCu.YeuCau = deTai.YeuCau;
                deTaiCu.SoSinhVienToiDa = deTai.SoSinhVienToiDa;
                deTaiCu.NamHoc = deTai.NamHoc;
                deTaiCu.HocKy = deTai.HocKy;
                deTaiCu.TrangThai = deTai.TrangThai;

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Cập nhật đề tài thành công.";
                return RedirectToAction(nameof(Index));
            }

            await LoadLinhVuc(deTai.MaLinhVuc);
            return View(deTai);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .Include(x => x.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deTai = await _context.DeTais.FindAsync(id);

            if (deTai == null)
                return NotFound();

            bool coDangKy = await _context.DangKyDeTaiModule2s
                .AnyAsync(x => x.MaDeTai == id);

            if (coDangKy)
            {
                TempData["Loi"] =
                    "Đề tài đã có đăng ký, không thể xóa. Hãy chuyển sang trạng thái Tạm khóa hoặc Kết thúc.";

                return RedirectToAction(nameof(Index));
            }

            _context.DeTais.Remove(deTai);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Xóa đề tài thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int> DemDangKyHopLe(int maDeTai)
        {
            return await _context.DangKyDeTaiModule2s
                .CountAsync(x =>
                    x.MaDeTai == maDeTai &&
                    (x.TrangThai == "Đã duyệt" ||
                     x.TrangThai == "Đang thực hiện"));
        }

        private async Task KiemTraDeTai(
            DeTai deTai,
            int? maDeTaiHienTai = null)
        {
            if (string.IsNullOrWhiteSpace(deTai.TenDeTai))
            {
                ModelState.AddModelError(
                    "TenDeTai",
                    "Tên đề tài không được để trống.");
            }
            else
            {
                deTai.TenDeTai = deTai.TenDeTai.Trim();

                bool trungTen = await _context.DeTais
                    .AnyAsync(x =>
                        x.TenDeTai == deTai.TenDeTai &&
                        (!maDeTaiHienTai.HasValue ||
                         x.MaDeTai != maDeTaiHienTai.Value) &&
                        x.NamHoc == deTai.NamHoc &&
                        x.HocKy == deTai.HocKy);

                if (trungTen)
                {
                    ModelState.AddModelError(
                        "TenDeTai",
                        "Tên đề tài đã tồn tại trong năm học và học kỳ này.");
                }
            }

            bool linhVucTonTai = await _context.LinhVucs
                .AnyAsync(x => x.MaLinhVuc == deTai.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    "MaLinhVuc",
                    "Vui lòng chọn lĩnh vực hợp lệ.");
            }

            if (deTai.SoSinhVienToiDa < 1)
            {
                ModelState.AddModelError(
                    "SoSinhVienToiDa",
                    "Số sinh viên tối đa phải lớn hơn 0.");
            }

            if (deTai.HocKy < 1 || deTai.HocKy > 3)
            {
                ModelState.AddModelError(
                    "HocKy",
                    "Học kỳ phải từ 1 đến 3.");
            }

            if (deTai.TrangThai != "Mở đăng ký" &&
                deTai.TrangThai != "Đủ số lượng" &&
                deTai.TrangThai != "Tạm khóa" &&
                deTai.TrangThai != "Kết thúc")
            {
                ModelState.AddModelError(
                    "TrangThai",
                    "Trạng thái đề tài không hợp lệ.");
            }
        }

        private async Task LoadLinhVuc(int? maLinhVuc = null)
        {
            var linhVucs = await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            ViewBag.MaLinhVuc = new SelectList(
                linhVucs,
                "MaLinhVuc",
                "TenLinhVuc",
                maLinhVuc);

            ViewBag.LinhVucs = new SelectList(
                linhVucs,
                "MaLinhVuc",
                "TenLinhVuc",
                maLinhVuc);
        }
    }
}

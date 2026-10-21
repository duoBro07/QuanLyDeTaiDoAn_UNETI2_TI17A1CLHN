
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
            string? sapXep,
            string? choCon,
            int trang = 1)
        {
            const int kichThuocTrang = 10;

            var query = _context.DeTais
                .AsNoTracking()
                .AsQueryable();

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
            if (maLinhVuc.HasValue)
            {
                query = query.Where(d =>
                    d.MaLinhVuc == maLinhVuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(d =>
                    d.NamHoc == namHoc);
            }
            if (hocKy.HasValue)
            {
                query = query.Where(d =>
                    d.HocKy == hocKy.Value);
            }
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(d =>
                    d.TrangThai == trangThai);
            }

            var thongKe = query.Select(d => new
            {
                DeTai = d,

                SoDaDangKy = _context
                    .DangKyDeTaiModule2s
                    .Count(k =>
                        k.MaDeTai == d.MaDeTai &&
                        (k.TrangThai == "Đã duyệt" ||
                         k.TrangThai == "Đang thực hiện"))
            });

            if (choCon == "con")
            {
                thongKe = thongKe.Where(x =>
                    x.DeTai.SoSinhVienToiDa >
                    x.SoDaDangKy);
            }

   
            if (choCon == "het")
            {
                thongKe = thongKe.Where(x =>
                    x.DeTai.SoSinhVienToiDa <=
                    x.SoDaDangKy);
            }

        
            var querySapXep = sapXep switch
            {
                "ten_za" => thongKe
                    .OrderByDescending(x =>
                        x.DeTai.TenDeTai),

                "sv_tang" => thongKe
                    .OrderBy(x =>
                        x.DeTai.SoSinhVienToiDa),

                "sv_giam" => thongKe
                    .OrderByDescending(x =>
                        x.DeTai.SoSinhVienToiDa),

                "cho_tang" => thongKe
                    .OrderBy(x =>
                        x.DeTai.SoSinhVienToiDa -
                        x.SoDaDangKy),

                "cho_giam" => thongKe
                    .OrderByDescending(x =>
                        x.DeTai.SoSinhVienToiDa -
                        x.SoDaDangKy),

                _ => thongKe
                    .OrderBy(x =>
                        x.DeTai.TenDeTai)
            };

    
            int tongBanGhi =
                await thongKe.CountAsync();

            int tongTrang = (int)Math.Ceiling(
                tongBanGhi / (double)kichThuocTrang);

            trang = Math.Max(1, trang);

            if (tongTrang > 0)
            {
                trang = Math.Min(trang, tongTrang);
            }

     
            var ketQua = await querySapXep
                .ThenBy(x => x.DeTai.MaDeTai)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .Select(x => new
                {
                    x.DeTai,
                    x.SoDaDangKy
                })
                .ToListAsync();

            var danhSach = ketQua
                .Select(x => x.DeTai)
                .ToList();
            var linhVucs = await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(l => l.TenLinhVuc)
                .ToListAsync();

            var linhVucTheoMa = linhVucs
                .ToDictionary(l => l.MaLinhVuc);

            foreach (var deTai in danhSach)
            {
                if (linhVucTheoMa.TryGetValue(
                    deTai.MaLinhVuc, out var linhVuc))
                {
                    deTai.LinhVuc = linhVuc;
                }
            }

            ViewBag.SoDaDangKy = ketQua
                .ToDictionary(
                    x => x.DeTai.MaDeTai,
                    x => x.SoDaDangKy);

            ViewBag.LinhVucs = linhVucs;

            ViewBag.NamHocs = await _context.DeTais
                .AsNoTracking()
                .Select(d => d.NamHoc)
                .Distinct()
                .OrderByDescending(n => n)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVucLoc = maLinhVuc;
            ViewBag.NamHocLoc = namHoc;
            ViewBag.HocKyLoc = hocKy;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.SapXep = sapXep;
            ViewBag.ChoCon = choCon;
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

            var deTai = await _context.DeTais
                .Include(d => d.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }
        public async Task<IActionResult> Create()
        {
            await NapLinhVuc();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenDeTai,MaLinhVuc,MoTa,YeuCau,SoSinhVienToiDa,NamHoc,HocKy,TrangThai")]
            DeTai deTai)
        {
            ChuanHoa(deTai);

            await KiemTraDuLieu(deTai);
            await KiemTraTrungTen(deTai);

            if (ModelState.IsValid)
            {
                _context.DeTais.Add(deTai);

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] =
                        "Thêm đề tài thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể lưu đề tài. Vui lòng kiểm tra dữ liệu.");
                }
            }

            await NapLinhVuc(deTai.MaLinhVuc);

            return View(deTai);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .FindAsync(id);

            if (deTai == null)
                return NotFound();

            await NapLinhVuc(deTai.MaLinhVuc);

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

            ChuanHoa(deTai);

            await KiemTraDuLieu(deTai);
            await KiemTraTrungTen(deTai, id);

            int soDaDangKy = await _context
                .DangKyDeTaiModule2s
                .CountAsync(k =>
                    k.MaDeTai == id &&
                    (k.TrangThai == "Đã duyệt" ||
                     k.TrangThai == "Đang thực hiện"));

            if (deTai.SoSinhVienToiDa < soDaDangKy)
            {
                ModelState.AddModelError(
                    nameof(deTai.SoSinhVienToiDa),
                    $"Số sinh viên tối đa không được nhỏ hơn {soDaDangKy} sinh viên đã đăng ký hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var deTaiCu = await _context.DeTais
                    .FindAsync(id);

                if (deTaiCu == null)
                    return NotFound();

                deTaiCu.TenDeTai = deTai.TenDeTai;
                deTaiCu.MaLinhVuc = deTai.MaLinhVuc;
                deTaiCu.MoTa = deTai.MoTa;
                deTaiCu.YeuCau = deTai.YeuCau;
                deTaiCu.SoSinhVienToiDa =
                    deTai.SoSinhVienToiDa;
                deTaiCu.NamHoc = deTai.NamHoc;
                deTaiCu.HocKy = deTai.HocKy;
                deTaiCu.TrangThai = deTai.TrangThai;

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] =
                        "Cập nhật đề tài thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật đề tài.");
                }
            }

            await NapLinhVuc(deTai.MaLinhVuc);

            return View(deTai);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var deTai = await _context.DeTais
                .Include(d => d.LinhVuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            return View(deTai);
        }
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deTai = await _context.DeTais
                .FindAsync(id);

            if (deTai == null)
                return NotFound();

            bool coDangKy = await _context
                .DangKyDeTaiModule2s
                .AnyAsync(k => k.MaDeTai == id);

            if (coDangKy)
            {
                TempData["Loi"] =
                    "Không thể xóa đề tài đã có đăng ký.";

                return RedirectToAction(nameof(Index));
            }

            _context.DeTais.Remove(deTai);

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    "Xóa đề tài thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] =
                    "Không thể xóa đề tài đang có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
        private void ChuanHoa(DeTai deTai)
        {
            deTai.TenDeTai =
                (deTai.TenDeTai ?? "").Trim();

            deTai.NamHoc =
                (deTai.NamHoc ?? "").Trim();

            deTai.MoTa =
                string.IsNullOrWhiteSpace(deTai.MoTa)
                    ? null
                    : deTai.MoTa.Trim();

            deTai.YeuCau =
                string.IsNullOrWhiteSpace(deTai.YeuCau)
                    ? null
                    : deTai.YeuCau.Trim();

            if (deTai.TenDeTai.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(deTai.TenDeTai),
                    "Vui lòng nhập tên đề tài.");
            }

            if (deTai.NamHoc.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(deTai.NamHoc),
                    "Vui lòng nhập năm học.");
            }
        }
        private async Task KiemTraTrungTen(
            DeTai deTai,
            int? idLoaiTru = null)
        {
            bool trungTen = await _context.DeTais
                .AnyAsync(d =>
                    (!idLoaiTru.HasValue ||
                     d.MaDeTai != idLoaiTru.Value) &&
                    d.TenDeTai == deTai.TenDeTai &&
                    d.NamHoc == deTai.NamHoc &&
                    d.HocKy == deTai.HocKy);

            if (trungTen)
            {
                ModelState.AddModelError(
                    nameof(deTai.TenDeTai),
                    "Tên đề tài đã tồn tại trong năm học và học kỳ này.");
            }
        }
        private async Task KiemTraDuLieu(DeTai deTai)
        {
            bool linhVucTonTai =
                await _context.LinhVucs
                    .AnyAsync(l =>
                        l.MaLinhVuc == deTai.MaLinhVuc);

            if (!linhVucTonTai)
            {
                ModelState.AddModelError(
                    nameof(deTai.MaLinhVuc),
                    "Lĩnh vực không tồn tại.");
            }

            string[] trangThaiHopLe =
            {
                "Mở đăng ký",
                "Đủ số lượng",
                "Tạm khóa",
                "Kết thúc"
            };

            if (!trangThaiHopLe.Contains(deTai.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(deTai.TrangThai),
                    "Trạng thái không hợp lệ.");
            }
        }
        private async Task NapLinhVuc(
            int? selected = null)
        {
            ViewBag.MaLinhVuc = new SelectList(
                await _context.LinhVucs
                    .AsNoTracking()
                    .OrderBy(l => l.TenLinhVuc)
                    .ToListAsync(),
                "MaLinhVuc",
                "TenLinhVuc",
                selected);
        }
    }
}

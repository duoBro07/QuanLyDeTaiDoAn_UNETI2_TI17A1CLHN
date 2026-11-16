
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("SinhVien")]
    public class CongSinhVienController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public CongSinhVienController(
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        private async Task<SinhVien?> LaySinhVien()
        {
            int? maTaiKhoan =
                HttpContext.Session.GetInt32("MaTaiKhoan");

            if (!maTaiKhoan.HasValue)
                return null;

            return await _db.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == maTaiKhoan.Value);
        }

        private bool DungDotDangKy(DeTai deTai)
        {
            string namHoc =
                _configuration["DotDangKy:NamHoc"]?.Trim() ?? "";

            int hocKy = _configuration.GetValue<int>(
                "DotDangKy:HocKy");

            bool dangMo = _configuration.GetValue<bool>(
                "DotDangKy:DangMo");

            return dangMo
                && !string.IsNullOrWhiteSpace(namHoc)
                && string.Equals(
                    deTai.NamHoc?.Trim(),
                    namHoc,
                    StringComparison.OrdinalIgnoreCase)
                && deTai.HocKy == hocKy;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sv = await LaySinhVien();

            ViewBag.HoTen = sv?.HoTen
                ?? HttpContext.Session.GetString("HoTen")
                ?? "Sinh viên";

            ViewBag.SoDeTaiMo = await _db.DeTais
                .CountAsync(x =>
                    x.TrangThai == "Mở đăng ký");

            ViewBag.SoDangKy = sv == null ? 0 :
                await _db.DangKyDeTais.CountAsync(x =>
                    x.MaSinhVien == sv.MaSinhVien);

            ViewBag.ChoDuyet = sv == null ? 0 :
                await _db.DangKyDeTais.CountAsync(x =>
                    x.MaSinhVien == sv.MaSinhVien &&
                    x.TrangThai == "Chờ duyệt");

            ViewBag.DaDuyet = sv == null ? 0 :
                await _db.DangKyDeTais.CountAsync(x =>
                    x.MaSinhVien == sv.MaSinhVien &&
                    x.TrangThai == "Đã duyệt");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var sv = await LaySinhVien();

            if (sv == null)
                return View("ChuaCoHoSo");

            return View(sv);
        }

        [HttpGet]
        public async Task<IActionResult> DeTai(
            string? tuKhoa,
            int? linhVuc,
            string? trangThai,
            string? namHoc,
            int? hocKy,
            string? choCon,
            string? sapXep,
            int trang = 1)
        {
            const int kichThuocTrang = 10;
            trang = Math.Max(1, trang);

            var query = _db.DeTais
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.TenDeTai.Contains(tuKhoa) ||
                    (x.MoTa != null &&
                     x.MoTa.Contains(tuKhoa)) ||
                    (x.YeuCau != null &&
                     x.YeuCau.Contains(tuKhoa)));
            }

            if (linhVuc.HasValue)
            {
                query = query.Where(x =>
                    x.MaLinhVuc == linhVuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(x =>
                    x.NamHoc == namHoc);
            }

            if (hocKy.HasValue)
            {
                query = query.Where(x =>
                    x.HocKy == hocKy.Value);
            }

            var danhSachCoSoLuong = query.Select(x => new
            {
                DeTai = x,
                DaDuyet = _db.DangKyDeTais.Count(d =>
                    d.MaDeTai == x.MaDeTai &&
                    d.TrangThai == "Đã duyệt")
            });

            if (choCon == "con")
            {
                danhSachCoSoLuong =
                    danhSachCoSoLuong.Where(x =>
                        x.DaDuyet < x.DeTai.SoSinhVienToiDa);
            }
            else if (choCon == "het")
            {
                danhSachCoSoLuong =
                    danhSachCoSoLuong.Where(x =>
                        x.DaDuyet >= x.DeTai.SoSinhVienToiDa);
            }

            var danhSachSapXep = sapXep switch
            {
                "ten_za" => danhSachCoSoLuong
                    .OrderByDescending(x => x.DeTai.TenDeTai),

                "toida_tang" => danhSachCoSoLuong
                    .OrderBy(x => x.DeTai.SoSinhVienToiDa),

                "toida_giam" => danhSachCoSoLuong
                    .OrderByDescending(x => x.DeTai.SoSinhVienToiDa),

                "cho_tang" => danhSachCoSoLuong
                    .OrderBy(x =>
                        x.DeTai.SoSinhVienToiDa - x.DaDuyet),

                "cho_giam" => danhSachCoSoLuong
                    .OrderByDescending(x =>
                        x.DeTai.SoSinhVienToiDa - x.DaDuyet),

                _ => danhSachCoSoLuong
                    .OrderBy(x => x.DeTai.TenDeTai)
            };

            int tongBanGhi =
                await danhSachSapXep.CountAsync();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(
                    tongBanGhi / (double)kichThuocTrang));

            trang = Math.Min(trang, tongTrang);

            var danhSach = await danhSachSapXep
                .ThenBy(x => x.DeTai.MaDeTai)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .Select(x => x.DeTai)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.LinhVuc = linhVuc;
            ViewBag.TrangThai = trangThai;
            ViewBag.NamHoc = namHoc;
            ViewBag.HocKy = hocKy;
            ViewBag.ChoCon = choCon;
            ViewBag.SapXep = sapXep;

            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongBanGhi = tongBanGhi;

            ViewBag.DanhSachLinhVuc =
                await _db.LinhVucs
                    .AsNoTracking()
                    .OrderBy(x => x.TenLinhVuc)
                    .ToListAsync();

            ViewBag.DanhSachNamHoc =
                await _db.DeTais
                    .AsNoTracking()
                    .Select(x => x.NamHoc)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var deTai = await _db.DeTais
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .FirstOrDefaultAsync(x => x.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            int daDuyet = await _db.DangKyDeTais
                .CountAsync(x =>
                    x.MaDeTai == id &&
                    x.TrangThai == "Đã duyệt");

            ViewBag.SoDaDangKy = daDuyet;

            ViewBag.SoChoCon = Math.Max(
                0,
                deTai.SoSinhVienToiDa - daDuyet);

            bool dungDot = DungDotDangKy(deTai);

            ViewBag.DungDotDangKy = dungDot;

            if (!dungDot)
            {
                string namHocMo =
                    _configuration["DotDangKy:NamHoc"]?.Trim() ?? "";

                int hocKyMo =
                    _configuration.GetValue<int>("DotDangKy:HocKy");

                bool dangMo =
                    _configuration.GetValue<bool>("DotDangKy:DangMo");

                ViewBag.ThongBaoDotDangKy = !dangMo
                    ? "Admin chưa mở đợt đăng ký."
                    : $"Đợt đang mở: {namHocMo}, học kỳ {hocKyMo}. " +
                      $"Đề tài: {deTai.NamHoc}, học kỳ {deTai.HocKy}.";
            }
            else
            {
                ViewBag.ThongBaoDotDangKy = "";
            }

            // Kiểm tra sinh viên hiện tại
            var sinhVien = await LaySinhVien();

            bool daDangKyChoDuyet = false;
            bool daDuocDuyetDeTaiNay = false;
            bool daCoDeTaiDuocDuyet = false;

            if (sinhVien != null)
            {
                // Đã đăng ký đề tài này và đang chờ duyệt
                daDangKyChoDuyet = await _db.DangKyDeTais
                    .AnyAsync(x =>
                        x.MaSinhVien == sinhVien.MaSinhVien &&
                        x.MaDeTai == id &&
                        x.TrangThai == "Chờ duyệt");

                // Đã được duyệt chính đề tài này
                daDuocDuyetDeTaiNay = await _db.DangKyDeTais
                    .AnyAsync(x =>
                        x.MaSinhVien == sinhVien.MaSinhVien &&
                        x.MaDeTai == id &&
                        x.TrangThai == "Đã duyệt");

                // Đã được duyệt đề tài khác cùng năm học, học kỳ
                daCoDeTaiDuocDuyet = await _db.DangKyDeTais
                    .AnyAsync(x =>
                        x.MaSinhVien == sinhVien.MaSinhVien &&
                        x.TrangThai == "Đã duyệt" &&
                        x.DeTai != null &&
                        x.DeTai.NamHoc == deTai.NamHoc &&
                        x.DeTai.HocKy == deTai.HocKy);
            }

            ViewBag.DaDangKyChoDuyet = daDangKyChoDuyet;
            ViewBag.DaDuocDuyetDeTaiNay = daDuocDuyetDeTaiNay;
            ViewBag.DaCoDeTaiDuocDuyet = daCoDeTaiDuocDuyet;

            return View(deTai);
        }

        [HttpGet]
        public async Task<IActionResult> PhanCongCuaToi()
        {
            var sv = await LaySinhVien();

            if (sv == null)
                return View("ChuaCoHoSo");

            var danhSach =
                await _db.PhanCongHuongDans
                    .AsNoTracking()
                    .Include(x => x.GiangVien)
                    .Include(x => x.DangKyDeTai)
                        .ThenInclude(x => x!.DeTai)
                            .ThenInclude(x => x!.LinhVuc)
                    .Where(x =>
                        x.DangKyDeTai != null &&
                        x.DangKyDeTai.MaSinhVien ==
                            sv.MaSinhVien)
                    .OrderByDescending(x => x.DangHieuLuc)
                    .ThenByDescending(x => x.NgayPhanCong)
                    .ToListAsync();

            ViewBag.HoTenSinhVien = sv.HoTen;

            return View(danhSach);
        }
    }
}

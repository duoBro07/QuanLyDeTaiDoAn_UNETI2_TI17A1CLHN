
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ThongKeController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? namHoc,
            int? hocKy,
            int[]? hocKys)
        {
            namHoc = namHoc?.Trim() ?? "";

            if (hocKy.HasValue &&
                (hocKy.Value < 1 || hocKy.Value > 3))
            {
                hocKy = null;
            }

            // Cho phép chọn nhiều học kỳ:
            // ?namHoc=2025-2026&hocKys=1&hocKys=2

            int[] kyDuocChon = (hocKys ?? Array.Empty<int>())
                .Where(x => x >= 1 && x <= 3)
                .Distinct()
                .ToArray();

            if (kyDuocChon.Length == 0 && hocKy.HasValue)
            {
                kyDuocChon = new[] { hocKy.Value };
            }

            // 1. Danh sách năm học

            var danhSachNamHoc = await _db.DeTais
                .AsNoTracking()
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            // 2. Lọc đề tài

            IQueryable<DeTai> deTaiQuery =
                _db.DeTais.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                deTaiQuery = deTaiQuery.Where(x =>
                    x.NamHoc == namHoc);
            }

            if (kyDuocChon.Length > 0)
            {
                deTaiQuery = deTaiQuery.Where(x =>
                    kyDuocChon.Contains(x.HocKy));
            }

            // 3. Lọc đăng ký theo đề tài

            IQueryable<DangKyDeTai> dangKyQuery =
                _db.DangKyDeTais
                    .AsNoTracking()
                    .Where(x => deTaiQuery.Any(d =>
                        d.MaDeTai == x.MaDeTai));

            // 4. Lọc phân công theo đăng ký

            IQueryable<PhanCongHuongDan> phanCongQuery =
                _db.PhanCongHuongDans
                    .AsNoTracking()
                    .Where(x => dangKyQuery.Any(d =>
                        d.MaDangKy == x.MaDangKy));

            // Phân công còn hiệu lực và chưa hoàn thành

            IQueryable<PhanCongHuongDan> dangHuongDanQuery =
                phanCongQuery.Where(x =>
                    x.DangHieuLuc &&
                    x.TrangThaiThucHien != "Hoàn thành");

            // 5. Thống kê tổng quan

            var model = new ThongKeViewModel
            {
                NamHoc = namHoc,

                HocKy = kyDuocChon.Length == 1
                    ? kyDuocChon[0]
                    : null,

                DanhSachNamHoc = danhSachNamHoc,

                TongSinhVien = await _db.SinhViens
                    .AsNoTracking()
                    .CountAsync(),

                TongGiangVien = await _db.GiangViens
                    .AsNoTracking()
                    .CountAsync(),

                TongDeTai = await deTaiQuery.CountAsync(),

                TongDangKy = await dangKyQuery.CountAsync(),

                ChoDuyet = await dangKyQuery.CountAsync(x =>
                    x.TrangThai == "Chờ duyệt"),

                DaDuyet = await dangKyQuery.CountAsync(x =>
                    x.TrangThai == "Đã duyệt"),

                TuChoi = await dangKyQuery.CountAsync(x =>
                    x.TrangThai == "Từ chối"),

                DangHuongDan = await dangHuongDanQuery
                    .CountAsync()
            };

            // 6. Đề tài theo lĩnh vực

            model.TheoLinhVuc = await deTaiQuery
                .GroupBy(x => x.LinhVuc!.TenLinhVuc)
                .Select(g => new ThongKeDong
                {
                    Ten = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Ten)
                .ToListAsync();

            // 7. Đăng ký theo trạng thái

            model.TheoTrangThai = await dangKyQuery
                .GroupBy(x => x.TrangThai)
                .Select(g => new ThongKeDong
                {
                    Ten = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Ten)
                .ToListAsync();

            // 8. Đăng ký theo tháng

            var dangKyTheoThang = await dangKyQuery
                .GroupBy(x => new
                {
                    x.NgayDangKy.Year,
                    x.NgayDangKy.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    SoLuong = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            model.TheoThang = dangKyTheoThang
                .Select(x => new ThongKeDong
                {
                    Ten = $"{x.Month:00}/{x.Year}",
                    SoLuong = x.SoLuong
                })
                .ToList();

            // 9. Đề tài theo năm học / học kỳ

            var deTaiTheoNamHoc = await deTaiQuery
                .GroupBy(x => new
                {
                    x.NamHoc,
                    x.HocKy
                })
                .Select(g => new
                {
                    g.Key.NamHoc,
                    g.Key.HocKy,
                    SoLuong = g.Count()
                })
                .OrderBy(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ToListAsync();

            model.TheoNamHoc = deTaiTheoNamHoc
                .Select(x => new ThongKeDong
                {
                    Ten = $"{x.NamHoc} - HK{x.HocKy}",
                    SoLuong = x.SoLuong
                })
                .ToList();

            // 10. Sinh viên theo đề tài
            // Chỉ đăng ký đã duyệt chiếm chỗ

            model.TheoDeTai = await deTaiQuery
                .Select(x => new ThongKeDeTaiDong
                {
                    Ten = x.TenDeTai,

                    LinhVuc = x.LinhVuc != null
                        ? x.LinhVuc.TenLinhVuc
                        : "Chưa xác định",

                    NamHoc = x.NamHoc,
                    HocKy = x.HocKy,
                    ToiDa = x.SoSinhVienToiDa,

                    SoSinhVien = _db.DangKyDeTais.Count(d =>
                        d.MaDeTai == x.MaDeTai &&
                        d.TrangThai == "Đã duyệt"),

                    SoChoCon = x.SoSinhVienToiDa -
                        _db.DangKyDeTais.Count(d =>
                            d.MaDeTai == x.MaDeTai &&
                            d.TrangThai == "Đã duyệt")
                })
                .OrderBy(x => x.Ten)
                .ThenBy(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ToListAsync();

            foreach (var deTai in model.TheoDeTai)
            {
                deTai.SoChoCon = Math.Max(0, deTai.SoChoCon);
            }

            // 11. Tải hướng dẫn giảng viên
            // Theo năm học và học kỳ được lọc

            model.TheoGiangVien = await _db.GiangViens
                .AsNoTracking()
                .Select(x => new ThongKeGiangVienDong
                {
                    Ten = x.HoTen,

                    LinhVuc = x.LinhVuc != null
                        ? x.LinhVuc.TenLinhVuc
                        : "Chưa xác định",

                    ToiDa = x.SoSinhVienHuongDanToiDa,

                    DangHuongDan = dangHuongDanQuery.Count(p =>
                        p.MaGiangVien == x.MaGiangVien)
                })
                .OrderByDescending(x => x.DangHuongDan)
                .ThenBy(x => x.Ten)
                .ToListAsync();

            // 12. Đề tài còn chỗ / hết chỗ

            model.DeTaiConCho = model.TheoDeTai.Count(x =>
                x.ToiDa > 0 &&
                x.SoSinhVien < x.ToiDa);

            model.DeTaiHetCho = model.TheoDeTai.Count(x =>
                x.ToiDa > 0 &&
                x.SoSinhVien >= x.ToiDa);

            // 13. Giảng viên đầy tải toàn hệ thống

            var taiToanHeThong = await _db.GiangViens
                .AsNoTracking()
                .Select(x => new
                {
                    x.MaGiangVien,
                    x.SoSinhVienHuongDanToiDa,

                    Tai = _db.PhanCongHuongDans.Count(p =>
                        p.MaGiangVien == x.MaGiangVien &&
                        p.DangHieuLuc &&
                        p.TrangThaiThucHien != "Hoàn thành")
                })
                .ToListAsync();

            model.GiangVienDayTai = taiToanHeThong.Count(x =>
                x.SoSinhVienHuongDanToiDa > 0 &&
                x.Tai >= x.SoSinhVienHuongDanToiDa);

            return View(model);
        }
    }
}


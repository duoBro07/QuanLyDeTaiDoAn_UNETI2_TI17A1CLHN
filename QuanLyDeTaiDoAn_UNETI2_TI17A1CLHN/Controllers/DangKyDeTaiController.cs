
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("SinhVien")]
    public class DangKyDeTaiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public DangKyDeTaiController(
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        // Lấy sinh viên từ tài khoản đang đăng nhập.
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

        // Lấy đợt đăng ký hiện tại.
        private bool LayKyDangKy(
            out string namHoc,
            out int hocKy)
        {
            namHoc = _configuration[
                "DotDangKy:NamHoc"]?.Trim() ?? "";

            hocKy = _configuration.GetValue<int>(
                "DotDangKy:HocKy");

            bool dangMo = _configuration.GetValue<bool>(
                "DotDangKy:DangMo");

            return dangMo &&
                   !string.IsNullOrWhiteSpace(namHoc) &&
                   hocKy >= 1 &&
                   hocKy <= 3;
        }

        // Kiểm tra đề tài thuộc đợt đang mở.
        private bool DungKyDangKy(DeTai deTai)
        {
            if (!LayKyDangKy(
                out string namHoc,
                out int hocKy))
            {
                return false;
            }

            return string.Equals(
                deTai.NamHoc?.Trim(),
                namHoc,
                StringComparison.OrdinalIgnoreCase)
                && deTai.HocKy == hocKy;
        }

        // Đếm sinh viên đã được duyệt.
        private async Task<int> DemDaDuyet(int maDeTai)
        {
            return await _db.DangKyDeTais
                .CountAsync(x =>
                    x.MaDeTai == maDeTai &&
                    x.TrangThai == "Đã duyệt");
        }

        // Một sinh viên không có hai đề tài đã duyệt
        // trong cùng năm học và học kỳ.
        private async Task<bool> DaCoDeTaiDuocDuyet(
            int maSinhVien,
            string namHoc,
            int hocKy)
        {
            var danhSach = await _db.DangKyDeTais
                .AsNoTracking()
                .Where(x =>
                    x.MaSinhVien == maSinhVien &&
                    x.TrangThai == "Đã duyệt" &&
                    x.DeTai != null &&
                    x.DeTai.HocKy == hocKy)
                .Select(x => x.DeTai!.NamHoc)
                .ToListAsync();

            return danhSach.Any(x =>
                string.Equals(
                    x?.Trim(),
                    namHoc.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        // Không đăng ký chờ duyệt trùng cùng đề tài.
        private async Task<bool> DaChoDuyetCungDeTai(
            int maSinhVien,
            int maDeTai)
        {
            return await _db.DangKyDeTais
                .AnyAsync(x =>
                    x.MaSinhVien == maSinhVien &&
                    x.MaDeTai == maDeTai &&
                    x.TrangThai == "Chờ duyệt");
        }

        // Danh sách đăng ký của chính sinh viên.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sinhVien = await LaySinhVien();

            if (sinhVien == null)
            {
                return RedirectToAction(
                    "HoSo", "CongSinhVien");
            }

            var danhSach = await _db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.DeTai)
                    .ThenInclude(x => x!.LinhVuc)
                .Where(x =>
                    x.MaSinhVien == sinhVien.MaSinhVien)
                .OrderByDescending(x => x.NgayDangKy)
                .ThenByDescending(x => x.MaDangKy)
                .ToListAsync();

            return View(danhSach);
        }

        // Hiển thị form đăng ký.
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var sinhVien = await LaySinhVien();

            if (sinhVien == null)
            {
                return RedirectToAction(
                    "HoSo", "CongSinhVien");
            }

            if (!sinhVien.TrangThai)
            {
                TempData["Error"] =
                    "Hồ sơ sinh viên không hoạt động.";

                return RedirectToAction(
                    "DeTai", "CongSinhVien");
            }

            var deTai = await _db.DeTais
                .AsNoTracking()
                .Include(x => x.LinhVuc)
                .FirstOrDefaultAsync(x =>
                    x.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            if (!DungKyDangKy(deTai))
            {
                TempData["Error"] =
                    "Đề tài không thuộc đợt đăng ký đang mở. " +
                    $"Đề tài: {deTai.NamHoc}, học kỳ {deTai.HocKy}.";

                return RedirectToAction(
                    "DeTai", "CongSinhVien");
            }

            if (deTai.TrangThai != "Mở đăng ký")
            {
                TempData["Error"] =
                    "Đề tài hiện không mở đăng ký.";

                return RedirectToAction(
                    "DeTai", "CongSinhVien");
            }

            int soDaDuyet =
                await DemDaDuyet(deTai.MaDeTai);

            if (deTai.SoSinhVienToiDa <= 0 ||
                soDaDuyet >= deTai.SoSinhVienToiDa)
            {
                TempData["Error"] =
                    "Đề tài đã đủ số lượng sinh viên.";

                return RedirectToAction(
                    "DeTai", "CongSinhVien");
            }

            bool daCoDeTai = await DaCoDeTaiDuocDuyet(
                sinhVien.MaSinhVien,
                deTai.NamHoc,
                deTai.HocKy);

            if (daCoDeTai)
            {
                TempData["Error"] =
                    "Bạn đã có một đề tài được duyệt " +
                    "trong năm học và học kỳ này.";

                return RedirectToAction(nameof(Index));
            }

            bool trungDangKy = await DaChoDuyetCungDeTai(
                sinhVien.MaSinhVien,
                deTai.MaDeTai);

            if (trungDangKy)
            {
                TempData["Error"] =
                    "Bạn đã đăng ký đề tài này " +
                    "và đang chờ xét duyệt.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.DeTai = deTai;
            ViewBag.SoChoCon = Math.Max(
                0,
                deTai.SoSinhVienToiDa - soDaDuyet);

            ViewBag.LyDoDangKy = "";

            return View();
        }

        // Gửi đăng ký.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int id,
            string? lyDoDangKy)
        {
            var sinhVien = await LaySinhVien();

            if (sinhVien == null)
            {
                return RedirectToAction(
                    "HoSo", "CongSinhVien");
            }

            lyDoDangKy = lyDoDangKy?.Trim();

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var deTai = await _db.DeTais
                .Include(x => x.LinhVuc)
                .FirstOrDefaultAsync(x =>
                    x.MaDeTai == id);

            if (deTai == null)
                return NotFound();

            ViewBag.DeTai = deTai;
            ViewBag.LyDoDangKy = lyDoDangKy ?? "";

            int soDaDuyet =
                await DemDaDuyet(deTai.MaDeTai);

            ViewBag.SoChoCon = Math.Max(
                0,
                deTai.SoSinhVienToiDa - soDaDuyet);

            if (string.IsNullOrWhiteSpace(lyDoDangKy))
            {
                ModelState.AddModelError(
                    "",
                    "Vui lòng nhập lý do đăng ký.");
            }
            else if (lyDoDangKy.Length > 1000)
            {
                ModelState.AddModelError(
                    "",
                    "Lý do đăng ký không được vượt quá 1000 ký tự.");
            }

            if (!sinhVien.TrangThai)
            {
                ModelState.AddModelError(
                    "",
                    "Hồ sơ sinh viên đã ngừng hoạt động.");
            }

            if (!DungKyDangKy(deTai))
            {
                ModelState.AddModelError(
                    "",
                    "Đợt đăng ký chưa mở hoặc đề tài " +
                    "không thuộc năm học/học kỳ hiện tại.");
            }

            if (deTai.TrangThai != "Mở đăng ký")
            {
                ModelState.AddModelError(
                    "",
                    "Đề tài không ở trạng thái Mở đăng ký.");
            }

            if (deTai.SoSinhVienToiDa <= 0 ||
                soDaDuyet >= deTai.SoSinhVienToiDa)
            {
                ModelState.AddModelError(
                    "",
                    "Đề tài đã đủ số lượng " +
                    "hoặc sức chứa không hợp lệ.");
            }

            bool daCoDeTai = await DaCoDeTaiDuocDuyet(
                sinhVien.MaSinhVien,
                deTai.NamHoc,
                deTai.HocKy);

            if (daCoDeTai)
            {
                ModelState.AddModelError(
                    "",
                    "Bạn đã có đề tài được duyệt " +
                    "trong năm học/học kỳ này.");
            }

            bool trungDangKy = await DaChoDuyetCungDeTai(
                sinhVien.MaSinhVien,
                deTai.MaDeTai);

            if (trungDangKy)
            {
                ModelState.AddModelError(
                    "",
                    "Bạn đã có đăng ký chờ duyệt " +
                    "cho chính đề tài này.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var dangKy = new DangKyDeTai
            {
                MaSinhVien = sinhVien.MaSinhVien,
                MaDeTai = deTai.MaDeTai,
                NgayDangKy = DateTime.Now,
                LyDoDangKy = lyDoDangKy!,
                TrangThai = "Chờ duyệt",
                GhiChuDuyet = null
            };

            _db.DangKyDeTais.Add(dangKy);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đăng ký đề tài thành công. " +
                "Vui lòng chờ Admin xét duyệt.";

            return RedirectToAction(nameof(Index));
        }

        // Sinh viên chỉ được hủy đăng ký Chờ duyệt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var sinhVien = await LaySinhVien();

            if (sinhVien == null)
            {
                return RedirectToAction(
                    "HoSo", "CongSinhVien");
            }

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var dangKy = await _db.DangKyDeTais
                .FirstOrDefaultAsync(x =>
                    x.MaDangKy == id &&
                    x.MaSinhVien == sinhVien.MaSinhVien);

            if (dangKy == null)
                return NotFound();

            if (dangKy.TrangThai != "Chờ duyệt")
            {
                TempData["Error"] =
                    "Chỉ được hủy đăng ký " +
                    "khi đang ở trạng thái Chờ duyệt.";

                return RedirectToAction(nameof(Index));
            }

            dangKy.TrangThai = "Đã hủy";

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã hủy đăng ký thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class DuyetDangKyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DuyetDangKyController> _logger;

        public DuyetDangKyController(
            ApplicationDbContext db,
            ILogger<DuyetDangKyController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =====================================================
        // 1. DANH SACH DANG KY
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? trangThai,
            string? tuKhoa,
            int? maLinhVuc)
        {
            trangThai = trangThai?.Trim();
            tuKhoa = tuKhoa?.Trim();

            var query = _db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.SinhVien)
                .Include(x => x.DeTai)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    (x.SinhVien != null &&
                     x.SinhVien.HoTen.Contains(tuKhoa)) ||
                    x.MaSinhVien.ToString().Contains(tuKhoa) ||
                    (x.DeTai != null &&
                     x.DeTai.TenDeTai.Contains(tuKhoa)));
            }

            if (maLinhVuc.HasValue && maLinhVuc.Value > 0)
            {
                query = query.Where(x =>
                    x.DeTai != null &&
                    x.DeTai.MaLinhVuc == maLinhVuc.Value);
            }

            ViewBag.TrangThai = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVuc = maLinhVuc;

            ViewBag.LinhVucs = await _db.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            ViewBag.TongDangKy = await _db.DangKyDeTais
                .CountAsync();

            ViewBag.ChoDuyet = await _db.DangKyDeTais
                .CountAsync(x => x.TrangThai == "Chờ duyệt");

            ViewBag.DaDuyet = await _db.DangKyDeTais
                .CountAsync(x => x.TrangThai == "Đã duyệt");

            ViewBag.TuChoi = await _db.DangKyDeTais
                .CountAsync(x => x.TrangThai == "Từ chối");

            ViewBag.DaHuy = await _db.DangKyDeTais
                .CountAsync(x => x.TrangThai == "Đã hủy");

            var danhSach = await query
                .OrderByDescending(x => x.NgayDangKy)
                .ThenByDescending(x => x.MaDangKy)
                .ToListAsync();

            return View(danhSach);
        }

        // =====================================================
        // 2. CHI TIET DANG KY
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var dangKy = await _db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.DeTai)
                    .ThenInclude(x => x!.LinhVuc)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKy == null)
                return NotFound();

            // Lay sinh vien truc tiep theo ma sinh vien
            // tren ho so dang ky.
            var sinhVien = await _db.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaSinhVien == dangKy.MaSinhVien);

            // Gan vao navigation de View hien thi thong tin.
            dangKy.SinhVien = sinhVien;

            bool sinhVienHopLe =
                sinhVien != null && sinhVien.TrangThai;

            bool taiKhoanHopLe = false;

            if (sinhVien != null)
            {
                taiKhoanHopLe = await _db.TaiKhoans
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaTaiKhoan == sinhVien.MaTaiKhoan &&
                        x.TrangThai &&
                        x.VaiTro == "SinhVien");
            }

            ViewBag.SinhVienHopLe = sinhVienHopLe;
            ViewBag.TaiKhoanHopLe = taiKhoanHopLe;

            // Thong bao chi tiet de tranh nham lan
            // giua trang thai tai khoan va ho so sinh vien.
            if (sinhVien == null)
            {
                ViewBag.LoiSinhVien =
                    "Đăng ký mã " + dangKy.MaDangKy +
                    " đang liên kết với mã sinh viên " +
                    dangKy.MaSinhVien +
                    ", nhưng không tìm thấy hồ sơ sinh viên tương ứng.";
            }
            else if (!sinhVien.TrangThai)
            {
                ViewBag.LoiSinhVien =
                    "Hồ sơ sinh viên " + sinhVien.HoTen +
                    " (mã " + sinhVien.MaSinhVien +
                    ") đang ở trạng thái ngừng hoạt động.";
            }
            else if (!taiKhoanHopLe)
            {
                ViewBag.LoiSinhVien =
                    "Hồ sơ sinh viên đang hoạt động, " +
                    "nhưng tài khoản liên kết không tồn tại, " +
                    "đã bị khóa hoặc không có vai trò SinhVien.";
            }
            else
            {
                ViewBag.LoiSinhVien = null;
            }

            // Phan cong dang hieu luc
            ViewBag.PhanCongHienTai =
                await _db.PhanCongHuongDans
                    .AsNoTracking()
                    .Include(x => x.GiangVien)
                    .FirstOrDefaultAsync(x =>
                        x.MaDangKy == id &&
                        x.DangHieuLuc);

            bool daCoPhanCong = await _db.PhanCongHuongDans
                .AsNoTracking()
                .AnyAsync(x => x.MaDangKy == id);

            ViewBag.DaCoPhanCong = daCoPhanCong;

            int soDaDuyet = 0;
            int soChoCon = 0;
            bool duSoLuong = true;
            bool daCoDeTaiKhac = false;
            bool linhVucHopLe = false;

            if (dangKy.DeTai != null)
            {
                var deTai = dangKy.DeTai;

                soDaDuyet = await _db.DangKyDeTais
                    .CountAsync(x =>
                        x.MaDeTai == deTai.MaDeTai &&
                        x.TrangThai == "Đã duyệt");

                soChoCon = Math.Max(
                    0,
                    deTai.SoSinhVienToiDa - soDaDuyet);

                duSoLuong =
                    deTai.SoSinhVienToiDa <= 0 ||
                    soDaDuyet >= deTai.SoSinhVienToiDa;

                daCoDeTaiKhac =
                    await _db.DangKyDeTais.AnyAsync(x =>
                        x.MaDangKy != id &&
                        x.MaSinhVien == dangKy.MaSinhVien &&
                        x.TrangThai == "Đã duyệt" &&
                        x.DeTai != null &&
                        x.DeTai.NamHoc == deTai.NamHoc &&
                        x.DeTai.HocKy == deTai.HocKy);

                linhVucHopLe =
                    await _db.LinhVucs.AnyAsync(x =>
                        x.MaLinhVuc == deTai.MaLinhVuc &&
                        x.TrangThai);
            }

            ViewBag.SoDaDuyet = soDaDuyet;
            ViewBag.SoChoCon = soChoCon;
            ViewBag.DuSoLuong = duSoLuong;
            ViewBag.DaCoDeTaiKhac = daCoDeTaiKhac;
            ViewBag.LinhVucHopLe = linhVucHopLe;

            return View(dangKy);
        }

        // =====================================================
        // 3. DUYET DANG KY
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var dangKy = await _db.DangKyDeTais
                    .Include(x => x.DeTai)
                    .FirstOrDefaultAsync(x =>
                        x.MaDangKy == id);

                if (dangKy == null)
                    return NotFound();

                // 1. Trang thai dang ky
                if (dangKy.TrangThai != "Chờ duyệt")
                {
                    TempData["Error"] =
                        "Đăng ký đã được xử lý hoặc đã hủy. " +
                        "Không thể duyệt lại.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 2. Lay sinh vien theo khoa ngoai thuc te
                var sinhVien = await _db.SinhViens
                    .FirstOrDefaultAsync(x =>
                        x.MaSinhVien == dangKy.MaSinhVien);

                if (sinhVien == null)
                {
                    TempData["Error"] =
                        "Không tìm thấy hồ sơ sinh viên có mã " +
                        dangKy.MaSinhVien +
                        " được liên kết với đăng ký này.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                if (!sinhVien.TrangThai)
                {
                    TempData["Error"] =
                        "Hồ sơ sinh viên " + sinhVien.HoTen +
                        " đang ngừng hoạt động. Không thể duyệt.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 3. Kiem tra tai khoan
                bool taiKhoanHopLe =
                    await _db.TaiKhoans.AnyAsync(x =>
                        x.MaTaiKhoan == sinhVien.MaTaiKhoan &&
                        x.TrangThai &&
                        x.VaiTro == "SinhVien");

                if (!taiKhoanHopLe)
                {
                    TempData["Error"] =
                        "Tài khoản liên kết với sinh viên " +
                        "không tồn tại, bị khóa hoặc sai vai trò.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 4. Kiem tra de tai
                var deTai = dangKy.DeTai;

                if (deTai == null)
                {
                    TempData["Error"] =
                        "Không tìm thấy đề tài của đăng ký.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                if (deTai.TrangThai != "Mở đăng ký")
                {
                    TempData["Error"] =
                        "Đề tài không còn ở trạng thái Mở đăng ký.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                if (deTai.SoSinhVienToiDa <= 0)
                {
                    TempData["Error"] =
                        "Sức chứa đề tài không hợp lệ.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 5. Kiem tra linh vuc
                bool linhVucHopLe =
                    await _db.LinhVucs.AnyAsync(x =>
                        x.MaLinhVuc == deTai.MaLinhVuc &&
                        x.TrangThai);

                if (!linhVucHopLe)
                {
                    TempData["Error"] =
                        "Lĩnh vực đề tài không còn hoạt động.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 6. Kiem tra so cho con
                int soDaDuyet =
                    await _db.DangKyDeTais.CountAsync(x =>
                        x.MaDeTai == deTai.MaDeTai &&
                        x.TrangThai == "Đã duyệt");

                int soChoCon =
                    deTai.SoSinhVienToiDa - soDaDuyet;

                if (soChoCon <= 0)
                {
                    TempData["Error"] =
                        "Đề tài đã đủ số lượng sinh viên. " +
                        "Không thể duyệt thêm.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 7. Khong duyet hai de tai cung dot
                bool daCoDeTaiKhac =
                    await _db.DangKyDeTais.AnyAsync(x =>
                        x.MaDangKy != id &&
                        x.MaSinhVien == dangKy.MaSinhVien &&
                        x.TrangThai == "Đã duyệt" &&
                        x.DeTai != null &&
                        x.DeTai.NamHoc == deTai.NamHoc &&
                        x.DeTai.HocKy == deTai.HocKy);

                if (daCoDeTaiKhac)
                {
                    TempData["Error"] =
                        "Sinh viên đã có đề tài khác " +
                        "được duyệt trong cùng năm học và học kỳ.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 8. Khong duyet neu da phat sinh phan cong
                bool daCoPhanCong =
                    await _db.PhanCongHuongDans.AnyAsync(x =>
                        x.MaDangKy == id);

                if (daCoPhanCong)
                {
                    TempData["Error"] =
                        "Đăng ký đã phát sinh dữ liệu phân công. " +
                        "Vui lòng kiểm tra lịch sử trước khi duyệt.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // 9. Duyet dang ky
                dangKy.TrangThai = "Đã duyệt";
                dangKy.GhiChuDuyet =
                    "Đăng ký được chấp nhận.";

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Duyệt đăng ký thành công.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi duyệt đăng ký {MaDangKy}",
                    id);

                TempData["Error"] =
                    "Không thể duyệt đăng ký. " +
                    "Dữ liệu có thể đã thay đổi hoặc " +
                    "đang được xử lý. " +
                    "Vui lòng tải lại trang và thử lại.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
        }

        // =====================================================
        // 4. TU CHOI DANG KY
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(
            int id,
            string? lyDo)
        {
            if (id <= 0)
                return BadRequest();

            lyDo = lyDo?.Trim();

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do từ chối.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            if (lyDo.Length > 1000)
            {
                TempData["Error"] =
                    "Lý do từ chối không được vượt quá 1000 ký tự.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var dangKy = await _db.DangKyDeTais
                    .FirstOrDefaultAsync(x =>
                        x.MaDangKy == id);

                if (dangKy == null)
                    return NotFound();

                if (dangKy.TrangThai != "Chờ duyệt")
                {
                    TempData["Error"] =
                        "Chỉ được từ chối đăng ký " +
                        "đang ở trạng thái Chờ duyệt.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                bool daCoPhanCong =
                    await _db.PhanCongHuongDans.AnyAsync(x =>
                        x.MaDangKy == id);

                if (daCoPhanCong)
                {
                    TempData["Error"] =
                        "Đăng ký đã có lịch sử phân công. " +
                        "Không thể từ chối trực tiếp.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                dangKy.TrangThai = "Từ chối";
                dangKy.GhiChuDuyet = lyDo;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Đã từ chối đăng ký thành công.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi từ chối đăng ký {MaDangKy}",
                    id);

                TempData["Error"] =
                    "Không thể từ chối đăng ký. " +
                    "Vui lòng tải lại và thử lại.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
        }
    }
}

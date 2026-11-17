
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [PhanQuyen("Admin")]
    public class PhanCongController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PhanCongController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 1. DANH SÁCH PHÂN CÔNG
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var danhSach = await _db.PhanCongHuongDans
                .AsNoTracking()
                .Include(x => x.GiangVien)
                .Include(x => x.DangKyDeTai)
                    .ThenInclude(x => x!.SinhVien)
                .Include(x => x.DangKyDeTai)
                    .ThenInclude(x => x!.DeTai)
                .Where(x => x.DangHieuLuc ||
                    (x.TrangThaiThucHien == "Hoàn thành" &&
                     x.NgayKetThuc != null))
                .OrderByDescending(x => x.NgayPhanCong)
                .ToListAsync();

            var chuaPhanCong = await _db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.SinhVien)
                .Include(x => x.DeTai)
                .Where(x =>
                    x.TrangThai == "Đã duyệt" &&
                    !_db.PhanCongHuongDans.Any(p =>
                        p.MaDangKy == x.MaDangKy))
                .OrderByDescending(x => x.NgayDangKy)
                .ToListAsync();

            ViewBag.ChuaPhanCong = chuaPhanCong;

            return View(danhSach);
        }

        // =====================================================
        // 2. LẤY GIẢNG VIÊN ĐÚNG LĨNH VỰC VÀ CÒN TẢI
        // =====================================================
        private async Task<List<GiangVien>> LayGiangVienPhuHop(
            int maLinhVuc)
        {
            var giangViens = await _db.GiangViens
                .AsNoTracking()
                .Where(x =>
                    x.MaLinhVuc == maLinhVuc &&
                    x.TrangThai == "Đang công tác" &&
                    x.SoSinhVienHuongDanToiDa > 0)
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var taiHuongDan = await _db.PhanCongHuongDans
                .AsNoTracking()
                .Where(x => x.DangHieuLuc)
                .GroupBy(x => x.MaGiangVien)
                .Select(g => new
                {
                    MaGiangVien = g.Key,
                    SoLuong = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.MaGiangVien,
                    x => x.SoLuong);

            return giangViens
                .Where(x =>
                    taiHuongDan.GetValueOrDefault(x.MaGiangVien)
                    < x.SoSinhVienHuongDanToiDa)
                .ToList();
        }

        // =====================================================
        // 3. FORM PHÂN CÔNG
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var dangKy = await _db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.SinhVien)
                .Include(x => x.DeTai)
                    .ThenInclude(x => x!.LinhVuc)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKy == null)
                return NotFound();

            if (dangKy.TrangThai != "Đã duyệt")
            {
                TempData["Error"] =
                    "Chỉ được phân công cho đăng ký đã duyệt.";

                return RedirectToAction(nameof(Index));
            }

            if (dangKy.DeTai == null)
                return NotFound();

            if (await _db.PhanCongHuongDans.AnyAsync(p =>
                p.MaDangKy == id &&
                p.TrangThaiThucHien == "Hoàn thành"))
            {
                TempData["Error"] =
                    "Đồ án đã hoàn thành, không thể phân công lại.";

                return RedirectToAction(nameof(Index));
            }

            if (dangKy.SinhVien == null ||
                !dangKy.SinhVien.TrangThai ||
                dangKy.DeTai.TrangThai == "Tạm khóa" ||
                dangKy.DeTai.TrangThai == "Kết thúc")
            {
                TempData["Error"] =
                    "Sinh viên không hoạt động hoặc đề tài đã bị khóa/kết thúc.";

                return RedirectToAction(nameof(Index));
            }

            bool daPhanCong = await _db.PhanCongHuongDans
                .AnyAsync(x =>
                    x.MaDangKy == id &&
                    x.DangHieuLuc);

            if (daPhanCong)
            {
                TempData["Error"] =
                    "Đăng ký này đã có giảng viên hướng dẫn.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.DangKy = dangKy;

            ViewBag.GiangViens = await LayGiangVienPhuHop(
                dangKy.DeTai.MaLinhVuc);

            return View();
        }

        // =====================================================
        // 4. LƯU PHÂN CÔNG
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int id,
            int maGiangVien,
            string? ghiChu)
        {
            ghiChu = ghiChu?.Trim();

            if (ghiChu?.Length > 500)
            {
                TempData["Error"] =
                    "Ghi chú không được quá 500 ký tự.";

                return RedirectToAction(nameof(Create), new { id });
            }

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var dangKy = await _db.DangKyDeTais
                .Include(x => x.DeTai)
                .Include(x => x.SinhVien)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKy == null)
                return NotFound();

            if (dangKy.TrangThai != "Đã duyệt")
            {
                TempData["Error"] =
                    "Đăng ký chưa được duyệt hoặc không còn hợp lệ.";

                return RedirectToAction(nameof(Index));
            }

            if (dangKy.DeTai == null)
                return NotFound();

            if (await _db.PhanCongHuongDans.AnyAsync(p =>
                p.MaDangKy == id &&
                p.TrangThaiThucHien == "Hoàn thành"))
            {
                TempData["Error"] =
                    "Đồ án đã hoàn thành, không thể phân công lại.";

                return RedirectToAction(nameof(Index));
            }

            if (dangKy.SinhVien == null ||
                !dangKy.SinhVien.TrangThai ||
                dangKy.DeTai.TrangThai == "Tạm khóa" ||
                dangKy.DeTai.TrangThai == "Kết thúc")
            {
                TempData["Error"] =
                    "Sinh viên không hoạt động hoặc đề tài bị khóa/kết thúc.";

                return RedirectToAction(nameof(Index));
            }

            bool daPhanCong = await _db.PhanCongHuongDans
                .AnyAsync(x =>
                    x.MaDangKy == id &&
                    x.DangHieuLuc);

            if (daPhanCong)
            {
                TempData["Error"] =
                    "Đăng ký đã có phân công đang hiệu lực.";

                return RedirectToAction(nameof(Index));
            }

            var giangVien = await _db.GiangViens
                .FirstOrDefaultAsync(x =>
                    x.MaGiangVien == maGiangVien);

            if (giangVien == null ||
                giangVien.TrangThai != "Đang công tác" ||
                giangVien.MaLinhVuc != dangKy.DeTai.MaLinhVuc)
            {
                TempData["Error"] =
                    "Giảng viên không phù hợp lĩnh vực hoặc không hoạt động.";

                return RedirectToAction(nameof(Create), new { id });
            }

            if (giangVien.SoSinhVienHuongDanToiDa <= 0)
            {
                TempData["Error"] =
                    "Giảng viên chưa được thiết lập giới hạn hướng dẫn.";

                return RedirectToAction(nameof(Create), new { id });
            }

            int soDangHuongDan = await _db.PhanCongHuongDans
                .CountAsync(x =>
                    x.MaGiangVien == maGiangVien &&
                    x.DangHieuLuc);

            if (soDangHuongDan >= giangVien.SoSinhVienHuongDanToiDa)
            {
                TempData["Error"] =
                    "Giảng viên đã đủ tải hướng dẫn.";

                return RedirectToAction(nameof(Create), new { id });
            }

            var phanCong = new PhanCongHuongDan
            {
                MaDangKy = id,
                MaGiangVien = maGiangVien,
                NgayPhanCong = DateTime.Now,
                GhiChu = ghiChu,
                TrangThaiThucHien = "Chưa bắt đầu",
                DangHieuLuc = true
            };

            _db.PhanCongHuongDans.Add(phanCong);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Phân công giảng viên thành công.";

            return RedirectToAction(
                nameof(Details),
                new { id = phanCong.MaPhanCong });
        }

        // =====================================================
        // 5. CHI TIẾT VÀ LỊCH SỬ PHÂN CÔNG
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var phanCong = await _db.PhanCongHuongDans
                .AsNoTracking()
                .Include(x => x.GiangVien)
                .Include(x => x.DangKyDeTai)
                    .ThenInclude(x => x!.SinhVien)
                .Include(x => x.DangKyDeTai)
                    .ThenInclude(x => x!.DeTai)
                        .ThenInclude(x => x!.LinhVuc)
                .FirstOrDefaultAsync(x =>
                    x.MaPhanCong == id);

            if (phanCong == null)
                return NotFound();

            ViewBag.LichSu = await _db.PhanCongHuongDans
                .AsNoTracking()
                .Include(x => x.GiangVien)
                .Where(x => x.MaDangKy == phanCong.MaDangKy)
                .OrderByDescending(x => x.NgayPhanCong)
                .ThenByDescending(x => x.MaPhanCong)
                .ToListAsync();

            ViewBag.GiangViens =
                phanCong.DangHieuLuc &&
                phanCong.DangKyDeTai?.DeTai != null
                    ? await LayGiangVienPhuHop(
                        phanCong.DangKyDeTai.DeTai.MaLinhVuc)
                    : new List<GiangVien>();

            return View(phanCong);
        }

        // =====================================================
        // 6. THAY ĐỔI GIẢNG VIÊN HƯỚNG DẪN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiGiangVien(
            int id,
            int maGiangVien,
            string? lyDo)
        {
            lyDo = lyDo?.Trim();

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do thay đổi giảng viên.";

                return RedirectToAction(nameof(Details), new { id });
            }

            const string tienTo = "Thay đổi giảng viên: ";

            if (lyDo.Length > 500 - tienTo.Length)
            {
                TempData["Error"] =
                    "Lý do thay đổi quá dài.";

                return RedirectToAction(nameof(Details), new { id });
            }

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var phanCongCu = await _db.PhanCongHuongDans
                .Include(x => x.DangKyDeTai)
                    .ThenInclude(x => x!.DeTai)
                .FirstOrDefaultAsync(x =>
                    x.MaPhanCong == id &&
                    x.DangHieuLuc);

            if (phanCongCu == null)
                return NotFound();

            if (phanCongCu.DangKyDeTai?.TrangThai != "Đã duyệt")
            {
                TempData["Error"] =
                    "Đăng ký không còn hợp lệ.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (phanCongCu.MaGiangVien == maGiangVien)
            {
                TempData["Error"] =
                    "Bạn đang chọn giảng viên hiện tại.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (phanCongCu.TrangThaiThucHien == "Hoàn thành")
            {
                TempData["Error"] =
                    "Không thể đổi giảng viên khi đồ án đã hoàn thành.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var deTai = phanCongCu.DangKyDeTai?.DeTai;

            if (deTai == null)
                return NotFound();

            var giangVienMoi = await _db.GiangViens
                .FirstOrDefaultAsync(x =>
                    x.MaGiangVien == maGiangVien);

            if (giangVienMoi == null ||
                giangVienMoi.TrangThai != "Đang công tác" ||
                giangVienMoi.MaLinhVuc != deTai.MaLinhVuc)
            {
                TempData["Error"] =
                    "Giảng viên mới không phù hợp lĩnh vực.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (giangVienMoi.SoSinhVienHuongDanToiDa <= 0)
            {
                TempData["Error"] =
                    "Giảng viên mới chưa có giới hạn hướng dẫn hợp lệ.";

                return RedirectToAction(nameof(Details), new { id });
            }

            int taiHienTai = await _db.PhanCongHuongDans
                .CountAsync(x =>
                    x.MaGiangVien == maGiangVien &&
                    x.DangHieuLuc);

            if (taiHienTai >= giangVienMoi.SoSinhVienHuongDanToiDa)
            {
                TempData["Error"] =
                    "Giảng viên mới đã đủ tải hướng dẫn.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Kết thúc hiệu lực phân công cũ
            phanCongCu.DangHieuLuc = false;
            phanCongCu.NgayKetThuc = DateTime.Now;

            // Tạo phân công mới để giữ lịch sử
            var phanCongMoi = new PhanCongHuongDan
            {
                MaDangKy = phanCongCu.MaDangKy,
                MaGiangVien = maGiangVien,
                NgayPhanCong = DateTime.Now,
                GhiChu = tienTo + lyDo,
                TrangThaiThucHien =
                    phanCongCu.TrangThaiThucHien,
                DangHieuLuc = true
            };

            _db.PhanCongHuongDans.Add(phanCongMoi);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã thay đổi giảng viên và lưu lịch sử.";

            return RedirectToAction(
                nameof(Details),
                new { id = phanCongMoi.MaPhanCong });
        }

        // =====================================================
        // 7. CẬP NHẬT TIẾN ĐỘ THỰC HIỆN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTienDo(
            int id,
            string trangThai)
        {
            string[] trangThaiHopLe =
            {
                "Chưa bắt đầu",
                "Đang thực hiện",
                "Hoàn thành"
            };

            if (!trangThaiHopLe.Contains(trangThai))
            {
                TempData["Error"] =
                    "Trạng thái tiến độ không hợp lệ.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var phanCong = await _db.PhanCongHuongDans
                .FirstOrDefaultAsync(x =>
                    x.MaPhanCong == id &&
                    x.DangHieuLuc);

            if (phanCong == null)
                return NotFound();

            if (phanCong.TrangThaiThucHien == "Hoàn thành" &&
                trangThai != "Hoàn thành")
            {
                TempData["Error"] =
                    "Đồ án đã hoàn thành, không thể chuyển ngược tiến độ.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (phanCong.TrangThaiThucHien == trangThai)
            {
                TempData["Error"] =
                    "Trạng thái mới giống trạng thái hiện tại.";

                return RedirectToAction(nameof(Details), new { id });
            }

            phanCong.TrangThaiThucHien = trangThai;

            // Hoàn thành thì giải phóng tải giảng viên
            if (trangThai == "Hoàn thành")
            {
                phanCong.DangHieuLuc = false;
                phanCong.NgayKetThuc = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật tiến độ thực hiện thành công.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

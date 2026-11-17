
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using System.Diagnostics;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. DASHBOARD QUẢN TRỊ
        // =====================================================
        [HttpGet]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            // ===============================================
            // 1. TỔNG QUAN HỆ THỐNG
            // ===============================================

            model.TongTaiKhoan =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .CountAsync();

            model.TongLinhVuc =
                await _context.LinhVucs
                    .AsNoTracking()
                    .CountAsync();

            model.TongSinhVien =
                await _context.SinhViens
                    .AsNoTracking()
                    .CountAsync();

            model.TongGiangVien =
                await _context.GiangViens
                    .AsNoTracking()
                    .CountAsync();

            model.TongDeTai =
                await _context.DeTais
                    .AsNoTracking()
                    .CountAsync();

            // ===============================================
            // 2. THỐNG KÊ ĐĂNG KÝ
            // ===============================================

            model.DangKyChoDuyet =
                await _context.DangKyDeTais
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Chờ duyệt");

            model.DangKyDaDuyet =
                await _context.DangKyDeTais
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Đã duyệt");

            // ===============================================
            // 3. THỐNG KÊ SỨC CHỨA ĐỀ TÀI
            // ===============================================

            // Chỉ đăng ký Đã duyệt mới chiếm chỗ.
            // Đăng ký Chờ duyệt, Từ chối, Đã hủy
            // không được tính vào số lượng đã nhận.

            var sucChuaDeTai =
                await _context.DeTais
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.MaDeTai,
                        x.SoSinhVienToiDa,

                        SoDaDuyet =
                            _context.DangKyDeTais.Count(d =>
                                d.MaDeTai == x.MaDeTai &&
                                d.TrangThai == "Đã duyệt")
                    })
                    .ToListAsync();

            model.DeTaiConCho =
                sucChuaDeTai.Count(x =>
                    x.SoSinhVienToiDa > 0 &&
                    x.SoDaDuyet < x.SoSinhVienToiDa);

            model.DeTaiDuSoLuong =
                sucChuaDeTai.Count(x =>
                    x.SoSinhVienToiDa > 0 &&
                    x.SoDaDuyet >= x.SoSinhVienToiDa);

            // ===============================================
            // 4. SINH VIÊN CÓ GIẢNG VIÊN HƯỚNG DẪN
            // ===============================================

            // Chỉ đếm sinh viên có phân công
            // đang hiệu lực và chưa hoàn thành.
            // Distinct tránh đếm trùng sinh viên.

            model.SinhVienCoGVHD =
                await _context.PhanCongHuongDans
                    .AsNoTracking()
                    .Where(x =>
                        x.DangHieuLuc &&
                        x.TrangThaiThucHien != "Hoàn thành" &&
                        x.DangKyDeTai != null &&
                        x.DangKyDeTai.TrangThai == "Đã duyệt")
                    .Select(x =>
                        x.DangKyDeTai!.MaSinhVien)
                    .Distinct()
                    .CountAsync();

            // ===============================================
            // 5. GIẢNG VIÊN ĐẠT GIỚI HẠN HƯỚNG DẪN
            // ===============================================

            // Tải hướng dẫn được tính trên toàn hệ thống.
            // Phân công đã hoàn thành hoặc hết hiệu lực
            // không được chiếm tải.

            var taiGiangVien =
                await _context.GiangViens
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.MaGiangVien,
                        x.SoSinhVienHuongDanToiDa,

                        DangHuongDan =
                            _context.PhanCongHuongDans.Count(p =>
                                p.MaGiangVien == x.MaGiangVien &&
                                p.DangHieuLuc &&
                                p.TrangThaiThucHien != "Hoàn thành")
                    })
                    .ToListAsync();

            model.GiangVienDatGioiHan =
                taiGiangVien.Count(x =>
                    x.SoSinhVienHuongDanToiDa > 0 &&
                    x.DangHuongDan >=
                        x.SoSinhVienHuongDanToiDa);

            // ===============================================
            // 6. BIỂU ĐỒ ĐỀ TÀI THEO LĨNH VỰC
            // ===============================================

            model.DeTaiTheoLinhVuc =
                await _context.DeTais
                    .AsNoTracking()
                    .GroupBy(x => new
                    {
                        x.MaLinhVuc,
                        TenLinhVuc =
                            x.LinhVuc != null
                                ? x.LinhVuc.TenLinhVuc
                                : "Chưa xác định"
                    })
                    .Select(g => new ThongKeLinhVuc
                    {
                        TenLinhVuc = g.Key.TenLinhVuc,
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ThenBy(x => x.TenLinhVuc)
                    .ToListAsync();

            // ===============================================
            // 7. BIỂU ĐỒ ĐĂNG KÝ THEO NĂM HỌC
            // ===============================================

            model.DangKyTheoNamHoc =
                await _context.DangKyDeTais
                    .AsNoTracking()
                    .GroupBy(x => x.DeTai!.NamHoc)
                    .Select(g => new ThongKeNamHoc
                    {
                        NamHoc = g.Key,
                        SoLuong = g.Count()
                    })
                    .OrderBy(x => x.NamHoc)
                    .ToListAsync();

            // ===============================================
            // 8. DANH SÁCH 5 ĐỀ TÀI MỚI NHẤT
            // ===============================================

            model.DeTaiMoiNhat =
                await _context.DeTais
                    .AsNoTracking()
                    .OrderByDescending(x => x.MaDeTai)
                    .Take(5)
                    .Select(x => new DeTaiMoiNhat
                    {
                        MaDeTai = x.MaDeTai,

                        TenDeTai = x.TenDeTai,

                        TenLinhVuc =
                            x.LinhVuc != null
                                ? x.LinhVuc.TenLinhVuc
                                : "Chưa xác định",

                        NamHoc = x.NamHoc,

                        HocKy = x.HocKy,

                        TrangThai = x.TrangThai
                    })
                    .ToListAsync();

            return View(model);
        }

        // =====================================================
        // 2. TRANG PRIVACY
        // =====================================================
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // =====================================================
        // 3. TRANG KHÔNG CÓ QUYỀN TRUY CẬP
        // =====================================================
        [HttpGet]
        public IActionResult KhongCoQuyen()
        {
            int? maTaiKhoan =
                HttpContext.Session.GetInt32("MaTaiKhoan");

            if (!maTaiKhoan.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "DangNhap");
            }

            return View();
        }

        // =====================================================
        // 4. TRANG BÁO LỖI
        // =====================================================
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}

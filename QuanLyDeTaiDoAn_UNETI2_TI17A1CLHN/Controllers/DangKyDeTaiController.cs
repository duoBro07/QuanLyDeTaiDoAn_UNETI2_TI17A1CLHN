// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Sinh viên đăng ký đề tài, xem/hủy đăng ký, xem GVHD hiện tại và lịch sử phân công của chính mình
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Services;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers;

[YeuCauVaiTro(VaiTroHeThong.SinhVien)]
public class DangKyDeTaiController : Module3BaseController
{
    public DangKyDeTaiController(AppDbContext db) : base(db) { }

    // Màn 11: danh sách đăng ký của tôi (chỉ dữ liệu của sinh viên đang đăng nhập)
    public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, int trang = 1)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var q = _db.DangKyDeTais.Where(d => d.MaSinhVien == sv.MaSinhVien);
        if (!string.IsNullOrWhiteSpace(tuKhoa)) { tuKhoa = tuKhoa.Trim(); q = q.Where(d => d.DeTai!.TenDeTai.Contains(tuKhoa)); }
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(d => d.TrangThai == trangThai);

        var pt = PhanTrang(await q.CountAsync(), trang);
        var ds = await q.Include(d => d.DeTai)
                        .Include(d => d.PhanCongs).ThenInclude(p => p.GiangVien)
                        .OrderByDescending(d => d.NgayDangKy)
                        .Skip((pt.Trang - 1) * KichThuocTrang).Take(KichThuocTrang)
                        .ToListAsync();
        pt.SoHienThi = ds.Count;

        ViewBag.PhanTrang = pt; ViewBag.TuKhoa = tuKhoa; ViewBag.TrangThai = trangThai;
        return View(ds);
    }

    // Màn 9: form đăng ký (id = MaDeTai)
    public async Task<IActionResult> Create(int id)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();
        var dt = await _db.DeTais.Include(d => d.LinhVuc).FirstOrDefaultAsync(d => d.MaDeTai == id);
        if (dt == null) return NotFound();

        return View(new DangKyFormVM
        {
            MaDeTai = dt.MaDeTai,
            TenDeTai = dt.TenDeTai,
            TenLinhVuc = dt.LinhVuc?.TenLinhVuc,
            ThongBaoKhongDangKy = await DangKyRules.KiemTra(_db, sv, dt)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DangKyFormVM vm)
    {
        var sv = await LaySinhVienHienTai();               // lấy từ Session, không từ form
        if (sv == null) return NotFound();
        var dt = await _db.DeTais.Include(d => d.LinhVuc).FirstOrDefaultAsync(d => d.MaDeTai == vm.MaDeTai);
        if (dt == null) return NotFound();

        vm.TenDeTai = dt.TenDeTai;
        vm.TenLinhVuc = dt.LinhVuc?.TenLinhVuc;

        // Kiểm tra lại toàn bộ điều kiện ở server (không tin giao diện)
        vm.ThongBaoKhongDangKy = await DangKyRules.KiemTra(_db, sv, dt);
        if (vm.ThongBaoKhongDangKy != null || !ModelState.IsValid) return View(vm);   // không lưu gì

        var dk = new DangKyDeTai
        {
            MaSinhVien = sv.MaSinhVien,
            MaDeTai = dt.MaDeTai,
            LyDoDangKy = vm.LyDoDangKy!.Trim(),
            NgayDangKy = DateTime.Now,                      // hệ thống tự gán
            TrangThai = TrangThaiDangKy.ChoDuyet
        };
        _db.DangKyDeTais.Add(dk);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(XacNhan), new { id = dk.MaDangKy });
    }

    // Màn 10: xác nhận đăng ký thành công (id = MaDangKy)
    public async Task<IActionResult> XacNhan(int id)
    {
        var dk = await LayDangKyCuaToi(id);
        return dk == null ? NotFound() : View(dk);
    }

    // Màn 12: chi tiết kết quả đăng ký
    public async Task<IActionResult> Details(int id)
    {
        var dk = await LayDangKyCuaToi(id);
        return dk == null ? NotFound() : View(dk);
    }

    // Màn 13: hủy đăng ký khi còn chờ duyệt
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var dk = await _db.DangKyDeTais.FirstOrDefaultAsync(d => d.MaDangKy == id && d.MaSinhVien == sv.MaSinhVien);
        if (dk == null) return NotFound();

        if (dk.TrangThai != TrangThaiDangKy.ChoDuyet)
        {
            TempData["Loi"] = "Chỉ được hủy đăng ký khi đang ở trạng thái Chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        dk.TrangThai = TrangThaiDangKy.DaHuy;               // không xóa vật lý, giữ lịch sử
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã hủy đăng ký.";
        return RedirectToAction(nameof(Index));
    }

    // Màn 15: giảng viên hướng dẫn hiện tại
    public async Task<IActionResult> GiangVienHienTai()
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var pc = await _db.PhanCongHuongDans
            .Include(p => p.GiangVien).ThenInclude(g => g!.LinhVuc)
            .Include(p => p.DangKyDeTai).ThenInclude(d => d!.DeTai)
            .Where(p => p.DangKyDeTai!.MaSinhVien == sv.MaSinhVien && p.TrangThai == TrangThaiPhanCong.DangHuongDan)
            .OrderByDescending(p => p.NgayBatDau)
            .FirstOrDefaultAsync();
        return View(pc);   // null => View hiển thị "chưa được phân công"
    }

    // Màn 16: lịch sử phân công liên quan đến bản thân
    public async Task<IActionResult> LichSuPhanCong()
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var ds = await _db.PhanCongHuongDans
            .Include(p => p.GiangVien)
            .Include(p => p.DangKyDeTai).ThenInclude(d => d!.DeTai)
            .Where(p => p.DangKyDeTai!.MaSinhVien == sv.MaSinhVien)
            .OrderByDescending(p => p.NgayBatDau)
            .ToListAsync();
        return View(ds);
    }

    // Chỉ lấy đăng ký thuộc về sinh viên đang đăng nhập; đổi id trên URL sang người khác thì ra null (NotFound)
    private async Task<DangKyDeTai?> LayDangKyCuaToi(int maDangKy)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return null;
        return await _db.DangKyDeTais
            .Include(d => d.DeTai).ThenInclude(t => t!.LinhVuc)
            .Include(d => d.PhanCongs).ThenInclude(p => p.GiangVien)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy && d.MaSinhVien == sv.MaSinhVien);
    }
}

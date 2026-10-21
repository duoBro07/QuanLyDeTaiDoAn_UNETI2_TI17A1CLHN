// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Admin quản lý sinh viên (danh sách, thêm, sửa, chi tiết, khóa/mở khóa) + trang chủ và thông tin cá nhân của sinh viên
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Services;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers;

public class SinhVienController : Module3BaseController
{
    private const string MatKhauMacDinh = "123456";   // nếu Module 1 mã hóa mật khẩu thì dùng hàm mã hóa của Module 1

    public SinhVienController(AppDbContext db) : base(db) { }

    // =====================  PHẦN ADMIN  =====================

    // Màn 1: danh sách + tìm kiếm + phân trang
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> Index(string? tuKhoa, int trang = 1)
    {
        var q = _db.SinhViens.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(s => s.MaSinhVien.Contains(tuKhoa) || s.HoTen.Contains(tuKhoa) || s.Email.Contains(tuKhoa));
        }

        var pt = PhanTrang(await q.CountAsync(), trang);
        var ds = await q.OrderBy(s => s.MaSinhVien)
                        .Skip((pt.Trang - 1) * KichThuocTrang).Take(KichThuocTrang)
                        .ToListAsync();
        pt.SoHienThi = ds.Count;

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.PhanTrang = pt;
        return View(ds);
    }

    // Màn 2: thêm sinh viên
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public IActionResult Create() => View(new SinhVien());

    [HttpPost, ValidateAntiForgeryToken]
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> Create(SinhVien sv)
    {
        sv.MaSinhVien = (sv.MaSinhVien ?? "").Trim();

        if (sv.MaSinhVien != "")
        {
            if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == sv.MaSinhVien))
                ModelState.AddModelError(nameof(sv.MaSinhVien), "Mã sinh viên đã tồn tại");
            else if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == sv.MaSinhVien))
                ModelState.AddModelError(nameof(sv.MaSinhVien), "Tên đăng nhập trùng với mã sinh viên này đã tồn tại");
        }
        if (!string.IsNullOrWhiteSpace(sv.Email) && await _db.SinhViens.AnyAsync(s => s.Email == sv.Email))
            ModelState.AddModelError(nameof(sv.Email), "Email đã được sử dụng");
        if (sv.NgaySinh.HasValue && sv.NgaySinh.Value.Date >= DateTime.Today)
            ModelState.AddModelError(nameof(sv.NgaySinh), "Ngày sinh phải nhỏ hơn ngày hiện tại");

        if (!ModelState.IsValid) return View(sv);

        // Tạo tài khoản đăng nhập (1-1) và sinh viên trong cùng một giao dịch
        using var tx = await _db.Database.BeginTransactionAsync();
        var tk = new TaiKhoan
        {
            TenDangNhap = sv.MaSinhVien,
            MatKhau = MatKhauMacDinh,
            HoTen = sv.HoTen,
            Email = sv.Email,
            VaiTro = VaiTroHeThong.SinhVien,
            TrangThai = sv.TrangThai == TrangThaiSinhVien.HoatDong
        };
        _db.TaiKhoans.Add(tk);
        await _db.SaveChangesAsync();

        sv.MaTaiKhoan = tk.MaTaiKhoan;
        _db.SinhViens.Add(sv);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["ThanhCong"] = $"Đã thêm sinh viên {sv.HoTen} ({sv.MaSinhVien}). Mật khẩu mặc định: {MatKhauMacDinh}";
        return RedirectToAction(nameof(Index));
    }

    // Màn 3: sửa sinh viên (không cho sửa mã sinh viên)
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSinhVien == id);
        return sv == null ? NotFound() : View(sv);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> Edit(string id, SinhVien form)
    {
        var sv = await _db.SinhViens.FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        form.MaSinhVien = id;   // luôn dùng mã từ URL, bỏ qua giá trị gửi lên
        ModelState.Remove(nameof(form.MaSinhVien));

        if (!string.IsNullOrWhiteSpace(form.Email) &&
            await _db.SinhViens.AnyAsync(s => s.Email == form.Email && s.MaSinhVien != id))
            ModelState.AddModelError(nameof(form.Email), "Email đã được sử dụng");
        if (form.NgaySinh.HasValue && form.NgaySinh.Value.Date >= DateTime.Today)
            ModelState.AddModelError(nameof(form.NgaySinh), "Ngày sinh phải nhỏ hơn ngày hiện tại");

        if (!ModelState.IsValid) return View(form);

        // Chỉ cập nhật các trường được phép (tránh overposting)
        sv.HoTen = form.HoTen;
        sv.NgaySinh = form.NgaySinh;
        sv.GioiTinh = form.GioiTinh;
        sv.Lop = form.Lop;
        sv.Khoa = form.Khoa;
        sv.ChuyenNganh = form.ChuyenNganh;
        sv.Email = form.Email;
        sv.SoDienThoai = form.SoDienThoai;
        sv.TrangThai = form.TrangThai;
        await DongBoTaiKhoan(sv);
        await _db.SaveChangesAsync();

        TempData["ThanhCong"] = "Cập nhật sinh viên thành công";
        return RedirectToAction(nameof(Index));
    }

    // Màn 4: chi tiết
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSinhVien == id);
        return sv == null ? NotFound() : View(sv);
    }

    // Màn 5: khóa / mở khóa (POST, có hộp thoại xác nhận ở View)
    [HttpPost, ValidateAntiForgeryToken]
    [YeuCauVaiTro(VaiTroHeThong.Admin)]
    public async Task<IActionResult> DoiTrangThai(string id)
    {
        var sv = await _db.SinhViens.FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        bool dangKhoa = sv.TrangThai == TrangThaiSinhVien.Khoa;
        sv.TrangThai = dangKhoa ? TrangThaiSinhVien.HoatDong : TrangThaiSinhVien.Khoa;
        await DongBoTaiKhoan(sv);
        await _db.SaveChangesAsync();

        TempData["ThanhCong"] = dangKhoa ? $"Đã mở khóa sinh viên {sv.HoTen}" : $"Đã khóa sinh viên {sv.HoTen}";
        return RedirectToAction(nameof(Index));
    }

    // Đồng bộ trạng thái sang TaiKhoan để sinh viên bị khóa không đăng nhập được.
    // [KHỚP VỚI MODULE 1] Nếu TaiKhoan.TrangThai của Module 1 không phải bool thì chỉ sửa dòng gán ở đây.
    private async Task DongBoTaiKhoan(SinhVien sv)
    {
        var tk = await _db.TaiKhoans.FindAsync(sv.MaTaiKhoan);
        if (tk != null) tk.TrangThai = sv.TrangThai == TrangThaiSinhVien.HoatDong;
    }

    // =====================  PHẦN SINH VIÊN  =====================

    // Màn 6: trang chủ sinh viên
    [YeuCauVaiTro(VaiTroHeThong.SinhVien)]
    public async Task<IActionResult> TrangChu()
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var dangKy = _db.DangKyDeTais.Where(d => d.MaSinhVien == sv.MaSinhVien);
        var deTaiMo = _db.DeTais.Where(d => d.TrangThai == TrangThaiDeTai.MoDangKy
                                         && d.NamHoc == DangKyRules.NamHocHienTai
                                         && d.HocKy == DangKyRules.HocKyHienTai);
        var vm = new TrangChuSinhVienVM
        {
            SinhVien = sv,
            TongDeTaiCoTheDangKy = await deTaiMo.CountAsync(),
            DaDangKy = await dangKy.CountAsync(),
            ChoDuyet = await dangKy.CountAsync(d => d.TrangThai == TrangThaiDangKy.ChoDuyet),
            DaDuyet = await dangKy.CountAsync(d => d.TrangThai == TrangThaiDangKy.DaDuyet),
            DeTaiNoiBat = await deTaiMo.OrderByDescending(d => d.MaDeTai).Take(3).ToTraCuuVM().ToListAsync()
        };
        return View(vm);
    }

    // Màn 14: thông tin cá nhân
    [YeuCauVaiTro(VaiTroHeThong.SinhVien)]
    public async Task<IActionResult> CaNhan()
    {
        var sv = await LaySinhVienHienTai();
        return sv == null ? NotFound() : View(sv);
    }

    // Sinh viên chỉ được sửa Email và Số điện thoại
    [YeuCauVaiTro(VaiTroHeThong.SinhVien)]
    public async Task<IActionResult> CapNhatCaNhan()
    {
        var sv = await LaySinhVienHienTai();
        return sv == null ? NotFound() : View(sv);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [YeuCauVaiTro(VaiTroHeThong.SinhVien)]
    public async Task<IActionResult> CapNhatCaNhan(string? email, string? soDienThoai)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        email = email?.Trim();
        soDienThoai = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai.Trim();

        if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
            ModelState.AddModelError("email", "Email không đúng định dạng");
        else if (await _db.SinhViens.AnyAsync(s => s.Email == email && s.MaSinhVien != sv.MaSinhVien))
            ModelState.AddModelError("email", "Email đã được sử dụng");

        if (soDienThoai != null && !Regex.IsMatch(soDienThoai, @"^(0|\+84)\d{9}$"))
            ModelState.AddModelError("soDienThoai", "Số điện thoại không hợp lệ (ví dụ 0987654321)");

        if (!ModelState.IsValid)
        {
            sv.Email = email ?? ""; sv.SoDienThoai = soDienThoai;   // chỉ để hiển thị lại, không lưu
            return View(sv);
        }

        sv.Email = email!;
        sv.SoDienThoai = soDienThoai;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Cập nhật thông tin cá nhân thành công";
        return RedirectToAction(nameof(CaNhan));
    }
}

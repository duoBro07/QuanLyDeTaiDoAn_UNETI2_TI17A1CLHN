// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Sinh viên tra cứu danh sách đề tài (tìm kiếm, lọc, phân trang) và xem chi tiết đề tài
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Services;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers;

[YeuCauVaiTro(VaiTroHeThong.SinhVien)]
public class TraCuuDeTaiController : Module3BaseController
{
    public TraCuuDeTaiController(AppDbContext db) : base(db) { }

    // Màn 7: danh sách đề tài
    public async Task<IActionResult> Index(string? tuKhoa, int? linhVuc, string? namHoc, int? hocKy, string? trangThai, int trang = 1)
    {
        var q = _db.DeTais.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa)) { tuKhoa = tuKhoa.Trim(); q = q.Where(d => d.TenDeTai.Contains(tuKhoa)); }
        if (linhVuc.HasValue) q = q.Where(d => d.MaLinhVuc == linhVuc.Value);
        if (!string.IsNullOrEmpty(namHoc)) q = q.Where(d => d.NamHoc == namHoc);
        if (hocKy.HasValue) q = q.Where(d => d.HocKy == hocKy.Value);
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(d => d.TrangThai == trangThai);

        var pt = PhanTrang(await q.CountAsync(), trang);
        var ds = await q.OrderBy(d => d.TenDeTai)
                        .Skip((pt.Trang - 1) * KichThuocTrang).Take(KichThuocTrang)
                        .ToTraCuuVM().ToListAsync();
        pt.SoHienThi = ds.Count;

        ViewBag.PhanTrang = pt;
        ViewBag.TuKhoa = tuKhoa; ViewBag.LinhVuc = linhVuc; ViewBag.NamHoc = namHoc;
        ViewBag.HocKy = hocKy; ViewBag.TrangThai = trangThai;
        ViewBag.LinhVucs = await _db.LinhVucs.OrderBy(l => l.TenLinhVuc).ToListAsync();
        ViewBag.NamHocs = await _db.DeTais.Select(d => d.NamHoc).Distinct().OrderBy(x => x).ToListAsync();
        return View(ds);
    }

    // Màn 8: chi tiết đề tài
    public async Task<IActionResult> Details(int id)
    {
        var sv = await LaySinhVienHienTai();
        if (sv == null) return NotFound();

        var dt = await _db.DeTais.Include(d => d.LinhVuc).FirstOrDefaultAsync(d => d.MaDeTai == id);
        if (dt == null) return NotFound();

        var vm = new DeTaiChiTietVM
        {
            DeTai = dt,
            TenLinhVuc = dt.LinhVuc?.TenLinhVuc ?? "",
            SoDaDangKy = await DangKyRules.DemDaDuyet(_db, id),
            GiangVienCungLinhVuc = await _db.GiangViens
                .Where(g => g.MaLinhVuc == dt.MaLinhVuc && g.TrangThai == TrangThaiGiangVien.DangCongTac)
                .OrderBy(g => g.HoTen).Select(g => g.HoTen).ToListAsync(),
            LyDoKhongDangKy = await DangKyRules.KiemTra(_db, sv, dt)
        };
        return View(vm);
    }
}

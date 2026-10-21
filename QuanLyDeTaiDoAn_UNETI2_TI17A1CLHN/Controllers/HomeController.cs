using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.ViewModels;
using System.Diagnostics;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardViewModel();

            if (HttpContext.Session.GetString("VaiTro") == "Admin")
            {
                model.TongTaiKhoan = await _context.TaiKhoans.CountAsync();
                model.TaiKhoanHoatDong = await _context.TaiKhoans.CountAsync(x => x.TrangThai);
                model.TaiKhoanBiKhoa = model.TongTaiKhoan - model.TaiKhoanHoatDong;
                model.TongLinhVuc = await _context.LinhVucs.CountAsync();
                model.LinhVucHoatDong = await _context.LinhVucs.CountAsync(x => x.TrangThai);
            }

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

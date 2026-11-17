
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false)]
    public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;
            int? maTaiKhoan = session.GetInt32("MaTaiKhoan");

            if (!maTaiKhoan.HasValue)
            {
                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
                return;
            }

            var db = context.HttpContext.RequestServices
                .GetRequiredService<ApplicationDbContext>();

            var taiKhoan = await db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == maTaiKhoan.Value);

            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                session.Clear();

                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
                return;
            }

            session.SetString("VaiTro", taiKhoan.VaiTro);
            session.SetString("HoTen", taiKhoan.HoTen);

            if (taiKhoan.VaiTro != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "KhongCoQuyen", "Home", null);
                return;
            }

            await next();
        }
    }
}

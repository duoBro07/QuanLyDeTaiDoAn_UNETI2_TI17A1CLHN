
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false)]
    public class PhanQuyenAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _vaiTroChoPhep;

        public PhanQuyenAttribute(params string[] vaiTro)
        {
            _vaiTroChoPhep = vaiTro ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            // 1. Kiểm tra đăng nhập
            int? maTaiKhoan = session.GetInt32("MaTaiKhoan");

            if (!maTaiKhoan.HasValue)
            {
                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
                return;
            }

            // 2. Lấy tài khoản từ Database
            var db = httpContext.RequestServices
                .GetRequiredService<ApplicationDbContext>();

            var taiKhoan = await db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == maTaiKhoan.Value);

            // 3. Tài khoản không tồn tại hoặc bị khóa
            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                session.Clear();

                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
                return;
            }

            // 4. Chuẩn hóa vai trò
            string vaiTroGoc = (taiKhoan.VaiTro ?? "").Trim();

            string vaiTro;

            if (vaiTroGoc.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                vaiTro = "Admin";
            }
            else if (vaiTroGoc.Equals(
                "SinhVien",
                StringComparison.OrdinalIgnoreCase))
            {
                vaiTro = "SinhVien";
            }
            else
            {
                // Không tự cấp quyền cho vai trò không hợp lệ
                session.Clear();

                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
                return;
            }

            // 5. Đồng bộ Session
            session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            session.SetString("VaiTro", vaiTro);
            session.SetString("HoTen", taiKhoan.HoTen ?? "");

            // 6. Kiểm tra quyền
            bool duocPhep =
                _vaiTroChoPhep.Length == 0 ||
                _vaiTroChoPhep.Any(x =>
                    string.Equals(
                        x?.Trim(),
                        vaiTro,
                        StringComparison.OrdinalIgnoreCase));

            if (!duocPhep)
            {
                context.Result = new RedirectToActionResult(
                    "KhongCoQuyen", "Home", null);
                return;
            }

            // 7. Cho phép thực hiện Action
            await next();
        }
    }
}

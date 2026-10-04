using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string? _vaiTro;

        public SessionAuthorizeAttribute(string? vaiTro = null)
        {
            _vaiTro = vaiTro;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var maTaiKhoan =
                context.HttpContext.Session.GetInt32("MaTaiKhoan");

            var vaiTro =
                context.HttpContext.Session.GetString("VaiTro");

            // Chưa đăng nhập
            if (maTaiKhoan == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "TaiKhoan",
                    null
                );

                return;
            }

            // Đã đăng nhập nhưng không đúng quyền
            if (_vaiTro != null && vaiTro != _vaiTro)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
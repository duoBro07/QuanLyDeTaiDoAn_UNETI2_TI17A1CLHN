
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var role = context.HttpContext.Session
                .GetString("VaiTro");

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult(
                    "Index", "DangNhap", null);
            }
            else if (role != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "KhongCoQuyen", "Home", null);
            }
        }
    }
}

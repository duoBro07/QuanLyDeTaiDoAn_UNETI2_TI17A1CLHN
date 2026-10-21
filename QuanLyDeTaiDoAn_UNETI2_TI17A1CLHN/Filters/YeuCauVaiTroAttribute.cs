// Họ và tên: Nguyen Dinh Nghiep
// Mã sinh viên: 23103100304
// Nội dung thực hiện: Kiểm tra quyền tại Controller (chạy trước Action). Khi ghép nhóm có thể thay bằng Filter của Module 1
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;

public class YeuCauVaiTroAttribute : ActionFilterAttribute
{
    private readonly string _vaiTro;
    public YeuCauVaiTroAttribute(string vaiTro) => _vaiTro = vaiTro;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var vaiTro = context.HttpContext.Session.GetString("VaiTro");   // do Module 1 lưu khi đăng nhập

        if (string.IsNullOrEmpty(vaiTro))
            context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", null);
        else if (vaiTro != _vaiTro)
            context.Result = new ContentResult
            {
                StatusCode = 403,
                ContentType = "text/plain; charset=utf-8",
                Content = "Bạn không có quyền truy cập chức năng này."
            };
    }
}

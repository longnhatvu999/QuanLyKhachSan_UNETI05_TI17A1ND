// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: BaseController - Kiểm tra quyền Admin bằng Session
// Module: 1 (áp dụng cho toàn hệ thống)
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    /// <summary>
    /// BaseController dùng để kiểm tra quyền Admin.
    /// Mọi Controller Admin (LoaiPhong, Phong, KhachHang, ThongKe...) đều kế thừa class này.
    /// </summary>
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Lấy thông tin từ Session
            var maTaiKhoan = context.HttpContext.Session.GetInt32("MaTaiKhoan");
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");

            // Nếu chưa đăng nhập → chuyển về trang Đăng nhập
            if (maTaiKhoan == null)
            {
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan",
                    new { returnUrl = context.HttpContext.Request.Path.ToString() });
                return;
            }

            // Nếu đã đăng nhập nhưng không phải Admin → chặn
            if (vaiTro != "Admin")
            {
                context.Result = new RedirectToActionResult("TuChoi", "TaiKhoan", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
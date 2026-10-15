using Microsoft.AspNetCore.Mvc;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

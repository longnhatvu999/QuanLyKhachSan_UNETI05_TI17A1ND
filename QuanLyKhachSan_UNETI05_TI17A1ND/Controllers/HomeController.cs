using Microsoft.AspNetCore.Mvc;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;
using System.Diagnostics;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

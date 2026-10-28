using Microsoft.AspNetCore.Mvc;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using System.Diagnostics;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
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

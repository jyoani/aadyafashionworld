using Microsoft.AspNetCore.Mvc;

namespace BulkyProject.Areas.Admin.Controllers
{
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace BitPantry.Tabs.Web.Controllers
{
    public class Settings : Controller
    {
        public IActionResult Index()
        {
            return View(nameof(Index));
        }
    }
}

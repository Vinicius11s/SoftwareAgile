using Microsoft.AspNetCore.Mvc;

namespace Agile.Controllers
{
    public class AgileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

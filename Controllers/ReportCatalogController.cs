using Microsoft.AspNetCore.Mvc;

namespace ReportSystem.Controllers
{
    
    public class ReportCatalogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

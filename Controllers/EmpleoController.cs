using Microsoft.AspNetCore.Mvc;

namespace SpaWebApp.Controllers
{
    public class EmpleoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Aplicar()
        {
            TempData["SuccessMessage"] = "Aplicación enviada con éxito.";
            return RedirectToAction("Index");
        }
    }
}

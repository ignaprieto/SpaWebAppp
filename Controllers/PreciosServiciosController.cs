using Microsoft.AspNetCore.Mvc;
using SpaWebApp.Data;
using SpaWebApp.Models;
using System.Linq;

namespace SpaWebApp.Controllers
{
    public class PrecioServiciosController : Controller
    {
        private readonly SpaContext _context;

        public PrecioServiciosController(SpaContext context)
        {
            _context = context;
        }

        // GET: PrecioServicios/Index
        public IActionResult Index()
        {
            ViewBag.MensajeExito = TempData["MensajeExito"];
            var preciosServicios = _context.PreciosServicios.ToList();
            return View(preciosServicios);
        }

        // POST: PrecioServicios/EditarPrecio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPrecio(string id, decimal? precio)
        {
            if (precio == null)
            {
                ModelState.AddModelError("", "El precio no puede estar vacío.");
                return RedirectToAction("Index");
            }

            var servicio = _context.PreciosServicios.SingleOrDefault(p => p.Servicio == id);
            if (servicio != null)
            {
                servicio.Precio = precio;
                _context.SaveChanges();
                TempData["MensajeExito"] = "Precio actualizado exitosamente.";
            }

            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SpaWebApp.Data;
using SpaWebApp.Models;
using System.Linq;

namespace SpaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly SpaContext _context;

        public HomeController(SpaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(Consulta consulta)
        {
            if (ModelState.IsValid)
            {
                //_context.Consultas.Add(consulta);
                //_context.SaveChanges();
                ViewBag.Message = "Consulta enviada correctamente.";
            }

            return View("Index");
        }

        // GET: Usuarios/Clientes
        public IActionResult Clientes()
        {
            // Obtenemos la lista de usuarios que tienen el rol "Cliente"
            var clientes = _context.Usuarios
                .Where(u => u.Rol == "Cliente")
                .ToList();

            return View(clientes);
        }
    }
}

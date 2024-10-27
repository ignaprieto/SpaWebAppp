using Microsoft.AspNetCore.Mvc;
using SpaWebApp.Models;
using SpaWebApp.Data;
using System.Security.Claims;

namespace SpaWebApp.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly SpaContext _context;

        public ComentariosController(SpaContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Comentarios comentarioModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                    if (!string.IsNullOrEmpty(userName))
                    {
                        comentarioModel.Nombre = userName;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo obtener el nombre del usuario.");
                        return RedirectToAction("Index", "Servicios");
                    }
                }

                if (string.IsNullOrEmpty(comentarioModel.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                    return RedirectToAction("Index", "Servicios");
                }

                _context.Comentarios.Add(comentarioModel);
                _context.SaveChanges();

                return RedirectToAction("Index", "Servicios");
            }

            return RedirectToAction("Index", "Servicios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int comentarioId)
        {
            var comentario = _context.Comentarios.Find(comentarioId);
            if (comentario != null)
            {
                _context.Comentarios.Remove(comentario);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Servicios");
        }
    }
}

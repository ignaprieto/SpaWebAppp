using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SpaWebApp.Models;
using SpaWebApp.Data;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SpaWebApp.Controllers
{
    [Authorize]
    public class TurnosController : Controller
    {
        private readonly SpaContext _context;

        public TurnosController(SpaContext context)
        {
            _context = context;
        }

        private List<SelectListItem> GetServicioList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Masajes_AntiStress", Text = "Masajes AntiStress" },
                new SelectListItem { Value = "Masajes_Descontracturantes", Text = "Masajes Descontracturantes" },
                new SelectListItem { Value = "Masajes_PiedrasCalientes", Text = "Masajes con Piedras Calientes" },
                new SelectListItem { Value = "Masajes_Circulatorios", Text = "Masajes Circulatorios" },
                new SelectListItem { Value = "Belleza_LiftingPestañas", Text = "Belleza: Lifting de Pestañas" },
                new SelectListItem { Value = "Belleza_DepilacionFacial", Text = "Belleza: Depilación Facial" },
                new SelectListItem { Value = "BellezaManosPies", Text = "Belleza de Manos y Pies" },
                new SelectListItem { Value = "Tratamientos_Faciales_PuntaDiamante", Text = "Tratamientos Faciales: Punta de Diamante" },
                new SelectListItem { Value = "Tratamientos_Faciales_LimpiezaProfunda", Text = "Tratamientos Faciales: Limpieza Profunda" },
                new SelectListItem { Value = "Tratamientos_Faciales_CrioFrecuenciaFacial", Text = "Tratamientos Faciales: Crio Frecuencia Facial" },
                new SelectListItem { Value = "Tratamientos_Corporales_VelaSlim", Text = "Tratamientos Corporales: VelaSlim" },
                new SelectListItem { Value = "Tratamientos_Corporales_DermoHealth", Text = "Tratamientos Corporales: DermoHealth" },
                new SelectListItem { Value = "Tratamientos_Corporales_CrioFrecuenciaCorporal", Text = "Tratamientos Corporales: Crio Frecuencia Corporal" },
                new SelectListItem { Value = "Tratamientos_Corporales_Ultracavitacion", Text = "Tratamientos Corporales: Ultracavitación" }
            };
        }

        // GET: Turnos/Index
        public IActionResult Index()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var usuario = _context.Usuarios.SingleOrDefault(u => u.Email == userEmail);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            switch (usuario.Rol)
            {
                case "Cliente":
                    return RedirectToAction("Reservar");
                case "Profesional":
                    var turnosProfesional = _context.Turnos
                        .Include(t => t.Usuario)
                        .Select(t => new Turno
                        {
                            TurnoID = t.TurnoID,
                            UsuarioID = t.UsuarioID,
                            Servicio = t.Servicio,
                            FechaTurno = t.FechaTurno,
                            Estado = t.Estado,
                            Comentarios = t.Comentarios,
                            Usuario = t.Usuario // Incluye el usuario para tener acceso al nombre completo
                        })
                        .ToList();
                    return View("Index", turnosProfesional);

                case "Administrador":
                    var turnosAdmin = _context.Turnos
                        .Include(t => t.Usuario)
                        .Select(t => new Turno
                        {
                            TurnoID = t.TurnoID,
                            UsuarioID = t.UsuarioID,
                            Servicio = t.Servicio,
                            FechaTurno = t.FechaTurno,
                            Estado = t.Estado,
                            Comentarios = t.Comentarios,
                            Usuario = t.Usuario // Incluye el usuario para tener acceso al nombre completo
                        })
                        .ToList();
                    return View("IndexPersonal", turnosAdmin);
            }

            return RedirectToAction("Login", "Auth");
        }

        // GET: Turnos/Reservar
        public IActionResult Reservar()
        {
            ViewBag.Servicios = GetServicioList();
            return View();
        }

        // GET: Turnos/ObtenerPrecioServicio
        [HttpGet]
        public JsonResult ObtenerPrecioServicio(string servicio)
        {
            var precio = _context.PreciosServicios
                .Where(p => p.Servicio == servicio)
                .Select(p => p.Precio)
                .FirstOrDefault();
            return Json(precio ?? 0);
        }

        // POST: Turnos/Reservar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reservar(Turno turno, string HorarioTurno, string Comentarios)
        {
            if (ModelState.IsValid)
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var usuario = _context.Usuarios.SingleOrDefault(u => u.Email == userEmail);

                if (usuario != null)
                {
                    turno.UsuarioID = usuario.UsuarioID;
                    turno.Comentarios = Comentarios;

                    if (turno.FechaTurno.HasValue && DateTime.TryParse(turno.FechaTurno.Value.ToString("yyyy-MM-dd") + " " + HorarioTurno, out DateTime fechaCompleta))

                    {
                        turno.FechaTurno = fechaCompleta;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al procesar la fecha y hora del turno.");
                        ViewBag.Servicios = GetServicioList();
                        return View(turno);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el usuario.");
                    ViewBag.Servicios = GetServicioList();
                    return View(turno);
                }

                _context.Turnos.Add(turno);
                _context.SaveChanges();

                return RedirectToAction("MisTurnos");
            }

            ViewBag.Servicios = GetServicioList();
            return View(turno);
        }

        // GET: Turnos/MisTurnos
        public IActionResult MisTurnos()
        {
            var userIdClaim = User.FindFirst("UsuarioID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var turnos = _context.Turnos
                .Include(t => t.Usuario)
                .Where(t => t.UsuarioID == userId)
                .Select(t => new Turno
                {
                    TurnoID = t.TurnoID,
                    UsuarioID = t.UsuarioID,
                    Servicio = t.Servicio ?? "Servicio no especificado",
                    FechaTurno = t.FechaTurno ?? (DateTime?)null, // Asegurarse de asignar null si no hay valor
                    Estado = t.Estado ?? "Estado no especificado",
                    Comentarios = t.Comentarios ?? "Sin comentarios",
                    MetodoPago = t.MetodoPago ?? "No disponible",
                    FechaPago = t.FechaPago ?? (DateTime?)null, // Asegurarse de asignar null si no hay valor
                    Usuario = t.Usuario
                })
                .ToList();

            ViewBag.PagoExitoso = TempData["PagoExitoso"];
            return View(turnos);
        }


        // POST: CompletarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompletarPago(int turnoId, string metodoPago)
        {
            var turno = _context.Turnos.SingleOrDefault(t => t.TurnoID == turnoId);
            if (turno != null && turno.Estado == "Pendiente")
            {
                turno.MetodoPago = metodoPago;
                turno.FechaPago = DateTime.Now;
                turno.Estado = "Confirmado";
                _context.SaveChanges();
                TempData["PagoExitoso"] = "Pago exitosamente completado.";
            }
            return RedirectToAction("MisTurnos");
        }

        // POST: Turnos/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ActualizarEstado(Dictionary<int, string> estados)
        {
            try
            {
                foreach (var estado in estados)
                {
                    if (estado.Key != 0 && !string.IsNullOrEmpty(estado.Value))
                    {
                        var turno = _context.Turnos.SingleOrDefault(t => t.TurnoID == estado.Key);
                        if (turno != null)
                        {
                            turno.Estado = estado.Value;
                        }
                    }
                }

                _context.SaveChanges();
                return Json(new { success = true, message = "El estado del turno se ha actualizado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el estado del turno: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error al actualizar el estado del turno." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarTurno([FromBody] int turnoID)
        {
            try
            {
                var turno = _context.Turnos.Find(turnoID);
                if (turno != null)
                {
                    _context.Turnos.Remove(turno);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Turno eliminado correctamente." });
                }
                return Json(new { success = false, message = "No se encontró el turno a eliminar." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el turno: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error al eliminar el turno." });
            }
        }
    }
}

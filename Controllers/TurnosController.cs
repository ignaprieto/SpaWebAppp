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
using SpaWebApp.Services;

namespace SpaWebApp.Controllers
{
    [Authorize]
    public class TurnosController : Controller
    {
        private readonly SpaContext _context;
        private readonly EmailService _emailService;

        public TurnosController(SpaContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                        // Validación de fecha y horario
                        if (fechaCompleta.Date < DateTime.Now.Date)
                        {
                            ModelState.AddModelError("FechaTurno", "La fecha del turno debe ser posterior a la fecha actual.");
                        }
                        else if (fechaCompleta.TimeOfDay < TimeSpan.FromHours(8) || fechaCompleta.TimeOfDay > TimeSpan.FromHours(20))
                        {
                            ModelState.AddModelError("HorarioTurno", "El horario del turno debe estar entre las 8:00 y las 20:00.");
                        }
                        else
                        {
                            turno.FechaTurno = fechaCompleta;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al procesar la fecha y hora del turno.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el usuario.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Servicios = GetServicioList();
                    return View(turno); // Mostrar los errores en la vista
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
                    FechaTurno = t.FechaTurno ?? (DateTime?)null,
                    Estado = t.Estado ?? "Estado no especificado",
                    Comentarios = t.Comentarios ?? "Sin comentarios",
                    MetodoPago = t.MetodoPago ?? "No disponible",
                    FechaPago = t.FechaPago ?? (DateTime?)null,
                    CodDescuento = t.CodDescuento ?? "No utilizado",
                    Usuario = t.Usuario
                })
                .ToList();

            // Cargar precios de los servicios en ViewBag
            var precios = _context.PreciosServicios.ToDictionary(p => p.Servicio, p => p.Precio);
            ViewBag.Precios = precios;

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

        //GUARDAR DATOS DE TARJETA
        /* [HttpPost]
         public async Task<IActionResult> ProcesarPago(int turnoId, string numeroTarjeta, string codigoTarjeta, DateTime fechaExpiracionTarjeta, string metodoPago)
         {
             // Cargar el turno junto con el usuario
             var turno = _context.Turnos
                 .Include(t => t.Usuario)
                 .SingleOrDefault(t => t.TurnoID == turnoId);

             if (turno == null)
             {
                 return NotFound();
             }

             // Guardar los datos de la tarjeta en el turno y confirmar el turno
             turno.NumeroTarjeta = numeroTarjeta;
             turno.CodigoTarjeta = codigoTarjeta;
             turno.FechaExpiracionTarjeta = fechaExpiracionTarjeta;
             turno.FechaPago = DateTime.Now;
             turno.Estado = "Confirmado";
             turno.MetodoPago = metodoPago;

             _context.SaveChanges();

             // Generar y enviar la factura por correo
             var usuarioEmail = turno.Usuario.Email;
             var cuerpoHtml = GenerarFacturaHtml(turno);
             await _emailService.EnviarFactura(usuarioEmail, "Factura de Pago - Spa", cuerpoHtml);

             return RedirectToAction("MisTurnos");
         }*/

        [HttpPost]
        public async Task<IActionResult> ProcesarPago(
    int turnoId,
    string numeroTarjeta,
    string codigoTarjeta,
    DateTime fechaExpiracionTarjeta,
    string metodoPago,
    string codDescuento)
        {
            // Cargar el turno junto con el usuario
            var turno = _context.Turnos
                .Include(t => t.Usuario)
                .SingleOrDefault(t => t.TurnoID == turnoId);

            if (turno == null)
            {
                return NotFound();
            }

            // Verificar si el código de descuento es válido
            decimal descuento = 0;
            if (!string.IsNullOrEmpty(codDescuento))
            {
                switch (codDescuento)
                {
                    case "Descuento10":
                        descuento = 0.10m;
                        break;
                    case "Descuento20":
                        descuento = 0.20m;
                        break;
                    case "Descuento30":
                        descuento = 0.30m;
                        break;
                    default:
                        TempData["Error"] = "Código de descuento no válido.";
                        return RedirectToAction("MisTurnos");
                }
            }

            // Obtener el precio del servicio
            var precioServicio = _context.PreciosServicios
                .Where(p => p.Servicio == turno.Servicio)
                .Select(p => p.Precio)
                .FirstOrDefault();

            if (precioServicio == null)
            {
                TempData["Error"] = "Error al obtener el precio del servicio.";
                return RedirectToAction("MisTurnos");
            }

            // Calcular el precio con descuento
            var precioConDescuento = descuento > 0 ? precioServicio.Value * (1 - descuento) : precioServicio.Value;

            // Guardar los datos de la tarjeta, el código de descuento y confirmar el turno
            turno.NumeroTarjeta = numeroTarjeta;
            turno.CodigoTarjeta = codigoTarjeta;
            turno.FechaExpiracionTarjeta = fechaExpiracionTarjeta;
            turno.FechaPago = DateTime.Now;
            turno.Estado = "Confirmado";
            turno.MetodoPago = metodoPago;
            turno.CodDescuento = codDescuento;

            _context.SaveChanges();

            // Generar y enviar la factura por correo
            var usuarioEmail = turno.Usuario.Email;
            var cuerpoHtml = GenerarFacturaHtml(turno, precioServicio.Value, precioConDescuento, descuento);
            await _emailService.EnviarFactura(usuarioEmail, "Factura de Pago - Spa", cuerpoHtml);

            TempData["PagoExitoso"] = "Pago exitosamente completado.";
            return RedirectToAction("MisTurnos");
        }


        // MÉTODO PARA GENERAR FACTURA

        private string GenerarFacturaHtml(Turno turno, decimal precioOriginal, decimal precioConDescuento, decimal descuento)
        {
            // Obtener el precio del servicio desde la base de datos
            var precioServicio = _context.PreciosServicios
                .Where(p => p.Servicio == turno.Servicio)
                .Select(p => p.Precio)
                .FirstOrDefault();

            return $@"
        <h2>Factura de Pago</h2>
        <p><strong>Servicio:</strong> {turno.Servicio}</p>
        <p><strong>Fecha del Turno:</strong> {turno.FechaTurno?.ToString("dd/MM/yyyy HH:mm")}</p>
        <p><strong>Método de Pago:</strong> {turno.MetodoPago}</p>
        <p><strong>Precio Original:</strong> ${precioOriginal:F2}</p>
        <p><strong>Descuento Aplicado:</strong> {descuento * 100}%</p>
        <p><strong>Precio Final:</strong> ${precioConDescuento:F2}</p>
        <p>Gracias por elegir nuestro spa. ¡Esperamos verlo pronto!</p>";
        }



    }

}

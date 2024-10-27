using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaWebApp.Data;
using SelectPdf;
using System;
using System.Linq;

namespace SpaWebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly SpaContext _context;

        public ReportesController(SpaContext context)
        {
            _context = context;
        }

        // Método para la vista de Informe de Servicios por Profesional
        public IActionResult InformeServiciosProfesional()
        {
            ViewBag.Profesionales = _context.Usuarios
                .Where(u => u.Rol == "Profesional")
                .Select(u => new { u.UsuarioID, Nombre = $"{u.Nombre} {u.Apellido}" })
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult InformeServiciosProfesional(DateTime fechaInicio, DateTime fechaFin, int profesionalId)
        {
            var servicios = _context.Turnos
                .Include(t => t.Usuario)
                .Where(t => t.UsuarioID == profesionalId && t.FechaTurno >= fechaInicio && t.FechaTurno <= fechaFin)
                .Select(t => new
                {
                    t.Servicio,
                    t.FechaTurno,
                    Cliente = t.Usuario != null ? $"{t.Usuario.Nombre} {t.Usuario.Apellido}" : "N/A",
                    Precio = _context.PreciosServicios.FirstOrDefault(p => p.Servicio == t.Servicio).Precio ?? 0
                })
                .ToList();

            ViewBag.Servicios = servicios;
            ViewBag.FechaInicio = fechaInicio.ToString("dd/MM/yyyy");
            ViewBag.FechaFin = fechaFin.ToString("dd/MM/yyyy");
            ViewBag.ProfesionalId = profesionalId;

            return View();
        }

        [HttpPost]
        public IActionResult GenerarPDFServiciosProfesional(DateTime fechaInicio, DateTime fechaFin, int profesionalId)
        {
            var servicios = _context.Turnos
                .Include(t => t.Usuario)
                .Where(t => t.UsuarioID == profesionalId && t.FechaTurno >= fechaInicio && t.FechaTurno <= fechaFin)
                .Select(t => new
                {
                    t.Servicio,
                    t.FechaTurno,
                    Cliente = t.Usuario != null ? $"{t.Usuario.Nombre} {t.Usuario.Apellido}" : "N/A",
                    Precio = _context.PreciosServicios.FirstOrDefault(p => p.Servicio == t.Servicio).Precio ?? 0
                })
                .ToList();

            var htmlContent = "<h1>Informe de Servicios por Profesional</h1>";
            htmlContent += $"<p><strong>Fecha de Inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>";
            htmlContent += $"<p><strong>Fecha de Fin:</strong> {fechaFin:dd/MM/yyyy}</p>";
            htmlContent += "<table style='width: 100%; border-collapse: collapse;'>";
            htmlContent += "<tr><th style='border: 1px solid #ddd; padding: 8px;'>Servicio</th><th style='border: 1px solid #ddd; padding: 8px;'>Fecha</th><th style='border: 1px solid #ddd; padding: 8px;'>Cliente</th><th style='border: 1px solid #ddd; padding: 8px;'>Precio</th></tr>";

            foreach (var servicio in servicios)
            {
                htmlContent += $"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{servicio.Servicio}</td><td style='border: 1px solid #ddd; padding: 8px;'>{servicio.FechaTurno:dd/MM/yyyy}</td><td style='border: 1px solid #ddd; padding: 8px;'>{servicio.Cliente}</td><td style='border: 1px solid #ddd; padding: 8px;'>{servicio.Precio:C}</td></tr>";
            }

            htmlContent += "</table>";

            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument doc = converter.ConvertHtmlString(htmlContent);

            byte[] pdf = doc.Save();
            doc.Close();

            return File(pdf, "application/pdf", "InformeServiciosProfesional.pdf");
        }

        // Método para la vista de Informe de Ingresos
        public IActionResult InformeIngresos()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InformeIngresos(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = _context.Turnos
                .Where(t => t.FechaPago >= fechaInicio && t.FechaPago <= fechaFin && t.Estado == "Confirmado")
                .Join(
                    _context.PreciosServicios,
                    turno => turno.Servicio,
                    precioServicio => precioServicio.Servicio,
                    (turno, precioServicio) => new
                    {
                        turno.MetodoPago,
                        Precio = precioServicio.Precio
                    }
                )
                .GroupBy(t => t.MetodoPago)
                .Select(g => new
                {
                    MetodoPago = g.Key,
                    TotalIngresos = g.Sum(t => t.Precio)
                })
                .ToList();

            ViewBag.Ingresos = ingresos;
            ViewBag.FechaInicio = fechaInicio.ToString("dd/MM/yyyy");
            ViewBag.FechaFin = fechaFin.ToString("dd/MM/yyyy");

            return View();
        }

        [HttpPost]
        public IActionResult GenerarPDFIngresos(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = _context.Turnos
                .Where(t => t.FechaPago >= fechaInicio && t.FechaPago <= fechaFin && t.Estado == "Confirmado")
                .Join(
                    _context.PreciosServicios,
                    turno => turno.Servicio,
                    precioServicio => precioServicio.Servicio,
                    (turno, precioServicio) => new
                    {
                        turno.MetodoPago,
                        Precio = precioServicio.Precio
                    }
                )
                .GroupBy(t => t.MetodoPago)
                .Select(g => new
                {
                    MetodoPago = g.Key,
                    TotalIngresos = g.Sum(t => t.Precio)
                })
                .ToList();

            var htmlContent = "<h1>Informe de Ingresos</h1>";
            htmlContent += $"<p><strong>Fecha de Inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>";
            htmlContent += $"<p><strong>Fecha de Fin:</strong> {fechaFin:dd/MM/yyyy}</p>";
            htmlContent += "<table style='width: 100%; border-collapse: collapse;'>";
            htmlContent += "<tr><th style='border: 1px solid #ddd; padding: 8px;'>Método de Pago</th><th style='border: 1px solid #ddd; padding: 8px;'>Total Ingresos</th></tr>";

            foreach (var ingreso in ingresos)
            {
                htmlContent += $"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{ingreso.MetodoPago}</td><td style='border: 1px solid #ddd; padding: 8px;'>{ingreso.TotalIngresos:C}</td></tr>";
            }

            htmlContent += "</table>";

            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument doc = converter.ConvertHtmlString(htmlContent);

            byte[] pdf = doc.Save();
            doc.Close();

            return File(pdf, "application/pdf", "InformeIngresos.pdf");
        }
    }
}

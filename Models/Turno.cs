using System;
using System.ComponentModel.DataAnnotations;

namespace SpaWebApp.Models
{
    public class Turno
    {
        public int TurnoID { get; set; }

        [Required]
        public int UsuarioID { get; set; }
        public Usuario? Usuario { get; set; } // Modificado para permitir nulo

        [Required]
        public string? Servicio { get; set; } // Modificado para permitir nulo

        [Required]
        public DateTime? FechaTurno { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public string Comentarios { get; set; } = string.Empty;

        public string? MetodoPago { get; set; } // Modificado para permitir nulo
        public DateTime? FechaPago { get; set; }

        public string NombreCompletoCliente => Usuario != null ? $"{Usuario.Nombre} {Usuario.Apellido}" : "N/A";
    }
}

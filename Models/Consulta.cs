using System;
using System.ComponentModel.DataAnnotations;

namespace SpaWebApp.Models
{
    public class Consulta
    {
        public int ConsultaID { get; set; }

        [Required]
        public int UsuarioID { get; set; } // Asegúrate de que coincida con la base de datos
        public Usuario? Usuario { get; set; } // Permitir que Usuario sea nullable

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        public string? Respuesta { get; set; } // Permitir que Respuesta sea nullable

        public DateTime FechaConsulta { get; set; } = DateTime.Now;
        public DateTime? FechaRespuesta { get; set; }
    }
}

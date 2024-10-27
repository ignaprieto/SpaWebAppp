using System;
using System.ComponentModel.DataAnnotations;

namespace SpaWebApp.Models
{
    public class Comentarios
    {
        public int ComentarioID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Comentario { get; set; } = string.Empty; // Nombre coincidente con el de la base de datos

        public DateTime FechaComentario { get; set; } = DateTime.Now;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SpaWebApp.Models
{
    public class PrecioServicio
    {
        [Key]
        public string Servicio { get; set; } // Nombre del servicio

        public decimal? Precio { get; set; } // Precio del servicio, opcional
    }
}

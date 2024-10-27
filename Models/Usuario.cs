using System;
using System.ComponentModel.DataAnnotations;

namespace SpaWebApp.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public string ContraseñaHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = string.Empty;
    }

    public enum RolEnum
    {
        Cliente,
        Profesional,
        Administrador
    }
}

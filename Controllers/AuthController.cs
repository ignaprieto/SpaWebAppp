using Microsoft.AspNetCore.Mvc;
using SpaWebApp.Data;
using SpaWebApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BCrypt.Net;  // Asegúrate de instalar BCrypt.Net-Next
using System.Collections.Generic;

namespace SpaWebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly SpaContext _context;

        public AuthController(SpaContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login()
        {
            ViewData["ShowRegisterLink"] = true; // Mostrar enlace de registro en la vista de inicio de sesión
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Usuarios.SingleOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.ContraseñaHash))
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
                return View();
            }

            // Crear los claims (información de usuario) para la autenticación
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Rol),
                new Claim("UsuarioID", user.UsuarioID.ToString()) // Asegúrate de incluir el ID del usuario
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Autenticar al usuario
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: Register
        public IActionResult Register()
        {
            ViewData["ShowRegisterLink"] = false; // No mostrar enlace de registro en la vista de registro
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(string Nombre, string Apellido, string Email, string Telefono, string Direccion, string Password)
        {
            var existingUser = _context.Usuarios.SingleOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Ya existe un usuario con ese email.");
                return View();
            }

            var newUser = new Usuario
            {
                Nombre = Nombre,
                Apellido = Apellido,
                Email = Email,
                Telefono = Telefono,
                Direccion = Direccion,
                FechaRegistro = DateTime.Now,
                ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(Password), // Encriptar la contraseña
                Rol = "Cliente"
            };

            _context.Usuarios.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Auth");
        }
    }
}

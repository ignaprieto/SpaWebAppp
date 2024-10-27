using Microsoft.AspNetCore.Mvc;

namespace SpaWebApp.Controllers
{
    public class NoticiasController : Controller
    {
        public IActionResult Index()
        {
            // Aquí puedes pasar datos o un modelo de noticias desde la base de datos.
            return View();
        }

        public IActionResult Detalle(int id)
        {
            // Títulos y descripciones de ejemplo
            var noticias = new Dictionary<int, (string Titulo, string Descripcion)>
                {
                    { 1, ("El mejor spa del mundo está en Córdoba", "El Azur Real Hotel Boutique, ubicado en Córdoba, ha sido galardonado con el premio al \"Mejor spa del mundo\" en los Luxury Spa Awards 2023. Este spa destaca por su diseño subterráneo inspirado en los baños romanos y su enfoque en ofrecer una experiencia única de bienestar y conexión personal​.") },
                    { 2, ("Auge del turismo de bienestar en Argentina", "La tendencia del turismo de bienestar está en auge en el país, con cada vez más personas buscando experiencias relajantes en spas. Los establecimientos en regiones como las sierras de Córdoba y la Patagonia están adaptándose para ofrecer servicios exclusivos que combinan naturaleza, relajación y tratamientos de lujo.") },
                    { 3, ("Spas en Buenos Aires: una opción popular para el estrés urbano", "En Buenos Aires, los spas urbanos han ganado popularidad como un escape del ritmo acelerado de la ciudad. Lugares como Aqua Vita Spa y Serena Spa son conocidos por ofrecer tratamientos que ayudan a los porteños a relajarse y desconectarse sin salir de la capital​.") }
                };

            if (noticias.ContainsKey(id))
            {
                ViewBag.Titulo = noticias[id].Titulo;
                ViewBag.Descripcion = noticias[id].Descripcion;
                ViewBag.Id = id; // Para la imagen de la noticia
            }
            else
            {
                return NotFound(); // Maneja los casos donde el id no exista
            }

            return View();
        }


    }
}
using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Models;

namespace BibliotecaPrjt.Controllers
{
    public class LibroController : Controller
    {
        public IActionResult Index()
        {
            List<Libro> libros = new List<Libro>()
            {
                new Libro
                {
                    ID = 1,
                    Titulo = "Cien años de soledad ",
                    Autor = "Gabriel García Márquez",
                    Categoria = "Realismo Magico",
                    Precio = 25.15M,
                    Disponible = true

                },
                new Libro
                {
                    ID = 1,
                    Titulo = "Don Quijote de la Mancha",
                    Autor = "Miguel de Cervantes",
                    Categoria = "Novela",
                    Precio = 30.15M,
                    Disponible = false

                }
            };

            ViewBag.Libro = libros;

            return View();
        }
    }
}

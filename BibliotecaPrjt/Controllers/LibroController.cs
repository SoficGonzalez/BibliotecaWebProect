using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BibliotecaPrjt.Controllers
{
    public class LibroController : Controller
    {
        private static List<Libro> _libros = new List<Libro>
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
            ID = 2,
            Titulo = "Don Quijote de la Mancha",
            Autor = "Miguel de Cervantes",
            Categoria = "Novela",
            Precio = 30.15M,
            Disponible = false

        }
    };

        public IActionResult Index()
        {

            return View(_libros);
        }

        public IActionResult Details(int id)
        {
            var libro = _libros.FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();

            }
            return View(libro);
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Libro libro, IFormFile imagenPortada)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            if (imagenPortada != null && imagenPortada.Length > 0)
            { 
                var extension = Path.GetExtension(imagenPortada.FileName);
                var nombreArchivoUnico = Guid.NewGuid().ToString() + extension;

                var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "libros");

                if (!Directory.Exists(rutaCarpeta))
                {
                    Directory.CreateDirectory(rutaCarpeta);
                }

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivoUnico);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    imagenPortada.CopyTo(stream);
                }

                
                libro.ImageUrl = "/images/libros/" + nombreArchivoUnico;
            }
            else
            {
                
                libro.ImageUrl = "/images/libros/default-book.png";
            }

            if (_libros.Any())
            {
                libro.ID = _libros.Max(x => x.ID) + 1;
            }
            else
            {
                libro.ID = 1;
            }

            _libros.Add(libro);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            var libro = _libros.FirstOrDefault(_ => _.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Libro libro)
        {
            if (id != libro.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            var existente = _libros.FirstOrDefault(x => x.ID == id);
            if (existente == null)
            {
                return NotFound();
            }

            existente.Titulo = libro.Titulo;
            existente.Autor = libro.Autor;
            existente.Categoria = libro.Categoria;
            existente.Precio = libro.Precio;
            existente.Disponible = libro.Disponible;

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var libro = _libros.FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var libro = _libros.FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }

            _libros.Remove(libro);
            return RedirectToAction(nameof(Index));

        }



    }



}

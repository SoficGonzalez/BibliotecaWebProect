using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BibliotecaPrjt.Controllers
{
    public class LibroController : Controller
    {
        private readonly IRepositorioLibro _repositorio;

        public LibroController(IRepositorioLibro repositorio)
        {
            _repositorio = repositorio;
        }

        private List<Libro> ObtenerListaInterna()
        {
            return _repositorio.ObtenerTodos() as List<Libro> ?? new List<Libro>();
        }

        public IActionResult Index()
        {
            var libros = _repositorio.ObtenerTodos();
            return View(libros);
        }

        public IActionResult Details(int id)
        {
            var libro = _repositorio.ObtenerTodos().FirstOrDefault(x => x.ID == id);
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

        //Non functional
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

            var listaLibros = ObtenerListaInterna();

            libro.ID = listaLibros.Any() ? listaLibros.Max(x => x.ID) + 1 : 1;

            listaLibros.Add(libro);
            return RedirectToAction(nameof(Index));
        }


        //Non functional
        public IActionResult Edit(int id)
        {
            var libro = _repositorio.ObtenerTodos().FirstOrDefault(_ => _.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }


        //Non functional
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

            var existente = _repositorio.ObtenerTodos().FirstOrDefault(x => x.ID == id);
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

        //Non functional
        public IActionResult Delete(int id)
        {
            var libro = _repositorio.ObtenerTodos().FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }

        //Non functional
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var listaLibros = ObtenerListaInterna();
            var libro = listaLibros.FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }

            listaLibros.Remove(libro);
            return RedirectToAction(nameof(Index));

        }



    }



}

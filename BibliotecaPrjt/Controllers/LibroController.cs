using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;
using BibliotecaPrjt.Data;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPrjt.Controllers
{
    public class LibroController : Controller
    {
        /* private readonly IRepositorioLibro _repositorio;

         public LibroController(IRepositorioLibro repositorio)
         {
             _repositorio = repositorio;
         }

         private List<Libro> ObtenerListaInterna()
         {
             return _repositorio.ObtenerTodos() as List<Libro> ?? new List<Libro>();
         }*/

        public readonly BibliotecaContext _context;
        public LibroController(BibliotecaContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var libros = await _context.Libros.ToListAsync();
            return View(libros);
        }

        public async Task<IActionResult> Details(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
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
        public async Task<IActionResult> Create(Libro libro, IFormFile imagenPortada)
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

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        
       public async Task<IActionResult> Edit(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            if (id != libro.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            var existente = await _context.Libros.AnyAsync(a => a.ID == id);
            if(!existente)
            {
                return NotFound();
            }

            _context.Update(libro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        
        /*public IActionResult Delete(int id)
        {
            var libro = _repositorio.ObtenerTodos().FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }*/

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listaLibros = await _context.Libros.FindAsync(id);
            if (listaLibros == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(listaLibros);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }



}

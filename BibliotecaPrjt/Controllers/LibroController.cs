using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;
using BibliotecaPrjt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliotecaPrjt.Controllers
{
    [Authorize]
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

        private readonly BibliotecaContext _context;

        private readonly ILogger<LibroController> _logger;
        public LibroController(BibliotecaContext context, ILogger<LibroController>logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de libros");
            
            try
            {
                var libros = await _context.Libros.ToListAsync();
                return View(libros);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar el listado de libros");
                throw;
            }
            
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Consultando los detalles del libro con identificador {LibroId}", id);
            
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    _logger.LogWarning("No se encontro el libro solicitado con identificador {LibroId}", id);
                    return NotFound();

                }
                return View(libro);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del libro con identificador {LibroId}", id);
                throw;
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operacion de creacion de un libro");
            return View();
        }

       
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Libro libro, IFormFile imagenPortada)
        {
            _logger.LogInformation("Iniciando la operacion de creacion de un libro");
            if (!ModelState.IsValid)
            {
                _logger.LogError("El libro enviado no cumple con los requisitos");
                return View(libro);
            }

            try
            {
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
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al crear el libro");
                throw;
            }
        }



        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Iniciando la operacion de edicion del libro con identificador {LibroId}", id);
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }
                return View(libro);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del libro para su edicion con identificador {Libroid}", id);
                return View();
            }
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            _logger.LogInformation("Iniciando la edicion del libro con identificador {LibroId}", id);
            if (id != libro.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            try
            {
                var existente = await _context.Libros.AnyAsync(a => a.ID == id);
                if (!existente)
                {
                    return NotFound();
                }

                _context.Update(libro);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al editar el libro con identificador {Libroid}", id);
                throw;
            }
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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Iniciando la eliminacion del libro con identificador {LibroId}", id);

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar el libro con identificador {LirboId}", id);
                throw;
            }

        }
    }



}

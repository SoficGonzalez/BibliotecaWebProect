using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BibliotecaPrjt.Models;
using BibliotecaPrjt.Data;
using Microsoft.Extensions.Logging;

namespace BibliotecaPrjt.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(BibliotecaContext context, ILogger<CategoriasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de categorias");

            try
            {
                var categorias = await _context.Categorias.ToListAsync();
                return View(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar el listado de categorias");
                throw;
            }
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operacion de creacion de una categoria");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            _logger.LogInformation("Iniciando la creacion de una categoria");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("La categoria enviada no cumple con los requisitos");
                return View(categoria);
            }

            try
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Categoria guardada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear la categoria");
                throw;
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Iniciando la operacion de edicion de la categoria con identificador {CategoriaId}", id);

            try
            {
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    _logger.LogWarning("No se encontro la categoria solicitada con identificador {CategoriaId}", id);
                    return NotFound();
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar la categoria para su edicion con identificador {CategoriaId}", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            _logger.LogInformation("Iniciando la edicion de la categoria con identificador {CategoriaId}", id);

            if (id != categoria.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            try
            {
                var existe = await _context.Categorias.AnyAsync(c => c.ID == id);
                if (!existe)
                {
                    _logger.LogWarning("No se encontro la categoria solicitada con identificador {CategoriaId}", id);
                    return NotFound();
                }

                _context.Update(categoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Categoria actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al editar la categoria con identificador {CategoriaId}", id);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Consultando la categoria con identificador {CategoriaId} para su eliminacion", id);

            try
            {
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    _logger.LogWarning("No se encontro la categoria solicitada con identificador {CategoriaId}", id);
                    return NotFound();
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar la categoria con identificador {CategoriaId}", id);
                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            _logger.LogInformation("Iniciando la eliminacion de la categoria con identificador {CategoriaId}", id);

            try
            {
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    _logger.LogWarning("No se encontro la categoria solicitada con identificador {CategoriaId}", id);
                    return NotFound();
                }

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Categoria eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar la categoria con identificador {CategoriaId}", id);
                throw;
            }
        }
    }
}
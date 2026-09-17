using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPrjt.Data;

namespace BibliotecaPrjt.Controllers
{
    public class AutoresController : Controller
    {

        /* public readonly IAutorService _repositorio;

         public AutoresController(IAutorService repositorio) 
         { 
             _repositorio = repositorio;
         }


         private List<Autor> ObtenerListaInterna()
         {

             return _repositorio.ObtenerAutores() as List<Autor> ?? new List<Autor>();

         }*/

        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }


       
        public async Task<IActionResult> Index()
        {
            var autores = await _context.Autores.ToListAsync();
            return View(autores);
        }

        public async Task<IActionResult> Details(int id) 
        {
            var autor = await _context.Autores.FindAsync(id);
            if(autor == null)
            {
                return NotFound();

            }
            return View(autor);
        }

        public IActionResult Create() 
        {

            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        /*
        public IActionResult Edit(int id) 
        { 
            var autor = _autores.FirstOrDefault(_ => _.ID == id);
            if(autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Autor autor)
        {
            if(id != autor.ID)
            {
                return NotFound();
            }

            if(!ModelState.IsValid)
            {
                return View(autor);
            }

            var existente = _autores.FirstOrDefault(x => x.ID == id);
            if (existente == null)
            {
                return NotFound();
            }

            existente.Nombre = autor.Nombre;
            existente.Nacionalidad = autor.Nacionalidad;
            existente.FechaDeNacimiento = autor.FechaDeNacimiento;
            existente.Activo = autor.Activo;

            return RedirectToAction(nameof(Index));
        } */

       /*
        public IActionResult Delete(int id)
        {
            var autor = _autores.FirstOrDefault(x => x.ID == id);
            if(autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var autor = _autores.FirstOrDefault(x => x.ID == id);
            if(autor == null)
            {
                return NotFound();
            }

            _autores.Remove(autor);
            return RedirectToAction(nameof(Index));

        } */


    }
}

using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BibliotecaPrjt.Data;

namespace BibliotecaPrjt.Controllers
{
    [Authorize]
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
        
        
        public async Task<IActionResult> Edit(int id) 
        {
            var autor = await _context.Autores.FindAsync(id);
            
            if (autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Autor autor)
        {
            if(id != autor.ID)
            {
                return BadRequest();
            }

            if(!ModelState.IsValid)
            {
                return View(autor);
            }
            
            var exists = await _context.Autores.AnyAsync(a => a.ID == id);
            if(!exists)
            {
                return NotFound();
            }

            _context.Update(autor);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        } 

        /*
         public IActionResult Delete(int id)
         {
             var autor = _autores.FirstOrDefault(x => x.ID == id);
             if(autor == null)
             {
                 return NotFound();
             }
             return View(autor);
         }*/

         [HttpPost, ActionName("Delete")]
         [ValidateAntiForgeryToken]
         public  async Task<IActionResult> DeleteConfirmed(int id)
         {
            var autor = await _context.Autores.FindAsync(id);
             if(autor == null)
             {
                 return NotFound();
             }

             //_context.Remove(autor);
             _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index));

         } 


    }
}

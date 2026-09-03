using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaPrjt.Controllers
{
    public class AutoresController : Controller
    {

        public readonly IAutorService _repositorio;

        public AutoresController(IAutorService repositorio) 
        { 
            _repositorio = repositorio;
        }
        
        
        private List<Autor> ObtenerListaInterna()
        {
            
            return _repositorio.ObtenerAutores() as List<Autor> ?? new List<Autor>();

        }

       
        public IActionResult Index()
        {
            var autor = _repositorio.ObtenerAutores();
            return View(autor);
        }

        public IActionResult Details(int id) 
        { 
            var autor = _repositorio.ObtenerAutores().FirstOrDefault(x => x.ID == id);
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

       
        //[HttpPost]
       // [ValidateAntiForgeryToken]
       // public IActionResult Create(Autor autor)
       // {
         //   if (!ModelState.IsValid)
         //   {
               // return View(autor);
          //  }

          //  if(_repositorio.Any())
           // {
           //     autor.ID = _autores.Max(x => x.ID) + 1;
           // }
           // else
            //{
            //    autor.ID = 1;
            //}

            

           // _autores.Add(autor);
            //return RedirectToAction(nameof(Index));
       // }
        
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

using BibliotecaPrjt.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaPrjt.Controllers
{
    public class AutoresController : Controller
    {
        private static List<Autor> _autores = new List<Autor>
        {
            new Autor
            {
                ID = 1,
                Nombre = "Gabriel García Márquez",
                Nacionalidad = "Colombiana",
                FechaDeNacimiento = new DateTime(1927,3,6),
                Activo = true

            }, new Autor
            {
                ID = 2,
                Nombre = "Miguel de Cervantes Saavedra",
                Nacionalidad = "Española",
                FechaDeNacimiento = new DateTime(1547,9,12),
                Activo = false

            },
              new Autor
            {
                ID = 3,
                Nombre = "Salvador Efraín Salazar Arrué",
                Nacionalidad = "Salvadoreña",
                FechaDeNacimiento = new DateTime(1899,10,22),
                Activo = true

            },
               new Autor
            {
                ID = 4,
                Nombre = "Isabel Allende",
                Nacionalidad = "Chilena",
                FechaDeNacimiento = new DateTime(1942,8,2),
                Activo = true

            },
             new Autor
            {
                ID = 5,
                Nombre = "Franz Kafka",
                Nacionalidad = "Austrohúngara",
                FechaDeNacimiento = new DateTime(1883,7,3),
                Activo = false

            }


        };

       
        public IActionResult Index()
        {
          
            return View(_autores);
        }

        public IActionResult Details(int id) 
        { 
            var autor = _autores.FirstOrDefault(x => x.ID == id);
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
        public IActionResult Create(Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            if(_autores.Any())
            {
                autor.ID = _autores.Max(x => x.ID) + 1;
            }
            else
            {
                autor.ID = 1;
            }

            

            _autores.Add(autor);
            return RedirectToAction(nameof(Index));
        }

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
        }

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

        }


    }
}

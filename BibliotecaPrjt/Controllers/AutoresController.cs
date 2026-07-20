using BibliotecaPrjt.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaPrjt.Controllers
{
    public class AutoresController : Controller
    {
        public IActionResult Index()
        {
            List<Autor> autores = new List<Autor>()
            {
                new Autor
                {
                    ID = 1,
                    Nombre = "Gabriel",
                    Apellido = "García Márquez",
                    Nacionalidad = "Colombiana",
                    FechaDeNacimiento = "6 de Marzo de 1927",
                    Activo = true

                },
                 new Autor
                {
                    ID = 2,
                    Nombre = "Miguel",
                    Apellido = "de Cervantes Saavedra",
                    Nacionalidad = "Española",
                    FechaDeNacimiento = "Septiembre de 1547",
                    Activo = false

                },
                  new Autor
                {
                    ID = 3,
                    Nombre = "Salvador Efraín",
                    Apellido = "Salazar Arrué",
                    Nacionalidad = "Salvadoreña",
                    FechaDeNacimiento = "22 de Octubre de 1899",
                    Activo = true

                },
                   new Autor
                {
                    ID = 4,
                    Nombre = "Isabel",
                    Apellido = "Allende",
                    Nacionalidad = "Chilena",
                    FechaDeNacimiento = "2 de Agosto de 1942",
                    Activo = true

                },
                 new Autor
                {
                    ID = 5,
                    Nombre = "Franz",
                    Apellido = "Kafka",
                    Nacionalidad = "Austrohúngara",
                    FechaDeNacimiento = "3 de julio de 1883",
                    Activo = false

                }
            };

            ViewBag.Autor = autores;
            return View();
        }
    }
}

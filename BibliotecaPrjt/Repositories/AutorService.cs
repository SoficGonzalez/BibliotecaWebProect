using BibliotecaPrjt.Models;

namespace BibliotecaPrjt.Repositories
{
    public class AutorService : IAutorService
    {
        public IEnumerable<Autor> ObtenerAutores()
        {
            return new List<Autor>()
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
        }
    }
}

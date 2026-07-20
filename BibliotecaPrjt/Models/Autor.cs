using Microsoft.AspNetCore.Mvc;

namespace BibliotecaPrjt.Models
{
    public class Autor
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Nacionalidad { get; set; }
        public string FechaDeNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}

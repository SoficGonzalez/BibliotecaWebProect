using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaPrjt.Models
{
    public class Autor
    {
        public int ID { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        [StringLength(50)]
        public string Nacionalidad { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaDeNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BibliotecaPrjt.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo electronico es obligatorio.")]
        [Display(Name = "Usuario o correo electronico")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatorio.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordar")]
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }

    }
}

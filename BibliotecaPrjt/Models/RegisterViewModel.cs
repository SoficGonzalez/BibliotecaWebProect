using System.ComponentModel.DataAnnotations;

namespace BibliotecaPrjt.Models
{
    public class RegisterViewModel
    {
        [Required (ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electronico es obligatorio.")]
        [EmailAddress(ErrorMessage ="Introduce un correo electronico valido.")]
        [Display(Name = "Email")]
        public string Email {  get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password {  get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmacion de la contrasena es obligatoria.")]
        [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword {  get; set; } = string.Empty;
    }
}

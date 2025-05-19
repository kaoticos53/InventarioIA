using System.ComponentModel.DataAnnotations;

namespace Inventario.Web.Models
{
    public class EmailVerificationModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de verificación es obligatorio")]
        public string Token { get; set; } = string.Empty;
    }
}

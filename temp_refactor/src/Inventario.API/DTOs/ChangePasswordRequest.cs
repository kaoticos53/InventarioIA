using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}

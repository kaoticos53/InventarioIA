using System.ComponentModel.DataAnnotations;

namespace Inventario.Web.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El token de restablecimiento es requerido")]
    public string Token { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

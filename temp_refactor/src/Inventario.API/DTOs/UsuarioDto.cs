using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.DTOs
{
    public class UsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool EmailConfirmado { get; set; }
        public bool Activo { get; set; } = true;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El {0} no puede tener más de {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El {0} no puede tener más de {1} caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        public string? Telefono { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUsuarioDto
    {
        [StringLength(50, ErrorMessage = "El {0} no puede tener más de {1} caracteres.")]
        public string? Nombre { get; set; }

        [StringLength(50, ErrorMessage = "El {0} no puede tener más de {1} caracteres.")]
        public string? Apellido { get; set; }

        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        public string? Telefono { get; set; }

        public bool? Activo { get; set; }
        public IList<string>? Roles { get; set; }
    }

    public class CambiarContrasenaDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
    }

    public class ResetearContrasenaDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;
    }

    public class ConfirmarResetearContrasenaDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
    }

    public class UsuarioFiltroDto
    {
        public string? Busqueda { get; set; }
        public bool? Activo { get; set; }
        public string? Rol { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanioPagina { get; set; } = 10;
    }

    public class UsuarioPaginacionDto
    {
        public IEnumerable<UsuarioDto> Usuarios { get; set; } = new List<UsuarioDto>();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}

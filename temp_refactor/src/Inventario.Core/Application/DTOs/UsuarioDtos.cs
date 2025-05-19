using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Application.DTOs
{
    public class UsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public bool EmailConfirmado { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }

    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUsuarioDto
    {
        [Required(ErrorMessage = "El ID es obligatorio")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUsuarioPerfilDto
    {
        [Required(ErrorMessage = "El ID es obligatorio")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        public string? Telefono { get; set; }
    }
}

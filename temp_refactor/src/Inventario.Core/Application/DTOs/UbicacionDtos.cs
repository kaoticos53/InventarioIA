using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Application.DTOs
{
    public class UbicacionDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
        public int TotalEquipos { get; set; }
    }

    public class CreateUbicacionDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }
    }

    public class UpdateUbicacionDto
    {
        [Required(ErrorMessage = "El ID es obligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Application.DTOs
{
    public class EquipoDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de serie es obligatorio")]
        [StringLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string NumeroSerie { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        public DateTime FechaCompra { get; set; }

        public DateTime? FechaFinGarantia { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Disponible";

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        public int UbicacionId { get; set; }
        public string? UbicacionNombre { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class CreateEquipoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de serie es obligatorio")]
        [StringLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string NumeroSerie { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        public DateTime FechaCompra { get; set; }

        public DateTime? FechaFinGarantia { get; set; }

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        public int UbicacionId { get; set; }
    }

    public class UpdateEquipoDto
    {
        [Required(ErrorMessage = "El ID es obligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de serie es obligatorio")]
        [StringLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string NumeroSerie { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        public DateTime FechaCompra { get; set; }

        public DateTime? FechaFinGarantia { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Disponible";

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        public int UbicacionId { get; set; }
    }
}

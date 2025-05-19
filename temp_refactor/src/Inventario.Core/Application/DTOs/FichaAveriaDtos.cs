using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Application.DTOs
{
    public class FichaAveriaDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El ID del equipo es obligatorio")]
        public int EquipoId { get; set; }
        public string? EquipoNombre { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Reportada"; // Reportada, En Revisión, Reparada, No Reparable

        public DateTime FechaReporte { get; set; } = DateTime.UtcNow;
        public DateTime? FechaResolucion { get; set; }
        public string? SolucionAplicada { get; set; }
        public string? UsuarioReporteId { get; set; }
        public string? UsuarioReporteNombre { get; set; }
        public string? UsuarioResolucionId { get; set; }
        public string? UsuarioResolucionNombre { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public int? TiempoReparacionMinutos { get; set; }
        public string? Comentarios { get; set; }
        public string? Prioridad { get; set; } // Baja, Media, Alta, Crítica
    }

    public class CreateFichaAveriaDto
    {
        [Required(ErrorMessage = "El ID del equipo es obligatorio")]
        public int EquipoId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        public string? Prioridad { get; set; } = "Media";
    }

    public class UpdateFichaAveriaDto
    {
        [Required(ErrorMessage = "El ID es obligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = string.Empty;

        public string? SolucionAplicada { get; set; }
        public string? Comentarios { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public string? Prioridad { get; set; }
    }

    public class AsignarFichaAveriaDto
    {
        [Required(ErrorMessage = "El ID del técnico es obligatorio")]
        public string UsuarioAsignadoId { get; set; } = string.Empty;
        
        public string? Comentarios { get; set; }
    }

    public class ResolverFichaAveriaDto
    {
        [Required(ErrorMessage = "La solución aplicada es obligatoria")]
        public string SolucionAplicada { get; set; } = string.Empty;
        
        public string? Comentarios { get; set; }
        public int? TiempoReparacionMinutos { get; set; }
    }
}

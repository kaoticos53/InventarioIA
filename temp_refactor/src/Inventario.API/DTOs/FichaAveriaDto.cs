using System;
using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.DTOs
{
    public class FichaAveriaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaReporte { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string Estado { get; set; } = "Reportada";
        public string? SolucionAplicada { get; set; }
        public int EquipoId { get; set; }
        public string? EquipoNombre { get; set; }
        public string UsuarioReporteId { get; set; } = string.Empty;
        public string? UsuarioReporteNombre { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public string? Prioridad { get; set; } = "Media";
        public string? Comentarios { get; set; }
    }

    public class CreateFichaAveriaDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del equipo es requerido")]
        public int EquipoId { get; set; }

        [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder los 500 caracteres")]
        public string? Comentarios { get; set; }
        
        public string? Prioridad { get; set; } = "Media";
    }

    public class UpdateFichaAveriaDto
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public string? SolucionAplicada { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public string? Prioridad { get; set; }
        public string? Comentarios { get; set; }
    }

    public class FichaAveriaFilterDto
    {
        public int? EquipoId { get; set; }
        public string? Estado { get; set; }
        public string? UsuarioReporteId { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public string? Prioridad { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool? IncluirResueltas { get; set; } = true;
    }
}

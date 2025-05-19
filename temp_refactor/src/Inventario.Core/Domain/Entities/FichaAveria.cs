using Inventario.Core.Domain.Common;

namespace Inventario.Core.Domain.Entities
{
    public class FichaAveria : BaseEntity
    {
        public int EquipoId { get; set; }
        public Equipo Equipo { get; set; } = null!;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = "Reportada"; // Reportada, En Revisión, Reparada, No Reparable
        public DateTime FechaReporte { get; set; } = DateTime.UtcNow;
        public DateTime? FechaResolucion { get; set; }
        public string? SolucionAplicada { get; set; }
        public string? UsuarioReporteId { get; set; }
        public ApplicationUser? UsuarioReporte { get; set; }
        public string? UsuarioResolucionId { get; set; }
        public ApplicationUser? UsuarioResolucion { get; set; }
        public string? UsuarioAsignadoId { get; set; }
        public ApplicationUser? UsuarioAsignado { get; set; }
        public int? TiempoReparacionMinutos { get; set; }
        public string? Comentarios { get; set; }
        public string? Prioridad { get; set; }
    }
}

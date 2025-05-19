using Inventario.Core.Domain.Common;

namespace Inventario.Core.Domain.Entities
{
    public class Equipo : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaFinGarantia { get; set; }
        public string Estado { get; set; } = "Disponible"; // Disponible, En Uso, En Mantenimiento, Baja
        public int UbicacionId { get; set; }
        public Ubicacion Ubicacion { get; set; } = null!;
        public ICollection<FichaAveria> FichasAveria { get; set; } = new List<FichaAveria>();
        public string? UsuarioCreacionId { get; set; }
        public ApplicationUser? UsuarioCreacion { get; set; }
    }
}

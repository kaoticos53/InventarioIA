using Inventario.Core.Domain.Common;

namespace Inventario.Core.Domain.Entities
{
    public class Ubicacion : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    }
}

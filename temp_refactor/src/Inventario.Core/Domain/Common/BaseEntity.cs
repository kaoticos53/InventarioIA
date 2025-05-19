using System;

namespace Inventario.Core.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;
    }
}

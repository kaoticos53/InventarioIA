using System;

namespace Inventario.Core.DTOs
{
    public class RolDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

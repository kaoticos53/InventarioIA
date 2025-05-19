using Microsoft.AspNetCore.Identity;

namespace Inventario.Core.Entities
{
    public class RolPersonalizado : IdentityRole
    {
        public RolPersonalizado() : base()
        {
        }

        public RolPersonalizado(string roleName) : base(roleName)
        {
        }

        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}

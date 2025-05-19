using Microsoft.AspNetCore.Identity;
using Inventario.Core.Domain.Common;

namespace Inventario.Core.Domain.Entities
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
    }
}

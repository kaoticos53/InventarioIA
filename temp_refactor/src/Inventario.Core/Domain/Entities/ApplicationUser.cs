using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Inventario.Core.Domain.Common;

namespace Inventario.Core.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime? UltimoAcceso { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        // Propiedad de navegación para los roles del usuario
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    }
}

using System.Security.Claims;
using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        int GetTokenExpirationMinutes();
    }
}

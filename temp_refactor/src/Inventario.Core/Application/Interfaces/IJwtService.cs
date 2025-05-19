using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(string userId, string userName, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

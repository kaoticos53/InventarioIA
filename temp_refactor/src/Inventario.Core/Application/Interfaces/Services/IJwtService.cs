using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Inventario.Core.Domain.Entities;

namespace Inventario.Core.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<bool> VerifyEmailConfirmationTokenAsync(ApplicationUser user, string token);
        string GetUserIdFromToken(string token);
    }
}

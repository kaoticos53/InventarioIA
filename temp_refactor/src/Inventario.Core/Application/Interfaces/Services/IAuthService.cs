using Inventario.Core.Application.DTOs;
using Inventario.Core.Application.DTOs.AuthDtos;
using Inventario.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeTokenAsync(string token);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    }
}

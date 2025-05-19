using Inventario.Core.DTOs;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
        Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, string userId);
        Task<bool> ResetPasswordAsync(string email);
    }
}

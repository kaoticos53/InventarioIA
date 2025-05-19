using Inventario.Web.Models;
using System;
using System.Threading.Tasks;

namespace Inventario.Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginModel loginModel);
        Task<AuthResponse> RegisterAsync(RegisterModel registerModel);
        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordModel model);
        Task<AuthResponse> SendVerificationEmailAsync(string email);
        Task<AuthResponse> VerifyEmailAsync(EmailVerificationModel model);
        Task<bool> IsEmailVerifiedAsync();
        Task<bool> IsAuthenticatedAsync();
        Task LogoutAsync();
        Task<UserSession> GetUserInfoAsync();
    }


}

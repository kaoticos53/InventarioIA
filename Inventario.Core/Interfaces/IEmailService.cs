using System.Threading.Tasks;

namespace Inventario.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendPasswordResetEmailAsync(string email, string resetToken, string appBaseUrl);
        Task SendEmailConfirmationEmailAsync(string email, string confirmationToken, string appBaseUrl);
        Task SendAccountLockedEmailAsync(string email, string userName, int lockoutDurationMinutes);
        Task SendPasswordChangedEmailAsync(string email, string userName);
        Task SendRegistrationConfirmationEmailAsync(string email, string userName, string callbackUrl);
    }
}

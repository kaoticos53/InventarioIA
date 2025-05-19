using Inventario.Core.DTOs;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto emailDto);
        Task SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl);
        Task SendAccountLockedEmailAsync(string email, int lockoutEndInMinutes);
    }
}

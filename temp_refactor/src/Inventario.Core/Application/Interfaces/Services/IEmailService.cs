using Inventario.Core.Application.DTOs;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto emailRequest);
    }
}

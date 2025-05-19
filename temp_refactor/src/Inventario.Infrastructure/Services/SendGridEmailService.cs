using System;
using System.Threading.Tasks;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Inventario.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _sendGridApiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _sendGridApiKey = _configuration["SendGrid:ApiKey"] 
                ?? throw new ArgumentNullException("SendGrid:ApiKey no está configurado");
            _fromEmail = _configuration["SendGrid:FromEmail"] 
                ?? throw new ArgumentNullException("SendGrid:FromEmail no está configurado");
            _fromName = _configuration["SendGrid:FromName"] ?? "Sistema de Inventario";
        }

        public async Task SendEmailAsync(EmailDto emailRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_sendGridApiKey))
                {
                    _logger.LogError("No se ha configurado la API Key de SendGrid");
                    return;
                }

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(emailRequest.ToEmail);
                
                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    emailRequest.Subject,
                    emailRequest.IsHtml ? null : emailRequest.Body,
                    emailRequest.IsHtml ? emailRequest.Body : null);

                var response = await client.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"Error al enviar correo: {response.StatusCode} - {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo electrónico");
                throw; // Re-lanzar para que el llamador pueda manejar el error si es necesario
            }
        }
    }
}

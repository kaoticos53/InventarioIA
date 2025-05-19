using System;
using System.IO;
using System.Threading.Tasks;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Inventario.Infrastructure.Services
{
    public class DevelopmentEmailService : IEmailService
    {
        private readonly ILogger<DevelopmentEmailService> _logger;
        private readonly string _emailsDirectory;

        public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
        {
            _logger = logger;
            _emailsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Emails");
            
            // Asegurarse de que el directorio existe
            if (!Directory.Exists(_emailsDirectory))
            {
                Directory.CreateDirectory(_emailsDirectory);
            }
        }

        public async Task SendEmailAsync(EmailDto emailRequest)
        {
            try
            {
                var emailContent = $"""
                ============================================
                To: {emailRequest.ToEmail}
                Subject: {emailRequest.Subject}
                IsHtml: {emailRequest.IsHtml}
                --------------------------------------------
                {emailRequest.Body}
                ============================================
                """;

                // Registrar en la consola
                _logger.LogInformation($"Email enviado a {emailRequest.ToEmail}\n{emailContent}");

                // Guardar en un archivo
                var fileName = $"email_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.txt";
                var filePath = Path.Combine(_emailsDirectory, fileName);
                
                await File.WriteAllTextAsync(filePath, emailContent);
                _logger.LogInformation($"Email guardado en: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el correo electrónico");
                throw;
            }
        }
    }
}

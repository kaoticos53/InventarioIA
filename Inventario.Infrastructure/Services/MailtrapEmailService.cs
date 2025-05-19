using System;
using System.Net;
using System.Threading.Tasks;
using Inventario.Core;
using Inventario.Core.Interfaces;
using Inventario.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Inventario.Infrastructure.Services
{
    public class MailtrapEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<MailtrapEmailService> _logger;

        public MailtrapEmailService(
            IOptions<EmailSettings> emailSettings, 
            ILogger<MailtrapEmailService> logger)
        {
            _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Validar configuración
            if (string.IsNullOrEmpty(_emailSettings.SmtpServer) || 
                _emailSettings.SmtpPort <= 0 || 
                string.IsNullOrEmpty(_emailSettings.SmtpUsername) || 
                string.IsNullOrEmpty(_emailSettings.SmtpPassword))
            {
                _logger.LogWarning("La configuración de correo electrónico no está completa. Verifica la configuración en appsettings.json");
            }
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentException("La dirección de correo electrónico del destinatario es requerida", nameof(to));
                
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("El asunto del correo es requerido", nameof(subject));
                
            if (string.IsNullOrEmpty(htmlMessage))
                throw new ArgumentException("El contenido del mensaje es requerido", nameof(htmlMessage));
                
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_emailSettings.FromEmail ?? "noreply@inventario.com"));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new SmtpClient();
                
                // Configuración para aceptar certificados autofirmados (solo para desarrollo)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                await smtp.ConnectAsync(
                    _emailSettings.SmtpServer, 
                    _emailSettings.SmtpPort, 
                    SecureSocketOptions.StartTlsWhenAvailable);
                
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername) && 
                    !string.IsNullOrEmpty(_emailSettings.SmtpPassword))
                {
                    await smtp.AuthenticateAsync(
                        new NetworkCredential(
                            _emailSettings.SmtpUsername, 
                            _emailSettings.SmtpPassword));
                }
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation($"Correo enviado exitosamente a {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo electrónico");
                throw new ApplicationException("No se pudo enviar el correo electrónico. Por favor, inténtalo de nuevo más tarde.", ex);
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = "¡Bienvenido a nuestra plataforma!";
            var message = @$"
                <h1>¡Bienvenido, {WebUtility.HtmlEncode(userName)}!</h1>
                <p>Gracias por registrarte en nuestra plataforma. Esperamos que disfrutes de nuestros servicios.</p>
                <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                <p>¡Gracias!</p>
                <p>El equipo de soporte</p>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken, string appBaseUrl)
        {
            var resetUrl = $"{appBaseUrl.TrimEnd('/')}/reset-password?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(resetToken)}";
            var subject = "Restablecer tu contraseña";
            var message = @$"
                <h1>Restablecer contraseña</h1>
                <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                <p>Por favor, haz clic en el siguiente enlace para continuar:</p>
                <p><a href='{resetUrl}'>{resetUrl}</a></p>
                <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                <p>Este enlace expirará en 24 horas.</p>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendEmailConfirmationEmailAsync(string email, string confirmationToken, string appBaseUrl)
        {
            var confirmationUrl = $"{appBaseUrl.TrimEnd('/')}/confirm-email?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(confirmationToken)}";
            var subject = "Confirma tu correo electrónico";
            var message = @$"
                <h1>Confirma tu correo electrónico</h1>
                <p>Gracias por registrarte. Por favor, confirma tu dirección de correo electrónico haciendo clic en el siguiente enlace:</p>
                <p><a href='{confirmationUrl}'>{confirmationUrl}</a></p>
                <p>Si no creaste una cuenta, puedes ignorar este correo.</p>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendAccountLockedEmailAsync(string email, string userName, int lockoutDurationMinutes)
        {
            var subject = "Tu cuenta ha sido bloqueada temporalmente";
            var message = @$"
                <h1>Cuenta bloqueada temporalmente</h1>
                <p>Hola {WebUtility.HtmlEncode(userName)},</p>
                <p>Tu cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos de inicio de sesión.</p>
                <p>La cuenta estará bloqueada durante {lockoutDurationMinutes} minutos. Después de este tiempo, podrás intentar iniciar sesión nuevamente.</p>
                <p>Si no fuiste tú quien intentó iniciar sesión, te recomendamos cambiar tu contraseña lo antes posible.</p>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordChangedEmailAsync(string email, string userName)
        {
            var subject = "Tu contraseña ha sido cambiada";
            var message = @$"
                <h1>Contraseña actualizada</h1>
                <p>Hola {WebUtility.HtmlEncode(userName)},</p>
                <p>Tu contraseña ha sido cambiada exitosamente.</p>
                <p>Si no realizaste este cambio, por favor contacta a soporte de inmediato.</p>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendRegistrationConfirmationEmailAsync(string email, string userName, string callbackUrl)
        {
            var subject = "Confirma tu registro";
            var message = @$"
                <h1>¡Bienvenido, {WebUtility.HtmlEncode(userName)}!</h1>
                <p>Gracias por registrarte en nuestra plataforma. Por favor, confirma tu cuenta haciendo clic en el siguiente enlace:</p>
                <p><a href='{callbackUrl}'>Confirmar mi cuenta</a></p>
                <p>O copia y pega la siguiente URL en tu navegador:</p>
                <p>{callbackUrl}</p>
                <p>Si no te registraste en nuestro sitio, puedes ignorar este correo.</p>";

            await SendEmailAsync(email, subject, message);
        }
    }
}

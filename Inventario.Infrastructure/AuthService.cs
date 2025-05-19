using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Inventario.Core;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.Infrastructure
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<AuthService> logger,
            IEmailService emailService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            // Verificar si el usuario existe
            if (user == null)
            {
                _logger.LogWarning("Intento de inicio de sesión fallido para el usuario: {Email}", request.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Verificar si la cuenta está bloqueada
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Intento de inicio de sesión para cuenta bloqueada: {Email}", request.Email);
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingTime = lockoutEnd?.Subtract(DateTimeOffset.UtcNow) ?? TimeSpan.Zero;
                
                await _emailService.SendAccountLockedEmailAsync(
                    user.Email, 
                    user.UserName, 
                    (int)Math.Ceiling(remainingTime.TotalMinutes));
                
                throw new UnauthorizedAccessException("Cuenta bloqueada temporalmente. Por favor, intente más tarde.");
            }

            // Verificar credenciales
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                // Incrementar el contador de intentos fallidos
                await _userManager.AccessFailedAsync(user);
                
                // Obtener el número de intentos restantes
                var accessFailedCount = await _userManager.GetAccessFailedCountAsync(user);
                var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                var remainingAttempts = maxAttempts - accessFailedCount;
                
                _logger.LogWarning("Credenciales inválidas para el usuario: {Email}, intentos restantes: {RemainingAttempts}", 
                    request.Email, remainingAttempts);
                
                // Si se agotaron los intentos, bloquear la cuenta
                if (remainingAttempts <= 0)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(
                        _userManager.Options.Lockout.DefaultLockoutTimeSpan));
                    
                    _logger.LogWarning("Cuenta bloqueada por exceso de intentos fallidos: {Email}", request.Email);
                    
                    // Enviar correo de notificación de bloqueo
                    await _emailService.SendAccountLockedEmailAsync(
                        user.Email, 
                        user.UserName, 
                        (int)_userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes);
                        
                    throw new UnauthorizedAccessException("Cuenta bloqueada temporalmente por exceso de intentos fallidos. Por favor, intente más tarde o restablezca su contraseña.");
                }
                
                throw new UnauthorizedAccessException($"Credenciales inválidas. Le quedan {remainingAttempts} intentos antes de que su cuenta sea bloqueada temporalmente.");
            }

            // Verificar si el correo está confirmado
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Intento de inicio de sesión con correo no confirmado: {Email}", request.Email);
                throw new UnauthorizedAccessException("Por favor, confirme su dirección de correo electrónico antes de iniciar sesión.");
            }

            // Verificar si la cuenta está activa
            if (!user.Activo)
            {
                _logger.LogWarning("Intento de inicio de sesión en cuenta inactiva: {Email}", request.Email);
                throw new UnauthorizedAccessException("Su cuenta está desactivada. Por favor, contacte al administrador.");
            }

            // Si todo está bien, restablecer el contador de intentos fallidos
            if (await _userManager.GetAccessFailedCountAsync(user) > 0)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Actualizar último acceso
            user.UltimoAcceso = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Error al actualizar la información de último acceso para el usuario: {Email}", user.Email);
            }

            // Generar token JWT
            return await GenerateJwtToken(user);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Activo = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new Exception($"Error al crear el usuario: {string.Join(", ", result.Errors)}");
            }

            return await GenerateJwtToken(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
        {
            // Implementar lógica de refresco de token
            throw new NotImplementedException();
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            // Implementar lógica de revocación de token
            throw new NotImplementedException();
        }

        private async Task<AuthResponse> GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);
            var tokenLifetime = TimeSpan.FromMinutes(Convert.ToDouble(_configuration["JwtSettings:TokenLifetimeMinutes"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.GivenName, $"{user.Nombre} {user.Apellido}".Trim())
            };

            // Agregar roles del usuario a los claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(tokenLifetime),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new AuthResponse
            {
                Token = tokenString,
                Expiration = token.ValidTo,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.Nombre} {user.Apellido}".Trim(),
                Roles = roles
            };
        }
    }
}

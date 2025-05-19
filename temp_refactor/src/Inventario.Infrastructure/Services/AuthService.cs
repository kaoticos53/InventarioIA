using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventario.Core.Application.DTOs.AuthDtos;
using Inventario.Core.Application.Interfaces.Services;
using Inventario.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Inventario.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Usuario o contraseña incorrectos" };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponse { Success = false, Message = "Por favor confirme su correo electrónico antes de iniciar sesión" };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return new AuthResponse { Success = false, Message = "Usuario o contraseña incorrectos" };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateJwtToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Guardar el refresh token (en una aplicación real, deberías guardarlo en la base de datos)
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse { Success = false, Message = "El correo electrónico ya está registrado" };
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                EmailConfirmed = false // El correo debe ser confirmado
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "Error al crear el usuario",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Asignar rol predeterminado (si lo hay)
            if (!string.IsNullOrEmpty(request.Role))
            {
                await _userManager.AddToRoleAsync(user, request.Role);
            }
            else
            {
                // Rol predeterminado si no se especifica
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Generar token de confirmación de correo
            var token = await _jwtService.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"https://tuapp.com/confirm-email?userId={user.Id}&token={token}";

            // Enviar correo de confirmación (implementación simulada)
            await _emailService.SendEmailAsync(new EmailDto
            {
                ToEmail = user.Email,
                Subject = "Confirma tu correo electrónico",
                Body = $"Por favor confirma tu correo electrónico haciendo clic en el siguiente enlace: {confirmationLink}"
            });

            return new AuthResponse
            {
                Success = true,
                Message = "Usuario registrado exitosamente. Por favor revisa tu correo electrónico para confirmar tu cuenta.",
                UserId = user.Id,
                Email = user.Email
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst("id")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return new AuthResponse { Success = false, Message = "Token inválido" };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResponse { Success = false, Message = "Token de refresco inválido o expirado" };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateJwtToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Actualizar el refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var userId = _jwtService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Invalidar el refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // No revelar que el usuario no existe o no está confirmado
                return;
            }


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://tuapp.com/reset-password?email={email}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmailAsync(new EmailDto
            {
                ToEmail = email,
                Subject = "Restablecer contraseña",
                Body = $"Para restablecer tu contraseña, haz clic en el siguiente enlace: {resetLink}"
            });
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}

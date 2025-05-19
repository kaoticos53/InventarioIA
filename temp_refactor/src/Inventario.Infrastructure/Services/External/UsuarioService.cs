using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using AutoMapper;
using Inventario.Core;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventario.Infrastructure
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<RolPersonalizado> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UsuarioService(
            UserManager<ApplicationUser> userManager,
            RoleManager<RolPersonalizado> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<UsuarioService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #region Usuarios

        public async Task<UsuarioDto> GetUsuarioByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");
            }

            // Cargar los roles del usuario
            var userRoles = await _userManager.GetRolesAsync(user);

            var userDto = _mapper.Map<UsuarioDto>(user);
            userDto.Roles = await _userManager.GetRolesAsync(user);
            return userDto;
        }

        public async Task<UsuarioDto> GetUsuarioByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con email {email}");
            }

            var userDto = _mapper.Map<UsuarioDto>(user);
            userDto.Roles = await _userManager.GetRolesAsync(user);
            return userDto;
        }

        public async Task<UsuarioPaginacionDto> GetUsuariosAsync(UsuarioFiltroDto filtro)
        {
            // Primero obtenemos los usuarios
            var query = _userManager.Users.AsQueryable();
            
            // Aplicamos los filtros
            if (!string.IsNullOrEmpty(filtro.Busqueda))
            {
                var busqueda = filtro.Busqueda.ToLower();
                query = query.Where(u => 
                    u.Nombre.ToLower().Contains(busqueda) ||
                    u.Apellido.ToLower().Contains(busqueda) ||
                    u.Email.ToLower().Contains(busqueda) ||
                    u.UserName.ToLower().Contains(busqueda));
            }

            if (filtro.Activo.HasValue)
            {
                query = query.Where(u => u.Activo == filtro.Activo.Value);
            }

            if (!string.IsNullOrEmpty(filtro.Rol))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(filtro.Rol);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }

            // Ordenar
            query = query.OrderBy(u => u.Apellido).ThenBy(u => u.Nombre);

            // Paginación
            var total = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(total / (double)filtro.TamanioPagina);
            
            var usuarios = await query
                .Skip((filtro.Pagina - 1) * filtro.TamanioPagina)
                .Take(filtro.TamanioPagina)
                .ToListAsync();

            // Mapear a DTOs
            var usuariosDto = new List<UsuarioDto>();
            foreach (var usuario in usuarios)
            {
                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                usuarioDto.Roles = await _userManager.GetRolesAsync(usuario);
                usuariosDto.Add(usuarioDto);
            }

            return new UsuarioPaginacionDto
            {
                Usuarios = usuariosDto,
                Total = total,
                Pagina = filtro.Pagina,
                TamanioPagina = filtro.TamanioPagina,
                TotalPaginas = totalPaginas
            };
        }

        public async Task<IdentityResult> CreateUsuarioAsync(CreateUsuarioDto usuarioDto)
        {
            try
            {
                // Verificar si el correo ya está en uso
                var existingUser = await _userManager.FindByEmailAsync(usuarioDto.Email);
                if (existingUser != null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateEmail",
                        Description = "El correo electrónico ya está en uso por otro usuario."
                    });
                }

                // Mapear DTO a entidad ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = usuarioDto.Email,
                    Email = usuarioDto.Email,
                    Nombre = usuarioDto.Nombre,
                    Apellido = usuarioDto.Apellido,
                    Activo = true,
                    EmailConfirmed = false // El correo no está confirmado inicialmente
                };

                // Crear el usuario
                var result = await _userManager.CreateAsync(user, usuarioDto.Password);

                if (result.Succeeded)
                {
                    // Asignar roles si se especificaron
                    if (usuarioDto.Roles != null && usuarioDto.Roles.Any())
                    {
                        foreach (var roleName in usuarioDto.Roles)
                        {
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                                if (roleExists)
                                {
                                    await _userManager.AddToRoleAsync(user, roleName);
                                }
                            }
                        }
                    }

                    // Generar token de confirmación de correo
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    
                    // Construir URL de confirmación
                    var baseUrl = _configuration["WebApp:BaseUrl"] ?? _httpContextAccessor.HttpContext?.Request.Scheme + "://" + 
                                _httpContextAccessor.HttpContext?.Request.Host;
                    var callbackUrl = $"{baseUrl}/confirmar-email?userId={user.Id}&token={encodedToken}";
                    
                    // Enviar correo de confirmación
                    await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.UserName, callbackUrl);
                    
                    // Enviar correo de bienvenida
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        public async Task<IdentityResult> UpdateUsuarioAsync(string id, UpdateUsuarioDto usuarioDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            // Actualizar propiedades básicas
            if (!string.IsNullOrEmpty(usuarioDto.Nombre)) user.Nombre = usuarioDto.Nombre;
            if (!string.IsNullOrEmpty(usuarioDto.Apellido)) user.Apellido = usuarioDto.Apellido;
            if (usuarioDto.Telefono != null) user.PhoneNumber = usuarioDto.Telefono;
            if (usuarioDto.Activo.HasValue) user.Activo = usuarioDto.Activo.Value;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }

            // Actualizar roles si se especificaron
            if (usuarioDto.Roles != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var rolesToAdd = usuarioDto.Roles.Except(userRoles);
                var rolesToRemove = userRoles.Except(usuarioDto.Roles);

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }
            }

            return result;
        }

        public async Task<IdentityResult> DeleteUsuarioAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            // Verificar si el usuario es el último administrador
            if (await _userManager.IsInRoleAsync(user, "Administrador"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Administrador");
                if (adminUsers.Count <= 1)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "LastAdmin",
                        Description = "No se puede eliminar al último administrador."
                    });
                }
            }

            // No eliminar físicamente, marcar como inactivo
            user.Activo = false;
            user.Email = $"{user.Email}_deleted_{DateTime.UtcNow.Ticks}";
            user.NormalizedEmail = user.Email.ToUpper();
            user.UserName = user.Email;
            user.NormalizedUserName = user.Email.ToUpper();

            return await _userManager.UpdateAsync(user);
        }

        #endregion

        #region Roles

        public async Task<IList<string>> GetRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}");
            }

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AsignarRolAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleNotFound",
                    Description = "El rol especificado no existe."
                });
            }

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> QuitarRolAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            // Verificar si el usuario es el último administrador
            if (roleName == "Administrador")
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Administrador");
                if (adminUsers.Count <= 1 && adminUsers.Any(u => u.Id == userId))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "LastAdmin",
                        Description = "No se puede quitar el rol de administrador al último administrador."
                    });
                }
            }

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}");
            }

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        #endregion

        #region Contraseñas

        public async Task<IdentityResult> CambiarContrasenaAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de cambio de contraseña para usuario no encontrado con ID: {UserId}", userId);
                    return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                
                if (result.Succeeded)
                {
                    // Enviar notificación de cambio de contraseña
                    await _emailService.SendPasswordChangedEmailAsync(user.Email, user.UserName);
                    _logger.LogInformation("Contraseña cambiada exitosamente para el usuario: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Error al cambiar la contraseña para el usuario: {Email}. Errores: {Errors}", 
                        user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña para el usuario con ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GenerarPasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con email {email}");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        public async Task<IdentityResult> ResetearContrasenaAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Intento de restablecer contraseña para usuario no encontrado: {Email}", email);
                    return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });
                }

                // Decodificar el token
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                
                if (result.Succeeded)
                {
                    // Si la cuenta estaba bloqueada, desbloquearla
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        await _userManager.SetLockoutEndDateAsync(user, null);
                        await _userManager.ResetAccessFailedCountAsync(user);
                    }
                    
                    _logger.LogInformation("Contraseña restablecida exitosamente para el usuario: {Email}", email);
                    
                    // Enviar notificación por correo
                    await _emailService.SendPasswordChangedEmailAsync(email, user.UserName);
                }
                else
                {
                    _logger.LogWarning("Error al restablecer la contraseña para el usuario: {Email}. Errores: {Errors}", 
                        email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña para el usuario: {Email}", email);
                throw;
            }
        }

        #endregion

        #region Bloqueo/Desbloqueo

        public async Task<IdentityResult> BloquearUsuarioAsync(string userId, DateTime? fechaFinBloqueo = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            // No permitir bloquear al propio usuario
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (user.Id == currentUserId)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "SelfLockout",
                    Description = "No puede bloquear su propia cuenta."
                });
            }

            // No permitir bloquear a otros administradores
            if (await _userManager.IsInRoleAsync(user, "Administrador"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Administrador");
                if (adminUsers.Count <= 1)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "LastAdmin",
                        Description = "No se puede bloquear al último administrador."
                    });
                }
            }

            user.LockoutEnd = fechaFinBloqueo ?? DateTimeOffset.MaxValue;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DesbloquearUsuarioAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Usuario no encontrado."
                });
            }

            user.LockoutEnd = null;
            return await _userManager.UpdateAsync(user);
        }

        #endregion

        #region Confirmación de correo

        public async Task<string> GenerarEmailConfirmationTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con email {email}");
            }

            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmarEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de confirmación de correo para usuario no encontrado con ID: {UserId}", userId);
                    return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });
                }

                // Decodificar el token
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                
                if (result.Succeeded)
                {
                    // Enviar correo de bienvenida después de confirmar el correo
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);
                    _logger.LogInformation("Correo electrónico confirmado exitosamente para el usuario: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Error al confirmar el correo electrónico para el usuario: {Email}. Errores: {Errors}", 
                        user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar el correo electrónico para el usuario con ID: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Autenticación

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Verificar si el usuario está activo
            if (!user.Activo)
            {
                throw new UnauthorizedAccessException("La cuenta está desactivada. Contacte al administrador.");
            }

            // Verificar si el correo está confirmado
            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("Por favor confirme su correo electrónico antes de iniciar sesión.");
            }

            // Verificar contraseña
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    throw new UnauthorizedAccessException("La cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos. Por favor, intente más tarde.");
                }
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Generar tokens
            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Guardar el refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 días de validez
            await _userManager.UpdateAsync(user);

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes()),
                UserId = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenException("Token inválido");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Refresh token inválido o expirado");
            }

            // Generar nuevo token
            var newToken = _jwtService.GenerateJwtToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Actualizar refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes()),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.Nombre} {user.Apellido}".Trim(),
                Nombre = user.Nombre ?? string.Empty,
                Apellido = user.Apellido ?? string.Empty,
                Roles = roles?.ToList() ?? new List<string>()
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
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

        #endregion
    }
}

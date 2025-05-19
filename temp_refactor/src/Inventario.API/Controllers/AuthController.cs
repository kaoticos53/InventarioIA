using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Inventario.Core;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Inventario.Infrastructure.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Inventario.Core.Interfaces;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            IAuthService authService, 
            IUsuarioService usuarioService,
            ILogger<AuthController> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Error de autenticación para el usuario {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión para el usuario {Email}", request.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                
                // Obtener el usuario recién creado
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    // Generar token de confirmación
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    
                    // Construir URL de confirmación
                    var baseUrl = _configuration["WebApp:BaseUrl"];
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        var scheme = _httpContextAccessor.HttpContext?.Request.Scheme ?? "https";
                        var host = _httpContextAccessor.HttpContext?.Request.Host.Value ?? "localhost";
                        baseUrl = $"{scheme}://{host}";
                    }
                    var callbackUrl = $"{baseUrl}/confirmar-email?userId={user.Id}&token={encodedToken}";
                    
                    // Validar que el correo no sea nulo antes de enviar
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogError("El correo electrónico del usuario no puede ser nulo");
                        return BadRequest(new { message = "Error al registrar el usuario: El correo electrónico no puede estar vacío" });
                    }

                    var userName = user.UserName ?? "Usuario";
                    
                    // Enviar correo de confirmación
                    await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, userName, callbackUrl);
                    
                    // También enviar correo de bienvenida
                    await _emailService.SendWelcomeEmailAsync(user.Email, userName);
                }
                
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error al registrar usuario: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud");
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar el token");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            try
            {
                await _authService.RevokeTokenAsync(token);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar el token");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        [HttpPost("cambiar-contrasena")]
        [Authorize]
        public async Task<IActionResult> CambiarContrasena([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var result = await _usuarioService.CambiarContrasenaAsync(userId, request.CurrentPassword, request.NewPassword);
                
                if (result.Succeeded)
                {
                    // Obtener el usuario para enviar notificación
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var userName = user.UserName ?? "Usuario";
                        await _emailService.SendPasswordChangedEmailAsync(user.Email, userName);
                    }
                    
                    return Ok("Contraseña cambiada exitosamente");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña");
                return StatusCode(500, "Ocurrió un error al cambiar la contraseña");
            }
        }

        /// <summary>
        /// Solicita un restablecimiento de contraseña
        /// </summary>
        /// <summary>
        /// Confirma el correo electrónico de un usuario
        /// </summary>
        [HttpGet("confirmar-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Se requieren userId y token para confirmar el correo electrónico");
            }

            try
            {
                // Decodificar el token
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                
                // Buscar al usuario
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Confirmar el correo electrónico
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al confirmar el correo electrónico para el usuario {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest("No se pudo confirmar el correo electrónico. El token puede haber expirado o ser inválido.");
                }

                _logger.LogInformation("Correo electrónico confirmado exitosamente para el usuario {UserId}", userId);
                
                // Redirigir a la página de confirmación exitosa en el frontend
                var frontendUrl = _configuration["WebApp:BaseUrl"];
                if (string.IsNullOrEmpty(frontendUrl))
                {
                    frontendUrl = "https://localhost:5001";
                }
                return Redirect($"{frontendUrl}/cuenta/confirmacion-exitosa");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar el correo electrónico para el usuario {UserId}", userId);
                return StatusCode(500, "Ocurrió un error al confirmar el correo electrónico");
            }
        }

        /// <summary>
        /// Reenvía el correo de verificación
        /// </summary>
        [HttpPost("reenviar-verificacion")]
        [AllowAnonymous]
        public async Task<IActionResult> ReenviarCorreoVerificacion([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("El correo electrónico es requerido");
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // No revelar que el usuario no existe por razones de seguridad
                    return Ok("Si el correo electrónico está registrado, recibirás un enlace de verificación.");
                }

                if (user.EmailConfirmed)
                {
                    return BadRequest("El correo electrónico ya ha sido verificado.");
                }

                // Generar token de confirmación
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                
                // Construir URL de confirmación
                var baseUrl = _configuration["WebApp:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    var scheme = _httpContextAccessor.HttpContext?.Request.Scheme ?? "https";
                    var host = _httpContextAccessor.HttpContext?.Request.Host.Value ?? "localhost";
                    baseUrl = $"{scheme}://{host}";
                }
                var callbackUrl = $"{baseUrl}/confirmar-email?userId={user.Id}&token={encodedToken}";
                
                // Validar que el correo no sea nulo antes de enviar
                if (string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogError("El correo electrónico del usuario no puede ser nulo");
                    return BadRequest(new { message = "El correo electrónico no puede estar vacío" });
                }
                
                var userName = user.UserName ?? "Usuario";
                
                // Enviar correo de confirmación
                await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, userName, callbackUrl);
                
                _logger.LogInformation("Correo de verificación reenviado a {Email}", email);
                
                return Ok("Se ha enviado un nuevo correo de verificación a tu dirección de correo electrónico.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar el correo de verificación a {Email}", email);
                return StatusCode(500, "Ocurrió un error al procesar la solicitud");
            }
        }

        [HttpPost("olvide-contrasena")]
        [AllowAnonymous]
        public async Task<IActionResult> OlvideContrasena([FromBody] ResetearContrasenaDto resetearContrasenaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Generar token de restablecimiento
                var token = await _usuarioService.GenerarPasswordResetTokenAsync(resetearContrasenaDto.Email);
                
                // En un entorno real, aquí enviarías un correo electrónico con el token
                // Para este ejemplo, simplemente lo devolvemos en la respuesta
                // En producción, NUNCA hagas esto
                return Ok(new { 
                    message = "Si existe una cuenta con este correo, se ha enviado un enlace para restablecer la contraseña",
                    // Solo para desarrollo
                    token = token 
                });
            }
            catch (KeyNotFoundException)
            {
                // No revelamos si el correo existe o no por seguridad
                return Ok(new { message = "Si existe una cuenta con este correo, se ha enviado un enlace para restablecer la contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de restablecimiento de contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Restablece la contraseña con el token proporcionado
        /// </summary>
        [HttpPost("restablecer-contrasena")]
        [AllowAnonymous]
        public async Task<IActionResult> RestablecerContrasena([FromBody] ConfirmarResetearContrasenaDto confirmarResetearContrasenaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _usuarioService.ResetearContrasenaAsync(
                    confirmarResetearContrasenaDto.Email,
                    confirmarResetearContrasenaDto.Token,
                    confirmarResetearContrasenaDto.NuevaContrasena);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al restablecer la contraseña" });
            }
        }

        /// <summary>
        /// Confirma el correo electrónico del usuario
        /// </summary>
        [HttpPost("confirmar-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailDto confirmarEmailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _usuarioService.ConfirmarEmailAsync(confirmarEmailDto.UsuarioId, confirmarEmailDto.Token);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return Ok(new { message = "Correo electrónico confirmado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar el correo electrónico");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al confirmar el correo electrónico" });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> ObtenerPerfil()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(userId);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado");
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al obtener el perfil" });
            }
        }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ConfirmarEmailDto
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

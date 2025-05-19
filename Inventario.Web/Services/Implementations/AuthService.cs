using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Inventario.Core;
using Inventario.Core.Interfaces;
using Inventario.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventario.Web.Services.Implementations
{
    public class AuthService : Inventario.Web.Services.Interfaces.IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
        private readonly ILogger<AuthService> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IEmailService _emailService;
        private readonly NavigationManager _navigationManager;
        private readonly string _authTokenKey = "authToken";
        private readonly string _refreshTokenKey = "refreshToken";
        private readonly string _tokenExpirationKey = "tokenExpiration";

        // Lista de dominios de correo electrónico permitidos para el registro
        private readonly string[] _allowedEmailDomains = { "gmail.com", "outlook.com", "hotmail.com", "yahoo.com", "inventario.com" };

        public AuthService(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider,
            Blazored.LocalStorage.ILocalStorageService localStorage,
            ILogger<AuthService> logger,
            IUsuarioService usuarioService,
            IEmailService emailService,
            NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
            _logger = logger;
            _usuarioService = usuarioService;
            _emailService = emailService;
            _navigationManager = navigationManager;
        }

        public async Task<Inventario.Web.Models.AuthResponse> LoginAsync(LoginModel loginModel)
        {
            try
            {
                // En un entorno real, esto haría una llamada a tu API
                // var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);
                // var result = await response.Content.ReadFromJsonAsync<Inventario.Web.Models.AuthResponse>();

                // Simulación de respuesta exitosa
                if (loginModel.Email == "admin@inventario.com" && loginModel.Password == "Admin123!")
                {
                    var result = new Inventario.Web.Models.AuthResponse
                    {
                        IsSuccess = true,
                        Token = "simulated_jwt_token",
                        RefreshToken = "simulated_refresh_token",
                        Expiration = DateTime.UtcNow.AddHours(1),
                        Message = "Inicio de sesión exitoso"
                    };

                    await StoreAuthTokens(result);
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                    
                    return result;
                }

                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Credenciales inválidas" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Error al conectar con el servidor. Por favor, intente más tarde." 
                };
            }
        }

        public async Task<Inventario.Web.Models.AuthResponse> ForgotPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = "El correo electrónico es obligatorio." 
                    };
                }

                // Simulación de envío de correo de recuperación
                await Task.Delay(1000); // Simular latencia de red
                
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = true, 
                    Message = "Se ha enviado un correo con las instrucciones para restablecer tu contraseña." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar recuperación de contraseña");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Error al procesar la solicitud. Por favor, intente más tarde." 
                };
            }
        }

        public async Task<Inventario.Web.Models.AuthResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            try
            {
                if (model == null)
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = "El modelo no puede ser nulo." 
                    };
                }

                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = "Todos los campos son obligatorios." 
                    };
                }

                var result = await _usuarioService.ResetearContrasenaAsync(model.Email, model.Token, model.NewPassword);
                
                if (result.Succeeded)
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = true, 
                        Message = "Tu contraseña ha sido restablecida exitosamente. Ahora puedes iniciar sesión con tu nueva contraseña." 
                    };
                }
                else
                {
                    var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al restablecer la contraseña: {Errors}", errorMessage);
                    
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = $"No se pudo restablecer la contraseña: {errorMessage}" 
                    };
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado al intentar restablecer la contraseña");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "No se encontró una cuenta con ese correo electrónico." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Ocurrió un error al intentar restablecer la contraseña. Por favor, verifica que el enlace de restablecimiento sea válido y no haya expirado." 
                };
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(_authTokenKey);
            var expiration = await _localStorage.GetItemAsync<DateTime?>(_tokenExpirationKey);
            
            return !string.IsNullOrEmpty(token) && expiration.HasValue && expiration > DateTime.UtcNow;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(_authTokenKey);
            await _localStorage.RemoveItemAsync(_refreshTokenKey);
            await _localStorage.RemoveItemAsync(_tokenExpirationKey);
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            _navigationManager.NavigateTo("/login");
        }



        public async Task<Inventario.Web.Models.AuthResponse> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                // Validar que las contraseñas coincidan
                if (registerModel.Password != registerModel.ConfirmPassword)
                {
                    return new Inventario.Web.Models.AuthResponse { IsSuccess = false, Message = "Las contraseñas no coinciden" };
                }

                // Validar que se acepten los términos y condiciones
                if (!registerModel.AcceptTerms)
                {
                    return new Inventario.Web.Models.AuthResponse { IsSuccess = false, Message = "Debes aceptar los términos y condiciones" };
                }

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerModel);
                response.EnsureSuccessStatusCode();
                
                var authResponse = await response.Content.ReadFromJsonAsync<Inventario.Web.Models.AuthResponse>();
                
                if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.Token))
                {
                    await StoreAuthTokens(authResponse);
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResponse.Token);
                }
                
                return authResponse ?? new Inventario.Web.Models.AuthResponse { IsSuccess = false, Message = "Error al procesar la respuesta del servidor" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el usuario");
                return new Inventario.Web.Models.AuthResponse { IsSuccess = false, Message = "Error al registrar el usuario. Por favor, inténtalo de nuevo más tarde." };
            }
        }

        public async Task<Inventario.Web.Models.AuthResponse> SendVerificationEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new Inventario.Web.Models.AuthResponse { IsSuccess = false, Message = "El correo electrónico es obligatorio." };
                }

                // En una aplicación real, aquí se haría una llamada a la API para enviar el correo de verificación
                // var response = await _httpClient.PostAsJsonAsync("api/auth/send-verification-email", new { Email = email });
                // var result = await response.Content.ReadFromJsonAsync<Inventario.Web.Models.AuthResponse>();
                
                // Simulación de envío de correo
                await Task.Delay(1000);
                
                // En producción, el token sería generado por el servidor
                var token = GenerateVerificationToken(email);
                
                // Guardar el token en localStorage (solo para simulación)
                await _localStorage.SetItemAsync($"verification_token_{email}", token);
                
                // En producción, el correo se enviaría aquí
                _logger.LogInformation($"Correo de verificación enviado a {email}. Token: {token}");
                
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = true, 
                    Message = "Se ha enviado un correo de verificación a su dirección de correo electrónico.",
                    Token = token // Solo para pruebas, en producción no se devuelve el token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de verificación");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Ocurrió un error al enviar el correo de verificación. Por favor, inténtelo de nuevo más tarde." 
                };
            }
        }
        
        public async Task<Inventario.Web.Models.AuthResponse> VerifyEmailAsync(EmailVerificationModel model)
        {
            try
            {
                if (model == null)
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = "El modelo de verificación no puede ser nulo." 
                    };
                }

                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
                {
                    return new Inventario.Web.Models.AuthResponse 
                    { 
                        IsSuccess = false, 
                        Message = "El correo electrónico y el token son obligatorios." 
                    };
                }

                // En una aplicación real, aquí se haría una llamada a la API para verificar el token
                // var response = await _httpClient.PostAsJsonAsync("api/auth/verify-email", model);
                // response.EnsureSuccessStatusCode();
                // var result = await response.Content.ReadFromJsonAsync<Inventario.Web.Models.AuthResponse>();
                
                // Simulación de verificación exitosa
                await Task.Delay(500); // Simular latencia de red
                
                // Marcar el correo como verificado en localStorage (solo para simulación)
                await _localStorage.SetItemAsync($"email_verified_{model.Email}", true);
                
                _logger.LogInformation($"Correo electrónico verificado: {model.Email}");
                
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = true, 
                    Message = "¡Tu correo electrónico ha sido verificado exitosamente!" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el correo electrónico");
                return new Inventario.Web.Models.AuthResponse 
                { 
                    IsSuccess = false, 
                    Message = "Ocurrió un error al verificar el correo electrónico. Por favor, inténtelo de nuevo más tarde." 
                };
            }
        }
        
        public async Task<bool> IsEmailVerifiedAsync()
        {
            try
            {
                // Obtener el usuario autenticado actual
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return false;
                }
                
                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                
                // En una aplicación real, esto se verificaría contra la API
                // var response = await _httpClient.GetAsync($"api/auth/is-email-verified?email={email}");
                // return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
                
                // Simulación: verificar en localStorage
                return await _localStorage.GetItemAsync<bool>($"email_verified_{email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de verificación del correo electrónico");
                return false;
            }
        }
        
        private async Task StoreAuthTokens(Inventario.Web.Models.AuthResponse authResult)
        {
            await _localStorage.SetItemAsync(_authTokenKey, authResult.Token);
            if (!string.IsNullOrEmpty(authResult.RefreshToken))
            {
                await _localStorage.SetItemAsync(_refreshTokenKey, authResult.RefreshToken);
            }
            if (authResult.Expiration.HasValue)
            {
                await _localStorage.SetItemAsync(_tokenExpirationKey, authResult.Expiration.Value);
            }
        }
        
        public async Task<UserSession> GetUserInfoAsync()
        {
            try
            {
                var userSession = await _localStorage.GetItemAsync<UserSession>("userSession");
                return userSession ?? new UserSession();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la información del usuario");
                return new UserSession(); // Devolver un objeto vacío en lugar de null
            }
        }
        
        private string GenerateVerificationToken(string email)
        {
            // En producción, esto lo generaría el servidor
            // Esta es solo una simulación para fines de demostración
            var token = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("x");
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{token}"));
        }
    }
}

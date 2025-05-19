using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Inventario.Web.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private const string AuthTokenKey = "authToken";

        public CustomAuthStateProvider(
            ILocalStorageService localStorage,
            HttpClient httpClient,
            ILogger<CustomAuthStateProvider> logger)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var savedToken = await _localStorage.GetItemAsync<string>(AuthTokenKey);

                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // En una aplicación real, validaríamos el token JWT aquí
                // Por ahora, simplemente verificamos que existe
                var claims = ParseClaimsFromJwt(savedToken);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de autenticación");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            
            try
            {
                // Validar y leer el token sin validar la firma (en el cliente no podemos validar la firma)
                var token = tokenHandler.ReadJwtToken(jwt);
                
                // Agregar todos los claims del token
                claims.AddRange(token.Claims);
                
                // Asegurarse de que los claims estándar estén mapeados correctamente
                var name = token.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name" || c.Type == "sub")?.Value;
                var email = token.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "unique_name")?.Value;
                var roleClaims = token.Claims.Where(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                
                // Agregar claims estándar si no existen
                if (!string.IsNullOrEmpty(name) && !claims.Any(c => c.Type == ClaimTypes.Name))
                    claims.Add(new Claim(ClaimTypes.Name, name));
                    
                if (!string.IsNullOrEmpty(email) && !claims.Any(c => c.Type == ClaimTypes.Email))
                    claims.Add(new Claim(ClaimTypes.Email, email));
                    
                foreach (var roleClaim in roleClaims)
                {
                    if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleClaim.Value))
                        claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
                
                return claims;
            }
            catch (Exception ex)
            {
                // En caso de error al decodificar el token, devolver un claim vacío
                Console.WriteLine($"Error al decodificar el token JWT: {ex.Message}");
                return new List<Claim>();
            }
        }
    }
}

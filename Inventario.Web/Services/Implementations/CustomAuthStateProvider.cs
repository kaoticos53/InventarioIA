using Blazored.LocalStorage;
using Inventario.Web.Models;
using Inventario.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventario.Web.Services.Implementations
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Inventario.Web.Services.Interfaces.IAuthService _authService;
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(Inventario.Web.Services.Interfaces.IAuthService authService, Blazored.LocalStorage.ILocalStorageService localStorage, ILogger<CustomAuthStateProvider> logger)
        {
            _authService = authService;
            _localStorage = localStorage;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSession = await _authService.GetUserInfoAsync();
                
                if (userSession?.Email != null)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, userSession.UserName ?? userSession.Email),
                        new Claim(ClaimTypes.Email, userSession.Email),
                        new Claim(ClaimTypes.Role, userSession.Role ?? "User")
                    };

                    var identity = new ClaimsIdentity(claims, "Server authentication");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de autenticación");
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task UpdateAuthenticationState(UserSession? userSession)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userSession.UserName ?? userSession.Email ?? ""),
                    new Claim(ClaimTypes.Email, userSession.Email ?? ""),
                    new Claim(ClaimTypes.Role, userSession.Role ?? "User")
                };

                var identity = new ClaimsIdentity(claims, "Server authentication");
                claimsPrincipal = new ClaimsPrincipal(identity);
            }
            else
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public void NotifyUserAuthentication(string token)
        {
            // Este método se llama cuando un usuario inicia sesión exitosamente
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Usuario Autenticado"),
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            // Este método se llama cuando un usuario cierra sesión
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}

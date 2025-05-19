using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Inventario.Web.Services.Http;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private const string AuthTokenKey = "authToken";

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // No agregar el token a las solicitudes de autenticación
        if (!request.RequestUri.AbsolutePath.Contains("/api/auth/"))
        {
            var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

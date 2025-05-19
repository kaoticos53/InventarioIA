using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Inventario.Web.Services;
using Microsoft.Extensions.Logging;

namespace Inventario.Web.Services.Implementations
{
    public class ConfigurationService : IConfigurationService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConfigurationService> _logger;
        private bool _disposed = false;
        private object _cachedConfig;
        private Type _cachedType;
        private DateTime _lastLoaded = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public ConfigurationService(HttpClient httpClient, ILogger<ConfigurationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetConfigurationAsync<T>(string? key = null) where T : class, new()
        {
            try
            {
                // Verificar si la caché está vencida o si el tipo ha cambiado
                if (_cachedConfig == null || _cachedType != typeof(T) || DateTime.UtcNow - _lastLoaded > CacheDuration)
                {
                    _logger.LogInformation("Cargando configuración desde el servidor...");
                    
                    // Cargar configuración local primero
                    var localConfig = await LoadLocalConfigurationAsync<T>();
                    _cachedConfig = localConfig ?? new T(); // Asegurarse de que no sea nulo
                    _cachedType = typeof(T);
                    _lastLoaded = DateTime.UtcNow;
                    
                    // Intentar cargar configuración remota
                    try
                    {
                        var remoteConfig = await _httpClient.GetFromJsonAsync<T>("api/configuration");
                        if (remoteConfig != null)
                        {
                            _cachedConfig = MergeConfigurations((T)_cachedConfig, remoteConfig);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo cargar la configuración remota. Usando configuración local.");
                    }
                }

                if (string.IsNullOrEmpty(key))
                {
                    return _cachedConfig as T;
                }

                // Usar reflexión para obtener el valor de la propiedad anidada
                var current = _cachedConfig as object;
                foreach (var part in key.Split(':'))
                {
                    var property = current?.GetType().GetProperty(part);
                    if (property == null) return null;
                    current = property.GetValue(current, null);
                }
                return (T)current;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la configuración");
                return new T();
            }
        }

        public async Task<string> GetApiUrlAsync()
        {
            var config = await GetConfigurationAsync<AppSettings>();
            return config?.ApiUrl?.TrimEnd('/') ?? string.Empty;
        }

        private async Task<T> LoadLocalConfigurationAsync<T>() where T : class, new()
        {
            try
            {
                var response = await _httpClient.GetAsync("appsettings.json");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new T();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la configuración local");
                return new T();
            }
        }

        private T MergeConfigurations<T>(T primary, T secondary) where T : class, new()
        {
            if (primary == null) return secondary ?? new T();
            if (secondary == null) return primary;

            var properties = typeof(T).GetProperties();
            var result = new T();

            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    var primaryValue = property.GetValue(primary);
                    var secondaryValue = property.GetValue(secondary);
                    
                    // Si el valor primario es nulo o vacío, usar el valor secundario
                    if (primaryValue == null || (primaryValue is string str && string.IsNullOrEmpty(str)))
                    {
                        property.SetValue(result, secondaryValue);
                    }
                    // Si ambos son objetos, fusionar recursivamente
                    else if (primaryValue != null && secondaryValue != null && 
                             !property.PropertyType.IsValueType && 
                             property.PropertyType != typeof(string))
                    {
                        // Usar reflexión para llamar al método genérico con el tipo correcto
                        var mergeMethod = typeof(ConfigurationService).GetMethod("MergeConfigurations", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var genericMergeMethod = mergeMethod.MakeGenericMethod(property.PropertyType);
                        var merged = genericMergeMethod.Invoke(this, new object[] { primaryValue, secondaryValue });
                        property.SetValue(result, merged);
                    }
                    else
                    {
                        property.SetValue(result, primaryValue);
                    }
                }
            }
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }

    public class AppSettings
    {
        public string ApiUrl { get; set; } = "https://localhost:5001";
        public string AppName { get; set; } = "Sistema de Inventario";
        public string AppVersion { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";
        public AppFeatures Features { get; set; } = new AppFeatures();
    }

    public class AppFeatures
    {
        public bool EnableRegistration { get; set; } = true;
        public bool EnableEmailVerification { get; set; } = true;
        public bool EnablePasswordReset { get; set; } = true;
    }
}

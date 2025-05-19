using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Inventario.Core.Interfaces;
using Inventario.Core.Models;
using IAppSettingsService = Inventario.Core.Interfaces.IAppSettingsService;
using AppSettings = Inventario.Core.Models.AppSettings;

namespace Inventario.Infrastructure.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly HttpClient _httpClient;
        private const string BasePath = "api/appsettings";

        public AppSettingsService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<AppSettings>($"{BasePath}");
                return response ?? new AppSettings();
            }
            catch (Exception ex)
            {
                // Log the error (you might want to inject an ILogger)
                Console.WriteLine($"Error al obtener la configuración: {ex.Message}");
                return new AppSettings();
            }
        }

        public async Task<AppSettings> UpdateAppSettingsAsync(AppSettings settings, string updatedBy)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            try
            {
                // Actualizar la información de auditoría
                settings.UpdatedAt = DateTime.UtcNow;
                settings.UpdatedBy = updatedBy ?? "Sistema";
                
                var response = await _httpClient.PutAsJsonAsync($"{BasePath}", settings);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<AppSettings>();
                return result ?? throw new InvalidOperationException("No se pudo deserializar la respuesta del servidor.");
            }
            catch (Exception ex)
            {
                // Log the error (you might want to inject an ILogger)
                Console.WriteLine($"Error al actualizar la configuración: {ex.Message}");
                throw;
            }
        }
    }
}

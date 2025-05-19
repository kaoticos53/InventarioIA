using System.Threading.Tasks;

namespace Inventario.Web.Services
{
    public interface IConfigurationService
    {
        Task<T> GetConfigurationAsync<T>(string? key = null) where T : class, new();
        Task<string> GetApiUrlAsync();
    }
}

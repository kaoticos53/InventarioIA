using Inventario.Core.Models;
using System.Threading.Tasks;

namespace Inventario.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<AppSettings> GetAppSettingsAsync();
        Task<AppSettings> UpdateAppSettingsAsync(AppSettings settings, string updatedBy);
    }
}

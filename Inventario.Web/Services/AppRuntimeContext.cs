using System.Threading.Tasks;
using Inventario.Core.Interfaces;
using Inventario.Core.Models;

namespace Inventario.Web.Services
{
    public class AppRuntimeContext
    {
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings _currentSettings;
        private bool _isLoaded = false;
        private readonly object _lock = new object();

        public AppRuntimeContext(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }

        public AppSettings CurrentSettings
        {
            get
            {
                if (!_isLoaded)
                {
                    // Cargar de forma síncrona si es necesario
                    LoadSettingsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                return _currentSettings ?? new AppSettings();
            }
        }

        public async Task LoadSettingsAsync()
        {
            if (!_isLoaded)
            {
                lock (_lock)
                {
                    if (!_isLoaded)
                    {
                        _currentSettings = _appSettingsService.GetAppSettingsAsync().Result;
                        _isLoaded = true;
                    }
                }
            }
        }

        public async Task RefreshSettingsAsync()
        {
            _currentSettings = await _appSettingsService.GetAppSettingsAsync();
            _isLoaded = true;
        }
    }
}

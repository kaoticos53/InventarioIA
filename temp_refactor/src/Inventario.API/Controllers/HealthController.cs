using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Inventario.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly HealthCheckService _healthCheckService;

        public HealthController(ILogger<HealthController> logger, HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            _logger.LogInformation($"Health check executed. Status: {healthReport.Status}");
            
            var response = new
            {
                status = healthReport.Status.ToString(),
                checks = healthReport.Entries.Select(e => new 
                { 
                    name = e.Key, 
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration,
                    exception = e.Value.Exception?.Message
                }),
                totalDuration = healthReport.TotalDuration
            };

            return healthReport.Status == HealthStatus.Healthy ? 
                Ok(response) : 
                StatusCode(503, response);
        }
    }
}

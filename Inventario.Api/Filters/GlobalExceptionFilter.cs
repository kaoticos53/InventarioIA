using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Inventario.Api.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception has occurred");

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ha ocurrido un error interno en el servidor.";

            if (context.Exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "No autorizado para realizar esta acción.";
            }
            else if (context.Exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = "El recurso solicitado no fue encontrado.";
            }
            else if (context.Exception is ArgumentException || context.Exception is ArgumentNullException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = context.Exception.Message;
            }

            var response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                // En desarrollo, incluir detalles del error
                Details = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.EnvironmentName == "Development" 
                    ? context.Exception.ToString() 
                    : null
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}

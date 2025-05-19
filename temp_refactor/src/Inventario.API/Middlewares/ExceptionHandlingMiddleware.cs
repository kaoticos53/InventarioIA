using System.Net;
using System.Text.Json;
using Inventario.Core.Application.Common.Exceptions;
using Inventario.Core.Application.DTOs;

namespace Inventario.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var response = new ApiResponse<object>();
            var stackTrace = string.Empty;

            switch (exception)
            {
                case BaseException ex:
                    statusCode = ex.StatusCode;
                    response = ApiResponse<object>.ErrorResponse(ex.Message, ex.Errors);
                    stackTrace = ex.StackTrace;
                    break;
                case FluentValidation.ValidationException ex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    var failures = ex.Errors
                        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                        .ToDictionary(
                            failureGroup => failureGroup.Key,
                            failureGroup => failureGroup.ToArray());
                    response = ApiResponse<object>.ErrorResponse("Error de validación", failures);
                    break;
                default:
                    stackTrace = _env.IsDevelopment() ? exception.StackTrace : string.Empty;
                    response = ApiResponse<object>.ErrorResponse(
                        _env.IsDevelopment() ? exception.Message : "Ha ocurrido un error interno del servidor.");
                    break;
            }

            _logger.LogError(exception, $"Error: {exception.Message}\nStackTrace: {stackTrace}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            
            await context.Response.WriteAsync(json);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}

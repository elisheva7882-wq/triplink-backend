using System.Net;
using System.Text.Json;
using TripLink.Core.Exceptions;

namespace TripLink.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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

        private async Task HandleExceptionAsync(
    HttpContext context,
    Exception exception)
        {
            var statusCode = exception switch
            {
                ArgumentException =>
                    HttpStatusCode.BadRequest,

                UnauthorizedAccessException =>
                    HttpStatusCode.Unauthorized,

                NotFoundException =>
                    HttpStatusCode.NotFound,

                ForbiddenException =>
                    HttpStatusCode.Forbidden,

                DomainConflictException =>
                    HttpStatusCode.Conflict,

                _ =>
                    HttpStatusCode.InternalServerError
            };

            var correlationId =
                context.Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString("N");

            if (statusCode == HttpStatusCode.Conflict)
            {
                _logger.LogWarning(
                    exception,
                    "A business conflict occurred.");
            }

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,

                message = statusCode ==
                          HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,

                 correlationId
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
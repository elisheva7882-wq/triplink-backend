namespace TripLink.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId =
                context.Request.Headers.TryGetValue(
                    HeaderName,
                    out var existingId)
                    ? existingId.ToString()
                    : Guid.NewGuid().ToString("N");

            context.Items["CorrelationId"] = correlationId;

            context.Response.Headers[HeaderName] =
                correlationId;

            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
            {
                _logger.LogInformation(
                    "Request {Method} {Path} started.",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                _logger.LogInformation(
                    "Request {Method} {Path} completed with status {StatusCode}.",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
            }
        }
    }
}
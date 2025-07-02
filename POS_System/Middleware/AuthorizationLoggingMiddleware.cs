namespace POS_System.Middleware
{
    public class AuthorizationLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationLoggingMiddleware> _logger;

        public AuthorizationLoggingMiddleware(RequestDelegate next, ILogger<AuthorizationLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var userName = context.User.Identity?.Name ?? "Anonymous";
                var path = context.Request.Path;
                _logger.LogWarning("🚫 ACCESS DENIED for User: {User}, Path: {Path}", userName, path);
            }
        }
    }

    public static class AuthorizationLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationLoggingMiddleware>();
        }
    }
}

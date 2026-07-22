using System.Diagnostics;
using System.Globalization;

namespace CartService.API.Middleware
{
    public sealed class IdentityAccessTokenLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdentityAccessTokenLoggingMiddleware> _logger;

        public IdentityAccessTokenLoggingMiddleware(
            RequestDelegate next,
            ILogger<IdentityAccessTokenLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            if (context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning(
                    "Unauthenticated Cart request. Method={Method}, Path={Path}, " +
                    "StatusCode={StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);

                return;
            }

            var roleValues = context.User
                .FindAll("roles")
                .Select(claim => claim.Value)
                .ToArray();

            var expiresAt = ParseExpiration(
                context.User.FindFirst("exp")?.Value);

            _logger.LogInformation(
                "Cart identity access. Subject={Subject}, ObjectId={ObjectId}, " +
                "TenantId={TenantId}, ClientId={ClientId}, Roles={Roles}, " +
                "Scopes={Scopes}, Audience={Audience}, ExpiresAt={ExpiresAt}, " +
                "Method={Method}, Path={Path}, StatusCode={StatusCode}, " +
                "ElapsedMs={ElapsedMs}",
                context.User.FindFirst("sub")?.Value,
                context.User.FindFirst("oid")?.Value,
                context.User.FindFirst("tid")?.Value,
                context.User.FindFirst("azp")?.Value,
                roleValues,
                context.User.FindFirst("scp")?.Value,
                context.User.FindFirst("aud")?.Value,
                expiresAt,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }

        private static DateTimeOffset? ParseExpiration(string? expiration)
        {
            if (!long.TryParse(
                    expiration,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var unixSeconds))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
    }
}

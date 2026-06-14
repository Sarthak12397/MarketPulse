using Microsoft.Extensions.Configuration;

namespace MarketPulse.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        // Skip auth on Hangfire dashboard and health
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/hangfire") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key) &&
    !context.Request.Headers.TryGetValue("x-api-key", out key))        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing X-Api-Key header");
            return;
        }

        var adminKeys   = (config["ApiKeys:AdminKeys"]   ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var premiumKeys = (config["ApiKeys:PremiumKeys"] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (adminKeys.Contains(key.ToString()))
        {
            context.Items["Tier"] = "Admin";
        }
        else if (premiumKeys.Contains(key.ToString()))
        {
            context.Items["Tier"] = "Premium";
        }
        else
        {
            context.Items["Tier"] = "Free";
        }

        await _next(context);
    }
}
// ApplicationA/Middleware/ApiVersionMiddleware.cs
public class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiVersionMiddleware> _logger;

    public ApiVersionMiddleware(RequestDelegate next, ILogger<ApiVersionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        
        // Check if this is an API request
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            // Extract version from URL
            var segments = path.Split('/');
            if (segments.Length >= 3 && segments[2].StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                // Extract version number (e.g., from "v1" get "1.0")
                var versionNum = segments[2].TrimStart('v', 'V');
                var version = $"{versionNum}.0"; // Append .0 to match our version format
                
                if (!ApiVersioning.IsVersionSupported(version))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Error = "Unsupported API version",
                        SupportedVersions = ApiVersioning.SupportedVersions,
                        RequestedVersion = version
                    });
                    return;
                }
            }
            
            // Add version headers to response
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-API-Version", ApiVersioning.CurrentVersion);
                context.Response.Headers.Append("X-API-Supported-Versions", 
                    string.Join(", ", ApiVersioning.SupportedVersions));
                return Task.CompletedTask;
            });
        }

        await _next(context);
    }
}
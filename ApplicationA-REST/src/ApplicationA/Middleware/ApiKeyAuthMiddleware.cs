public class ApiKeyAuthMiddleware
{
    private const string API_KEY_HEADER = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly ApiKeyService _apiKeyService;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        ApiKeyService apiKeyService)
    {
        _next = next;
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for non-API endpoints
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // Skip authentication for health check endpoint
        if (context.Request.Path.StartsWithSegments("/api/v1/health"))
        {
            await _next(context);
            return;
        }

        string? apiKey = context.Request.Headers[API_KEY_HEADER];
        string? origin = context.Request.Headers.Origin;

        if (_apiKeyService.ValidateApiKey(apiKey, origin))
        {
            var keyDetails = _apiKeyService.GetKeyDetails(apiKey!);
            if (keyDetails != null)
            {
                // Add client info to the context for logging/tracking
                context.Items["ApiKeyName"] = keyDetails.Name;
                context.Items["ApiKeyDetails"] = keyDetails;
            }
            
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Authentication failed for request to {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Invalid API key",
                Details = "Please provide a valid API key in the X-API-Key header"
            });
        }
    }
}
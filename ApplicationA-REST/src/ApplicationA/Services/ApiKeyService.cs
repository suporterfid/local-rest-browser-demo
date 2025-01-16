public class ApiKeyService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyService> _logger;
    private readonly Dictionary<string, ApiKeyDetails> _apiKeys;

    public ApiKeyService(IConfiguration configuration, ILogger<ApiKeyService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _apiKeys = LoadApiKeys();
    }

    private Dictionary<string, ApiKeyDetails> LoadApiKeys()
    {
        var keys = new Dictionary<string, ApiKeyDetails>();
        
        // Load from configuration
        var apiKeysSection = _configuration.GetSection("ApiKeys");
        foreach (var keySection in apiKeysSection.GetChildren())
        {
            var key = keySection["Key"];
            if (!string.IsNullOrEmpty(key))
            {
                keys[key] = new ApiKeyDetails
                {
                    Name = keySection["Name"] ?? "Unknown",
                    AllowedOrigins = keySection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>(),
                    RateLimit = int.Parse(keySection["RateLimit"] ?? "100"),
                    IsEnabled = bool.Parse(keySection["IsEnabled"] ?? "true")
                };
            }
        }

        return keys;
    }

    public bool ValidateApiKey(string? apiKey, string? origin)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API key is missing");
            return false;
        }

        if (!_apiKeys.TryGetValue(apiKey, out var keyDetails))
        {
            _logger.LogWarning("Invalid API key attempted: {ApiKey}", apiKey);
            return false;
        }

        if (!keyDetails.IsEnabled)
        {
            _logger.LogWarning("Disabled API key attempted: {ApiKey}", apiKey);
            return false;
        }

        if (!string.IsNullOrEmpty(origin) && 
            keyDetails.AllowedOrigins.Length > 0 && 
            !keyDetails.AllowedOrigins.Contains(origin))
        {
            _logger.LogWarning("Unauthorized origin {Origin} for API key {ApiKey}", origin, apiKey);
            return false;
        }

        return true;
    }

    public ApiKeyDetails? GetKeyDetails(string apiKey)
    {
        return _apiKeys.GetValueOrDefault(apiKey);
    }
}

public class ApiKeyDetails
{
    public required string Name { get; set; }
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public int RateLimit { get; set; }
    public bool IsEnabled { get; set; }
}
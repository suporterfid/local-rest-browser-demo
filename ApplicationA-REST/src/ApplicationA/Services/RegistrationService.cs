// ApplicationA/Services/RegistrationService.cs
using System.Net.Http.Json;

public class RegistrationService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegistrationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _instanceId;
    private int _retryCount = 0;
    private const int MAX_RETRY_COUNT = 3;
    private const int INITIAL_RETRY_INTERVAL_SECONDS = 30;

    public RegistrationService(
        IConfiguration configuration,
        ILogger<RegistrationService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _instanceId = Guid.NewGuid().ToString();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var success = await RegisterWithAppB();
                if (success)
                {
                    _retryCount = 0; // Reset retry count on successful registration
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                else
                {
                    // Calculate exponential backoff time
                    var retryInterval = CalculateRetryInterval();
                    _logger.LogWarning("Will retry registration in {RetryInterval} seconds", retryInterval);
                    await Task.Delay(TimeSpan.FromSeconds(retryInterval), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Registration service is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in registration service");
                await Task.Delay(TimeSpan.FromSeconds(CalculateRetryInterval()), stoppingToken);
            }
        }
    }

    private int CalculateRetryInterval()
    {
        // Exponential backoff: 30s, 60s, 120s, then stays at 120s
        var multiplier = Math.Min(Math.Pow(2, _retryCount), 4);
        return (int)(INITIAL_RETRY_INTERVAL_SECONDS * multiplier);
    }

    private async Task<bool> RegisterWithAppB()
    {
        try
        {
            var registrationEndpoint = _configuration["AppB:RegistrationUrl"] 
                ?? "https://localhost:5002/api/discovery/register";

            var registrationData = new
            {
                InstanceId = _instanceId,
                Url = $"https://{_configuration["Host"]}:{_configuration["Port"]}",
                Status = "Active",
                LastHeartbeat = DateTime.UtcNow,
                Metadata = new
                {
                    Version = "1.0.0",
                    StartTime = DateTime.UtcNow,
                    Capabilities = new[] { "hello", "echo" }
                }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5 second timeout
            var response = await _httpClient.PostAsJsonAsync(registrationEndpoint, registrationData, cts.Token);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully registered with Application B");
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to register with Application B. Status code: {StatusCode}", response.StatusCode);
                _retryCount++;
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Cannot connect to Application B: {Message}. Application A will continue to operate independently.", 
                ex.Message.Contains("actively refused it") ? "Application B is not available" : ex.Message);
            _retryCount++;
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Registration request timed out");
            _retryCount++;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            _retryCount++;
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration service is stopping");
        
        try
        {
            var deregistrationEndpoint = _configuration["AppB:DeregistrationUrl"] 
                ?? "https://localhost:5002/api/discovery/deregister";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5 second timeout
            await _httpClient.PostAsJsonAsync(deregistrationEndpoint, new { InstanceId = _instanceId }, cts.Token);
            _logger.LogInformation("Successfully deregistered from Application B");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deregister from Application B");
        }

        await base.StopAsync(cancellationToken);
    }
}
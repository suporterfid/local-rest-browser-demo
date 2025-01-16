public class DiscoveryService
{
    private readonly ILogger<DiscoveryService> _logger;
    private readonly Dictionary<string, InstanceInfo> _instances = new();
    private readonly object _lock = new();

    public DiscoveryService(ILogger<DiscoveryService> logger)
    {
        _logger = logger;
    }

    public void RegisterInstance(InstanceInfo instanceInfo)
    {
        lock (_lock)
        {
            _instances[instanceInfo.InstanceId] = instanceInfo;
            _logger.LogInformation("Registered instance {InstanceId} at {Url}", 
                instanceInfo.InstanceId, instanceInfo.Url);
        }
    }

    public void DeregisterInstance(string instanceId)
    {
        lock (_lock)
        {
            if (_instances.Remove(instanceId))
            {
                _logger.LogInformation("Deregistered instance {InstanceId}", instanceId);
            }
        }
    }

    public List<InstanceInfo> GetActiveInstances()
    {
        lock (_lock)
        {
            // Remove instances that haven't sent a heartbeat in 2 minutes
            var inactiveInstances = _instances.Values
                .Where(i => DateTime.UtcNow - i.LastHeartbeat > TimeSpan.FromMinutes(2))
                .Select(i => i.InstanceId)
                .ToList();

            foreach (var instanceId in inactiveInstances)
            {
                _instances.Remove(instanceId);
                _logger.LogWarning("Removed inactive instance {InstanceId}", instanceId);
            }

            return _instances.Values.ToList();
        }
    }
}

public class InstanceInfo
{
    public required string InstanceId { get; set; }
    public required string Url { get; set; }
    public required string Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
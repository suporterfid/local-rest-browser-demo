public static class ApiVersioning
{
    public const string CurrentVersion = "1.0";
    public static readonly string[] SupportedVersions = new[] { "1.0" };

    public static class V1
    {
        public const string Route = "api/v1";
    }

    public static bool IsVersionSupported(string version)
    {
        return SupportedVersions.Contains(version);
    }
}

public class ApiVersionAttribute : Attribute
{
    public string Version { get; }

    public ApiVersionAttribute(string version)
    {
        Version = version;
    }
}

public class VersionedResponse<T>
{
    public T Data { get; set; }
    public string ApiVersion { get; set; }
    public DateTime Timestamp { get; set; }

    public VersionedResponse(T data)
    {
        Data = data;
        ApiVersion = ApiVersioning.CurrentVersion;
        Timestamp = DateTime.UtcNow;
    }
}
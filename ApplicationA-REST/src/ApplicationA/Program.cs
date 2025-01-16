var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application A - REST Server...");

// Add configuration for host and allowed origins
builder.Configuration.AddCommandLine(args);
var host = builder.Configuration["Host"] ?? "localhost";
var port = int.Parse(builder.Configuration["Port"] ?? "5001");
var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "https://localhost:5002").Split(',');

Console.WriteLine($"Server will listen on: https://{host}:{port}");
Console.WriteLine($"Allowed origins: {string.Join(", ", allowedOrigins)}");

// Get the base directory path
var baseDirectory = Directory.GetCurrentDirectory();
var certificatePath = Path.Combine(baseDirectory, "..", "..", "..", "certificates", "ApplicationA.pfx");
var certificatePassword = "demo123";

Console.WriteLine($"Using certificate from: {certificatePath}");

// Configure HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    // If host is "localhost" or "127.0.0.1", use ListenLocalhost, otherwise listen on any IP
    if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || 
        host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
    {
        options.ListenLocalhost(port, listenOptions =>
        {
            Console.WriteLine($"Configuring HTTPS on port {port} for localhost");
            listenOptions.UseHttps(certificatePath, certificatePassword);
        });
    }
    else
    {
        options.Listen(System.Net.IPAddress.Parse(host), port, listenOptions =>
        {
            Console.WriteLine($"Configuring HTTPS on port {port} for {host}");
            listenOptions.UseHttps(certificatePath, certificatePassword);
        });
    }
});

// Configure CORS
Console.WriteLine("Configuring CORS policy for Web Application");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure HTTP client
builder.Services.AddHttpClient<RegistrationService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

// Add the registration service
builder.Services.AddHostedService<RegistrationService>();

// Register API key service
builder.Services.AddSingleton<ApiKeyService>();


// Add configuration for Application B URL
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    {"AppB:RegistrationUrl", "https://localhost:5002/api/discovery/register"},
    {"AppB:DeregistrationUrl", "https://localhost:5002/api/discovery/deregister"}
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowWebApp");

// Add the version middleware
app.UseMiddleware<ApiVersionMiddleware>();

// Add the API key authentication middleware after CORS and before endpoints
app.UseMiddleware<ApiKeyAuthMiddleware>();

// Optional: Add middleware to log API key usage
app.Use(async (context, next) =>
{
    if (context.Items.TryGetValue("ApiKeyName", out var keyName))
    {
        Console.WriteLine($"Request to {context.Request.Path} using API key: {keyName}");
    }
    await next();
});

// Enable static files
app.UseStaticFiles();
Console.WriteLine("Static files middleware enabled");

// API endpoints
Console.WriteLine("Configuring API endpoints");

// app.MapGet("/api/hello", (HttpContext context) =>
// {
//     Console.WriteLine($"GET /api/hello requested from {context.Request.Headers["Origin"]}");
//     return Results.Json(new { message = "Hello from Application A!" });
// });

// app.MapPost("/api/echo", async (HttpContext context) =>
// {
//     Console.WriteLine($"POST /api/echo requested from {context.Request.Headers["Origin"]}");
//     using var reader = new StreamReader(context.Request.Body);
//     var body = await reader.ReadToEndAsync();
//     Console.WriteLine($"Echo request body: {body}");
//     return Results.Json(new { echo = body });
// });

// app.MapGet("/api/health", (HttpContext context) =>
// {
//     Console.WriteLine($"Health check requested from {context.Request.Headers["Origin"]}");
//     return Results.Json(new { 
//         status = "healthy",
//         timestamp = DateTime.UtcNow,
//         version = "1.0.0",
//         endpoint = context.Request.Host.ToString()
//     });
// });

// Version 1 endpoints
app.MapGet($"/{ApiVersioning.V1.Route}/health", [ApiVersion("1.0")] (HttpContext context) =>
{
    Console.WriteLine($"Health check requested from {context.Request.Headers["Origin"]}");
    var response = new VersionedResponse<object>(new { 
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        endpoint = context.Request.Host.ToString()
    });
    return Results.Json(response);
});

app.MapGet($"/{ApiVersioning.V1.Route}/hello", [ApiVersion("1.0")] (HttpContext context) =>
{
    Console.WriteLine($"GET /{ApiVersioning.V1.Route}/hello requested from {context.Request.Headers["Origin"]}");
    var response = new VersionedResponse<object>(new { message = "Hello from Application A!" });
    return Results.Json(response);
});

app.MapPost($"/{ApiVersioning.V1.Route}/echo", [ApiVersion("1.0")] async (HttpContext context) =>
{
    Console.WriteLine($"POST /{ApiVersioning.V1.Route}/echo requested from {context.Request.Headers["Origin"]}");
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"Echo request body: {body}");
    var response = new VersionedResponse<object>(new { echo = body });
    return Results.Json(response);
});

Console.WriteLine("Application A is ready to start");
app.Run();
Console.WriteLine("Application A is shutting down");
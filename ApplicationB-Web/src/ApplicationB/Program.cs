using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application B - Web Server...");

// Add configuration for Application A hosts
builder.Configuration.AddCommandLine(args);
var appAHosts = (builder.Configuration["AppAHosts"] ?? "https://localhost:5001").Split(',');
Console.WriteLine($"Available Application A hosts: {string.Join(", ", appAHosts)}");

var apiKey = builder.Configuration["AppAApiKey"] ?? "demo-key-1";
Console.WriteLine("API key configured for Application A communication");

// Get the base directory path
var baseDirectory = Directory.GetCurrentDirectory();
var certificatePath = Path.Combine(baseDirectory, "..", "..", "..", "certificates", "ApplicationB.pfx");
var certificatePassword = "demo123";

Console.WriteLine($"Using certificate from: {certificatePath}");

// Configure HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002, listenOptions =>
    {
        Console.WriteLine("Configuring HTTPS on port 5002");
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

// Register the DiscoveryService
builder.Services.AddSingleton<DiscoveryService>();

var app = builder.Build();

// Configure static files with JSX support
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".jsx"] = "application/javascript"; // Update existing mapping instead of adding

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "",
    ContentTypeProvider = contentTypeProvider
});

// Serve default page with injected configuration
Console.WriteLine("Configuring default page route");
app.MapGet("/", async context =>
{
    var html = await File.ReadAllTextAsync("wwwroot/index.html");
    // Replace both the hosts and API key
    html = html.Replace("APP_A_HOSTS", JsonSerializer.Serialize(appAHosts));
    html = html.Replace("APP_A_API_KEY", JsonSerializer.Serialize(apiKey));
    await context.Response.WriteAsync(html);
    Console.WriteLine("index.html sent to client with configured host and API key");
});

app.MapPost("/api/discovery/register", async (HttpContext context, DiscoveryService discovery) =>
{
    var instanceInfo = await context.Request.ReadFromJsonAsync<InstanceInfo>();
    if (instanceInfo == null)
    {
        return Results.BadRequest("Invalid instance information");
    }

    discovery.RegisterInstance(instanceInfo);
    return Results.Ok();
});

app.MapPost("/api/discovery/deregister", async (HttpContext context, DiscoveryService discovery) =>
{
    var request = await context.Request.ReadFromJsonAsync<DeregisterRequest>();
    if (request?.InstanceId == null)
    {
        return Results.BadRequest("Instance ID is required");
    }

    discovery.DeregisterInstance(request.InstanceId);
    return Results.Ok();
});

app.MapGet("/api/discovery/instances", (DiscoveryService discovery) =>
{
    var instances = discovery.GetActiveInstances();
    return Results.Json(instances);
});

Console.WriteLine("Application B is ready to start");
app.Run();
Console.WriteLine("Application B is shutting down");

public class DeregisterRequest
{
    public required string InstanceId { get; set; }
}
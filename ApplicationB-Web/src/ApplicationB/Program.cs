using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application B - Web Server...");

// Add configuration for Application A hosts
builder.Configuration.AddCommandLine(args);
var appAHosts = (builder.Configuration["AppAHosts"] ?? "https://localhost:5001").Split(',');
Console.WriteLine($"Available Application A hosts: {string.Join(", ", appAHosts)}");

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
    html = html.Replace("APP_A_HOSTS", JsonSerializer.Serialize(appAHosts));
    await context.Response.WriteAsync(html);
    Console.WriteLine("index.html sent to client with configured host");
});

Console.WriteLine("Application B is ready to start");
app.Run();
Console.WriteLine("Application B is shutting down");
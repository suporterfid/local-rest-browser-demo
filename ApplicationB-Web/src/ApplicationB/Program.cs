var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application B - Web Server...");

// Add configuration for Application A host
builder.Configuration.AddCommandLine(args);
var appAHost = builder.Configuration["AppAHost"] ?? "https://localhost:5001";
Console.WriteLine($"Application A host configured as: {appAHost}");

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

// Enable static files
Console.WriteLine("Enabling static files middleware");
app.UseStaticFiles();

// Serve default page with injected configuration
Console.WriteLine("Configuring default page route");
app.MapGet("/", async context =>
{
    var html = await File.ReadAllTextAsync("wwwroot/index.html");
    html = html.Replace("APP_A_HOST", appAHost);
    await context.Response.WriteAsync(html);
    Console.WriteLine("index.html sent to client with configured host");
});

Console.WriteLine("Application B is ready to start");
app.Run();
Console.WriteLine("Application B is shutting down");
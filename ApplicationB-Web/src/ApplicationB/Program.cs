var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application B - Web Server...");

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

// Serve default page
Console.WriteLine("Configuring default page route");
app.MapGet("/", async context =>
{
    //Console.WriteLine($"GET / requested from {context.Request.Headers["Origin"] ?? "unknown origin"}");
    await context.Response.SendFileAsync("wwwroot/index.html");
    Console.WriteLine("index.html sent to client");
});

Console.WriteLine("Application B is ready to start");
app.Run();
Console.WriteLine("Application B is shutting down");
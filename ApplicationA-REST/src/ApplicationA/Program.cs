var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting Application A - REST Server...");

// Get the base directory path
var baseDirectory = Directory.GetCurrentDirectory();
var certificatePath = Path.Combine(baseDirectory, "..", "..", "..", "certificates", "ApplicationA.pfx");
var certificatePassword = "demo123";

Console.WriteLine($"Using certificate from: {certificatePath}");

// Configure HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        Console.WriteLine("Configuring HTTPS on port 5001");
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

// Configure CORS
Console.WriteLine("Configuring CORS policy for Web Application");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:5002")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowWebApp");

// Enable static files
app.UseStaticFiles();
Console.WriteLine("Static files middleware enabled");

// API endpoints
Console.WriteLine("Configuring API endpoints");

app.MapGet("/api/hello", (HttpContext context) =>
{
    Console.WriteLine($"GET /api/hello requested from {context.Request.Headers["Origin"]}");
    return Results.Json(new { message = "Hello from Application A!" });
});

app.MapPost("/api/echo", async (HttpContext context) =>
{
    Console.WriteLine($"POST /api/echo requested from {context.Request.Headers["Origin"]}");
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"Echo request body: {body}");
    return Results.Json(new { echo = body });
});

Console.WriteLine("Application A is ready to start");
app.Run();
Console.WriteLine("Application A is shutting down");
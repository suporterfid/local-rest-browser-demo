var builder = WebApplication.CreateBuilder(args);

// Get the base directory path
var baseDirectory = Directory.GetCurrentDirectory();
var certificatePath = Path.Combine(baseDirectory, "..", "..", "..", "certificates", "ApplicationA.pfx");
var certificatePassword = "demo123";

// Configure HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

// Configure CORS
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

// API endpoints
app.MapGet("/api/hello", () => Results.Json(new { message = "Hello from Application A!" }));
app.MapPost("/api/echo", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    return Results.Json(new { echo = body });
});

app.Run();
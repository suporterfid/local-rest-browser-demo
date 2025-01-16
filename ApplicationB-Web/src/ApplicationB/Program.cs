var builder = WebApplication.CreateBuilder(args);

// Get the base directory path
var baseDirectory = Directory.GetCurrentDirectory();
var certificatePath = Path.Combine(baseDirectory, "..", "..", "..", "certificates", "ApplicationA.pfx");
var certificatePassword = "demo123";

// Configure HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002, listenOptions =>
    {
        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

var app = builder.Build();

// Enable static files
app.UseStaticFiles();

// Serve default page
app.MapGet("/", async context =>
{
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
using Microsoft.EntityFrameworkCore;
using WabiView.Data;
using WabiView.Services;

var builder = WebApplication.CreateBuilder(args);

// Check for demo mode (for local preview without Bitcoin Core/Electrs)
var isDemoMode = Environment.GetEnvironmentVariable("WABIVIEW_DEMO") == "1"
    || builder.Environment.IsDevelopment();

if (isDemoMode)
{
    Console.WriteLine("========================================");
    Console.WriteLine("  WabiView - DEMO MODE");
    Console.WriteLine("  Using sample data, no Bitcoin Core/Electrs required");
    Console.WriteLine("========================================");
}

// Configure services
builder.Services.AddRazorPages();

// Database - use local path in demo mode
var dataPath = isDemoMode
    ? Path.Combine(Directory.GetCurrentDirectory(), ".wabiview-data")
    : Environment.GetEnvironmentVariable("WABIVIEW_DATA_PATH") ?? "/data";
Directory.CreateDirectory(dataPath);
var dbPath = Path.Combine(dataPath, "wabiview.db");

builder.Services.AddDbContext<WabiViewDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Configuration
builder.Services.Configure<BitcoinRpcSettings>(builder.Configuration.GetSection("BitcoinRpc"));
builder.Services.Configure<ElectrsSettings>(builder.Configuration.GetSection("Electrs"));

// Core services
builder.Services.AddSingleton<ManualCoordinatorRegistry>();
builder.Services.AddHttpClient<CoordinatorMonitorService>();
builder.Services.AddHttpClient<ElectrsService>();
builder.Services.AddScoped<BitcoinRpcService>();
builder.Services.AddScoped<CoinjoinService>();

// Background services - only run if not in demo mode
if (!isDemoMode)
{
    builder.Services.AddHostedService<CoordinatorPollingService>();
    builder.Services.AddHostedService<CoinjoinScannerService>();
}

var app = builder.Build();

// Ensure database is created and seed demo data if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WabiViewDbContext>();
    db.Database.EnsureCreated();

    if (isDemoMode)
    {
        await DemoDataService.SeedDemoDataAsync(db);
        Console.WriteLine("Demo data seeded successfully!");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// Health check endpoint for StartOS
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

var port = Environment.GetEnvironmentVariable("WABIVIEW_PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();

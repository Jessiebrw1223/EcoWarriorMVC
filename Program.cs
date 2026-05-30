using EcoWarriorMVC.Data;
using EcoWarriorMVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ─────────────────────────────────────────────
// MVC + JSON camelCase
// ─────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase);

// ─────────────────────────────────────────────
// PostgreSQL
// ─────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────────
// Sesiones
// ─────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ─────────────────────────────────────────────
// Cache
// ─────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ─────────────────────────────────────────────
// Servicios
// ─────────────────────────────────────────────
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// ML.NET
builder.Services.AddSingleton<IEcoRecommendationService,
    EcoRecommendationService>();

// Weather API
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>(
            "Weather:TimeoutSeconds",
            10));

    client.DefaultRequestHeaders.Add(
        "User-Agent",
        "EcoWarriorMVC/1.0");
});

// Semantic Kernel
builder.Services.AddScoped<IEcoAiAgentService,
    EcoAiAgentService>();

// ─────────────────────────────────────────────
// Logging
// ─────────────────────────────────────────────
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ─────────────────────────────────────────────
// ENTRENAR ML.NET
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// Middleware
// ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// ─────────────────────────────────────────────
// Endpoints
// ─────────────────────────────────────────────
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// ─────────────────────────────────────────────
// Seed DB
// ─────────────────────────────────────────────
DbInitializer.EnsureSeeded(app.Services);

app.Run();
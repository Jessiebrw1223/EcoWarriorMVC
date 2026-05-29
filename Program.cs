using EcoWarriorMVC.Data;
using EcoWarriorMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── MVC + JSON camelCase ──────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase);

// ─── Base de datos PostgreSQL ──────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Sesiones ──────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// ─── Caché en memoria (WeatherService) ────────────────────────────
builder.Services.AddMemoryCache();

// ─── Servicios de negocio ──────────────────────────────────────────
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// ML.NET: Singleton (modelo entrena lazy una sola vez)
builder.Services.AddSingleton<IEcoRecommendationService, EcoRecommendationService>();

// HttpClient del Weather con timeout
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("Weather:TimeoutSeconds", 10));
    client.DefaultRequestHeaders.Add("User-Agent", "EcoWarriorMVC/1.0");
});

// Agente IA Semantic Kernel (Scoped)
builder.Services.AddScoped<IEcoAiAgentService, EcoAiAgentService>();

// ─── Logging ──────────────────────────────────────────────────────
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);


MLTrainingService.TrainModel();

var app = builder.Build();

// ─── Middleware ────────────────────────────────────────────────────
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

// ─── Rutas ────────────────────────────────────────────────────────
// API controllers primero
app.MapControllers();

// MVC default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// ─── Seed BD ───────────────────────────────────────────────────────
DbInitializer.EnsureSeeded(app.Services);

app.Run();

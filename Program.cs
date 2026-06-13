using EcoWarriorMVC.Data;
using EcoWarriorMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render usa la variable PORT. En local usará 10000 si no existe.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró ConnectionStrings:DefaultConnection. Configúralo en appsettings.Development.json o en Render como ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var redisConnection = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "EcoWarriorMVC:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EcoWarrior.Session";
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IEcoAiAgentService, EcoAiAgentService>();

// ML.NET activo: el modelo se entrena de forma Lazy dentro del servicio.
builder.Services.AddSingleton<IEcoRecommendationService, EcoRecommendationService>();

builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("Weather:TimeoutSeconds", 10));

    client.DefaultRequestHeaders.UserAgent.ParseAdd("EcoWarriorMVC/1.0");
});

var app = builder.Build();

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

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

try
{
    DbInitializer.EnsureSeeded(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
    logger?.LogError(ex, "No se pudo migrar o sembrar la base de datos. Revisa la cadena de conexión.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    try
    {
        var recommender = app.Services.GetRequiredService<IEcoRecommendationService>();
        recommender.Warmup();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
        logger?.LogWarning(ex, "No se pudo inicializar el servicio ML en warmup.");
    }
}

// En desarrollo, entrenar el modelo ML al iniciar para que la vista muestre
// recomendaciones inmediatamente. Evitar en producción para no bloquear arranque.
if (app.Environment.IsDevelopment())
{
    try
    {
        var recommender = app.Services.GetRequiredService<IEcoRecommendationService>();
        recommender.Warmup();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
        logger?.LogWarning(ex, "No se pudo inicializar el servicio ML en warmup.");
    }
}

app.Run();

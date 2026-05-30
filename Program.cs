using EcoWarriorMVC.Data;
using EcoWarriorMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render usa la variable PORT. En local usará 10000 si no existe.
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// ML.NET activo: el modelo se entrena de forma Lazy dentro del servicio.
// No entrenar en Program.cs para evitar que Render cierre la app al iniciar.
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

DbInitializer.EnsureSeeded(app.Services);

app.Run();

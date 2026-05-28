using EcoWarriorMVC.Data;
using EcoWarriorMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddSingleton<IEcoRecommendationService, EcoRecommendationService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

DbInitializer.EnsureSeeded(app.Services);

app.Urls.Add("http://0.0.0.0:8080");

app.Run();
using EcoWarriorMvc.Data;
using EcoWarriorMvc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
{
    connectionString = PostgresUrlConverter.ToNpgsql(connectionString);
}
connectionString ??= builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "EcoWarrior.Session";
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<ClassificationService>();
builder.Services.AddScoped<SemanticKernelService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.EnsureCreated();
        SeedData.Initialize(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR INICIALIZANDO BD:");
        Console.WriteLine(ex.ToString());
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

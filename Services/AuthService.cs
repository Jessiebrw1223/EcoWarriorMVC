using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Services;

public class AuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;
    public AuthService(ApplicationDbContext db, IHttpContextAccessor http) { _db = db; _http = http; }
    public int? UserId => _http.HttpContext?.Session.GetInt32("UserId");
    public string? UserRole => _http.HttpContext?.Session.GetString("UserRole");
    public bool IsAuthenticated => UserId.HasValue;
    public async Task<AppUser?> CurrentUserAsync() => UserId.HasValue ? await _db.Users.FindAsync(UserId.Value) : null;
    public async Task<(bool ok, string message)> RegisterAsync(RegisterViewModel model)
    {
        if (await _db.Users.AnyAsync(x => x.Email == model.Email)) return (false, "El correo ya está registrado.");
        var user = new AppUser { FullName = model.FullName, Email = model.Email.ToLower(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), District = model.District };
        _db.Users.Add(user); await _db.SaveChangesAsync(); SignIn(user); return (true, "Cuenta creada correctamente.");
    }
    public async Task<bool> LoginAsync(LoginViewModel model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash)) return false;
        SignIn(user); return true;
    }
    public void SignIn(AppUser user)
    {
        _http.HttpContext!.Session.SetInt32("UserId", user.Id);
        _http.HttpContext.Session.SetString("UserName", user.FullName);
        _http.HttpContext.Session.SetString("UserRole", user.Role);
    }
    public void Logout() => _http.HttpContext?.Session.Clear();
}

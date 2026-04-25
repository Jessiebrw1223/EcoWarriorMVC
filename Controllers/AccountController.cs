using EcoWarriorMvc.Models;
using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;
    public IActionResult Login() => View(new LoginViewModel());
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _auth.LoginAsync(model)) return RedirectToAction("Index", "Home");
        ModelState.AddModelError("", "Correo o contraseña incorrectos."); return View(model);
    }
    public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _auth.RegisterAsync(model);
        if (result.ok) return RedirectToAction("Index", "Home");
        ModelState.AddModelError("", result.message); return View(model);
    }
    public IActionResult Logout() { _auth.Logout(); return RedirectToAction("Login"); }
}

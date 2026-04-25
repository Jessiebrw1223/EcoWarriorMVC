using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class ProfileController : Controller
{
    private readonly AuthService _auth;
    public ProfileController(AuthService auth) => _auth = auth;
    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        return View(await _auth.CurrentUserAsync());
    }
}

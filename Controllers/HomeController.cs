using System.Diagnostics;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using EcoWarriorMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

public class HomeController(
    IProductoService productoService,
    IHomeService     homeService,
    IWeatherService  weatherService) : Controller
{
    // ─── AUTH ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioCorreo")))
            return RedirectToAction(nameof(Index));
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var login = homeService.IniciarSesion(model);
        if (login is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }
        GuardarSesion(login.Nombre, login.Correo);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Registro()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioCorreo")))
            return RedirectToAction(nameof(Index));
        return View(new RegistroViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var resultado = homeService.RegistrarUsuario(new RegistroRequest
        {
            Nombre = model.Nombre, Correo = model.Correo, Contrasena = model.Contrasena
        });
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }
        GuardarSesion(model.Nombre, model.Correo);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        TempData.Clear();
        return RedirectToAction(nameof(Login));
    }

    // ─── DASHBOARD ───────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioCorreo")))
            return RedirectToAction(nameof(Login));

        ConfigurarDatosUsuarioEnVista();

        var correo  = HttpContext.Session.GetString("UsuarioCorreo")!;
        var perfil  = homeService.ObtenerPerfil(correo);
        var puntos  = perfil?.Puntos ?? 0;
        var retos   = perfil?.RetosCompletados ?? 0;

        // Clima cargado server-side como fallback inicial; el JS lo refresca vía API
        var clima = await weatherService.ObtenerClimaPorCiudadAsync("Lima");

        var (nivel, pMin, pMax) = CalcularNivelDetallado(puntos);
        var progreso = pMax > pMin
            ? Math.Min(100.0, (double)(puntos - pMin) / (pMax - pMin) * 100)
            : 100.0;

        var model = new HomeViewModel
        {
            Destacados               = productoService.ObtenerDestacados(),
            TotalProductos           = productoService.ObtenerTodos().Count,
            TotalCategorias          = productoService.ObtenerTodos().Select(x => x.Categoria).Distinct().Count(),
            StockDisponible          = productoService.ObtenerTodos().Sum(x => x.Stock),
            Clima                    = clima,
            PuntosUsuario            = puntos,
            RetosCompletados         = retos,
            ReduccionCO2Kg           = Math.Round(retos * 1.05, 1),
            NivelUsuario             = nivel,
            PuntosParaSiguienteNivel = Math.Max(0, pMax - puntos),
            PorcentajeProgreso       = Math.Round(progreso, 1)
        };

        return View(model);
    }

    // Vistas de sección — protegidas con redirect a login
    [HttpGet] public IActionResult Retos()   { if (!Autenticado()) return RedirectToAction(nameof(Login)); ConfigurarDatosUsuarioEnVista(); return View(); }
    [HttpGet] public IActionResult Ranking() { if (!Autenticado()) return RedirectToAction(nameof(Login)); ConfigurarDatosUsuarioEnVista(); return View(); }
    [HttpGet] public IActionResult Perfil()  { if (!Autenticado()) return RedirectToAction(nameof(Login)); ConfigurarDatosUsuarioEnVista(); return View(); }

    public IActionResult Nosotros() => View();

    [HttpGet] public IActionResult Contacto() => View(new ContactoViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Contacto(ContactoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        TempData["MensajeExito"] = $"Gracias, {model.Nombre}. Tu mensaje fue recibido.";
        return RedirectToAction(nameof(Contacto));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    // ─── HELPERS ────────────────────────────────────────────────────────────

    private bool Autenticado() =>
        !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioCorreo"));

    private void GuardarSesion(string nombre, string correo)
    {
        HttpContext.Session.SetString("UsuarioNombre", nombre);
        HttpContext.Session.SetString("UsuarioCorreo", correo);
    }

    private void ConfigurarDatosUsuarioEnVista()
    {
        var nombre  = HttpContext.Session.GetString("UsuarioNombre") ?? "Eco Warrior";
        var correo  = HttpContext.Session.GetString("UsuarioCorreo") ?? "eco@ecowarrior.com";
        var username = "@" + correo.Split('@')[0].ToLowerInvariant();
        var iniciales = string.Concat(
            nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => char.ToUpperInvariant(x[0])));
        var primerNombre = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? nombre;

        ViewData["NombreUsuario"]       = nombre;
        ViewData["UsernameUsuario"]     = username;
        ViewData["InicialesUsuario"]    = iniciales is { Length: > 0 } ? iniciales : "EW";
        ViewData["PrimerNombreUsuario"] = primerNombre;
    }

    private static (int Nivel, int Min, int Max) CalcularNivelDetallado(int pts) => pts switch
    {
        >= 3000 => (10, 3000, 5000),
        >= 2000 => (8,  2000, 3000),
        >= 1000 => (6,  1000, 2000),
        >= 500  => (4,   500, 1000),
        >= 200  => (3,   200,  500),
        >= 50   => (2,    50,  200),
        _       => (1,     0,   50),
    };
}

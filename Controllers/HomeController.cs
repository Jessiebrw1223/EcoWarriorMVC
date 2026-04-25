using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using EcoWarriorMVC.ViewModels;
using Microsoft.AspNetCore.Http;

namespace EcoWarriorMVC.Controllers;

public class HomeController(IProductoService productoService, IHomeService homeService) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var login = homeService.IniciarSesion(model);
        if (login is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contrasena incorrectos.");
            return View(model);
        }

        HttpContext.Session.SetString("UsuarioNombre", login.Nombre);
        HttpContext.Session.SetString("UsuarioCorreo", login.Correo);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = homeService.RegistrarUsuario(new RegistroRequest
        {
            Nombre = model.Nombre,
            Correo = model.Correo,
            Contrasena = model.Contrasena
        });

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        HttpContext.Session.SetString("UsuarioNombre", model.Nombre);
        HttpContext.Session.SetString("UsuarioCorreo", model.Correo);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        TempData.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Index()
    {
        ConfigurarDatosUsuarioEnVista();

        var productos = productoService.ObtenerTodos();
        var model = new HomeViewModel
        {
            Destacados = productoService.ObtenerDestacados(),
            TotalProductos = productos.Count,
            TotalCategorias = productos.Select(x => x.Categoria).Distinct().Count(),
            StockDisponible = productos.Sum(x => x.Stock)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Retos()
    {
        ConfigurarDatosUsuarioEnVista();
        return View();
    }

    [HttpGet]
    public IActionResult Ranking()
    {
        ConfigurarDatosUsuarioEnVista();
        return View();
    }

    [HttpGet]
    public IActionResult Perfil()
    {
        ConfigurarDatosUsuarioEnVista();
        return View();
    }

    public IActionResult Nosotros() => View();

    [HttpGet]
    public IActionResult Contacto() => View(new ContactoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contacto(ContactoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["MensajeExito"] = $"Gracias, {model.Nombre}. Tu mensaje fue recibido correctamente.";
        return RedirectToAction(nameof(Contacto));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private void ConfigurarDatosUsuarioEnVista()
    {
        var nombre = HttpContext.Session.GetString("UsuarioNombre") ?? "Eco Warrior";
        var correo = HttpContext.Session.GetString("UsuarioCorreo") ?? "eco@ecowarrior.com";

        var username = "@" + correo.Split('@')[0].ToLowerInvariant();
        var iniciales = ObtenerIniciales(nombre);
        var primerNombre = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? nombre;

        ViewData["NombreUsuario"] = nombre;
        ViewData["UsernameUsuario"] = username;
        ViewData["InicialesUsuario"] = iniciales;
        ViewData["PrimerNombreUsuario"] = primerNombre;
    }

    private static string ObtenerIniciales(string nombre)
    {
        var partes = nombre
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(x => char.ToUpperInvariant(x[0]));

        var iniciales = string.Concat(partes);
        return string.IsNullOrWhiteSpace(iniciales) ? "EW" : iniciales;
    }
}

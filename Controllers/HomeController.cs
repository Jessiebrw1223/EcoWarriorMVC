using System.Diagnostics;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using EcoWarriorMVC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

public class HomeController(
    IProductoService productoService,
    IHomeService homeService,
    IWeatherService weatherService) : Controller
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

    public async Task<IActionResult> Index([FromQuery] string? ciudad = null)
    {
        ConfigurarDatosUsuarioEnVista();

        var correo = ObtenerCorreoSesion();
        var perfil = homeService.ObtenerPerfil(correo);
        var productos = productoService.ObtenerTodos();
        var ciudadConsulta = string.IsNullOrWhiteSpace(ciudad) ? "Lima" : ciudad.Trim();
        var clima = await weatherService.ObtenerClimaPorCiudadAsync(ciudadConsulta);

        var model = new HomeViewModel
        {
            Destacados = productoService.ObtenerDestacados(),
            TotalProductos = productos.Count,
            TotalCategorias = productos.Select(x => x.Categoria).Distinct().Count(),
            StockDisponible = productos.Sum(x => x.Stock),
            Clima = clima,
            CiudadConsulta = ciudadConsulta,
            Perfil = perfil,
            Retos = homeService.ObtenerRetos(correo).Take(3).ToList(),
            Ranking = homeService.ObtenerRanking().Take(3).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Retos()
    {
        ConfigurarDatosUsuarioEnVista();

        var correo = ObtenerCorreoSesion();
        var model = new RetosViewModel
        {
            Perfil = homeService.ObtenerPerfil(correo),
            Retos = homeService.ObtenerRetos(correo),
            NuevoReto = new CrearRetoRequest()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearReto(RetosViewModel model)
    {
        ConfigurarDatosUsuarioEnVista();

        if (!ModelState.IsValid)
        {
            model.Perfil = homeService.ObtenerPerfil(ObtenerCorreoSesion());
            model.Retos = homeService.ObtenerRetos(ObtenerCorreoSesion());
            return View("Retos", model);
        }

        var resultado = homeService.CrearReto(model.NuevoReto);
        TempData[resultado.Exito ? "MensajeExito" : "MensajeError"] = resultado.Mensaje;

        return RedirectToAction(nameof(Retos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompletarReto(int retoId)
    {
        var resultado = homeService.CompletarReto(ObtenerCorreoSesion(), retoId);
        TempData[resultado.Exito ? "MensajeExito" : "MensajeError"] = resultado.Mensaje;

        return RedirectToAction(nameof(Retos));
    }

    [HttpGet]
    public IActionResult Ranking()
    {
        ConfigurarDatosUsuarioEnVista();
        ViewData["PuntosUsuario"] = homeService.ObtenerPerfil(ObtenerCorreoSesion())?.Puntos ?? 0;
        return View(homeService.ObtenerRanking());
    }

    [HttpGet]
    public IActionResult Perfil()
    {
        ConfigurarDatosUsuarioEnVista();

        var perfil = homeService.ObtenerPerfil(ObtenerCorreoSesion());
        var model = new PerfilViewModel
        {
            Perfil = perfil,
            Editar = new PerfilEditRequest
            {
                Nombre = perfil?.Nombre ?? string.Empty,
                Correo = perfil?.Correo ?? string.Empty,
                Ciudad = perfil?.Ciudad ?? "Lima",
                FotoUrl = perfil?.FotoUrl ?? string.Empty,
                CategoriaFavorita = perfil?.CategoriaFavorita ?? "General"
            },
            RetosCompletados = homeService.ObtenerRetos(ObtenerCorreoSesion())
                .Where(x => x.Completado)
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Perfil(PerfilViewModel model)
    {
        ConfigurarDatosUsuarioEnVista();

        if (!ModelState.IsValid)
        {
            model.Perfil = homeService.ObtenerPerfil(ObtenerCorreoSesion());
            model.RetosCompletados = homeService.ObtenerRetos(ObtenerCorreoSesion()).Where(x => x.Completado).ToList();
            return View(model);
        }

        var resultado = homeService.ActualizarPerfil(ObtenerCorreoSesion(), model.Editar);
        TempData[resultado.Exito ? "MensajeExito" : "MensajeError"] = resultado.Mensaje;

        if (resultado.Exito)
        {
            HttpContext.Session.SetString("UsuarioNombre", model.Editar.Nombre);
            HttpContext.Session.SetString("UsuarioCorreo", model.Editar.Correo);
        }

        return RedirectToAction(nameof(Perfil));
    }

    public IActionResult Nosotros() => View();

    [HttpGet]
    public IActionResult Avances() => View();

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
        var correo = ObtenerCorreoSesion();

        var username = "@" + correo.Split('@')[0].ToLowerInvariant();
        var iniciales = ObtenerIniciales(nombre);
        var primerNombre = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? nombre;

        ViewData["NombreUsuario"] = nombre;
        ViewData["UsernameUsuario"] = username;
        ViewData["InicialesUsuario"] = iniciales;
        ViewData["PrimerNombreUsuario"] = primerNombre;
    }

    private string ObtenerCorreoSesion()
    {
        return HttpContext.Session.GetString("UsuarioCorreo") ?? "admin@ecowarrior.com";
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

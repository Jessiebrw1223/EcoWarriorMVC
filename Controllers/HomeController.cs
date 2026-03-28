using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using EcoWarriorMVC.ViewModels;

namespace EcoWarriorMVC.Controllers;

public class HomeController(IProductoService productoService) : Controller
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

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
    {
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
}

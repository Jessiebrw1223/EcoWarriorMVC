using Microsoft.AspNetCore.Mvc;
using EcoWarriorMVC.Services;

namespace EcoWarriorMVC.Controllers;

public class ProductosController(IProductoService productoService) : Controller
{
    public IActionResult Index(string? categoria)
    {
        var productos = productoService.ObtenerTodos();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            productos = productos
                .Where(x => x.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Categoria = categoria;
        return View(productos);
    }

    public IActionResult Detalle(int id)
    {
        var producto = productoService.ObtenerPorId(id);
        if (producto is null)
        {
            return NotFound();
        }

        return View(producto);
    }
}

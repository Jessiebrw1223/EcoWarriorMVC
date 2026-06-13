using Microsoft.AspNetCore.Mvc;
using EcoWarriorMVC.Models;
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
        ViewBag.Categorias = productoService.ObtenerTodos()
            .Select(x => x.Categoria)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

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

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new Producto { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return View(producto);
        }

        productoService.Crear(producto);
        TempData["MensajeExito"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var producto = productoService.ObtenerPorId(id);
        if (producto is null)
        {
            return NotFound();
        }

        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Producto producto)
    {
        if (!ModelState.IsValid)
        {
            producto.Id = id;
            return View(producto);
        }

        var actualizado = productoService.Actualizar(id, producto);
        if (!actualizado)
        {
            return NotFound();
        }

        TempData["MensajeExito"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        var eliminado = productoService.Eliminar(id);
        TempData[eliminado ? "MensajeExito" : "MensajeError"] = eliminado
            ? "Producto eliminado correctamente."
            : "No se encontró el producto indicado.";

        return RedirectToAction(nameof(Index));
    }
}

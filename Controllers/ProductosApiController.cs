using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosApiController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Producto>> ObtenerTodos([FromQuery] string? categoria = null)
    {
        var productos = productoService.ObtenerTodos();
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            productos = productos
                .Where(x => x.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Producto> ObtenerPorId(int id)
    {
        var producto = productoService.ObtenerPorId(id);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public ActionResult<Producto> Crear([FromBody] Producto producto)
    {
        var creado = productoService.Crear(producto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, [FromBody] Producto producto)
    {
        return productoService.Actualizar(id, producto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        return productoService.Eliminar(id) ? NoContent() : NotFound();
    }
}

using EcoWarriorMVC.Data;
using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Services;

public class ProductoService(ApplicationDbContext dbContext) : IProductoService
{
    public IReadOnlyList<Producto> ObtenerTodos() =>
        dbContext.Productos
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToList();

    public IReadOnlyList<Producto> ObtenerDestacados(int cantidad = 3) =>
        dbContext.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderByDescending(p => p.Stock)
            .Take(cantidad)
            .ToList();

    public Producto? ObtenerPorId(int id) =>
        dbContext.Productos
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);

    public Producto Crear(Producto producto)
    {
        dbContext.Productos.Add(producto);
        dbContext.SaveChanges();
        return producto;
    }

    public bool Actualizar(int id, Producto productoActualizado)
    {
        var producto = dbContext.Productos.FirstOrDefault(p => p.Id == id);
        if (producto is null)
        {
            return false;
        }

        producto.Nombre = productoActualizado.Nombre;
        producto.Categoria = productoActualizado.Categoria;
        producto.Precio = productoActualizado.Precio;
        producto.Stock = productoActualizado.Stock;
        producto.Descripcion = productoActualizado.Descripcion;
        producto.ImagenUrl = productoActualizado.ImagenUrl;
        producto.Activo = productoActualizado.Activo;

        dbContext.SaveChanges();
        return true;
    }

    public bool Eliminar(int id)
    {
        var producto = dbContext.Productos.FirstOrDefault(p => p.Id == id);
        if (producto is null)
        {
            return false;
        }

        dbContext.Productos.Remove(producto);
        dbContext.SaveChanges();
        return true;
    }
}

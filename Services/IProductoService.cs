using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IProductoService
{
    IReadOnlyList<Producto> ObtenerTodos();
    IReadOnlyList<Producto> ObtenerDestacados(int cantidad = 3);
    Producto? ObtenerPorId(int id);
}

using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IProductoService
{
    IReadOnlyList<Producto> ObtenerTodos();
    IReadOnlyList<Producto> ObtenerDestacados(int cantidad = 3);
    Producto? ObtenerPorId(int id);
    Producto Crear(Producto producto);
    bool Actualizar(int id, Producto productoActualizado);
    bool Eliminar(int id);
}

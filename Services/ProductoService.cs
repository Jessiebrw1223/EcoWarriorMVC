using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public class ProductoService : IProductoService
{
    private readonly List<Producto> _productos =
    [
        new()
        {
            Id = 1,
            Nombre = "Botella reutilizable Eco",
            Categoria = "Hogar sostenible",
            Precio = 39.90m,
            Stock = 120,
            Descripcion = "Botella térmica reutilizable fabricada con acero inoxidable para reducir el uso de plástico.",
            ImagenUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=1200&q=80"
        },
        new()
        {
            Id = 2,
            Nombre = "Kit de reciclaje doméstico",
            Categoria = "Reciclaje",
            Precio = 89.00m,
            Stock = 45,
            Descripcion = "Set de contenedores compactos para separar residuos orgánicos, plásticos y papel.",
            ImagenUrl = "https://images.unsplash.com/photo-1532996122724-e3c354a0b15b?auto=format&fit=crop&w=1200&q=80"
        },
        new()
        {
            Id = 3,
            Nombre = "Panel solar portátil",
            Categoria = "Energía verde",
            Precio = 249.90m,
            Stock = 18,
            Descripcion = "Solución liviana para cargar dispositivos con energía renovable en viajes y actividades al aire libre.",
            ImagenUrl = "https://images.unsplash.com/photo-1509391366360-2e959784a276?auto=format&fit=crop&w=1200&q=80"
        },
        new()
        {
            Id = 4,
            Nombre = "Bolsa compostable premium",
            Categoria = "Consumo responsable",
            Precio = 19.50m,
            Stock = 250,
            Descripcion = "Alternativa biodegradable para compras diarias y empaques ecológicos.",
            ImagenUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=1200&q=80"
        }
    ];

    public IReadOnlyList<Producto> ObtenerTodos() => _productos.OrderBy(p => p.Nombre).ToList();

    public IReadOnlyList<Producto> ObtenerDestacados(int cantidad = 3) =>
        _productos.Where(p => p.Activo)
                 .OrderByDescending(p => p.Stock)
                 .Take(cantidad)
                 .ToList();

    public Producto? ObtenerPorId(int id) => _productos.FirstOrDefault(p => p.Id == id);
}

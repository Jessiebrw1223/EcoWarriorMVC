using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Producto> Destacados { get; set; } = [];
    public int TotalProductos { get; set; }
    public int TotalCategorias { get; set; }
    public int StockDisponible { get; set; }
}

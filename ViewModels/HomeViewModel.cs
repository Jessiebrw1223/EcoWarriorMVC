using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Producto> Destacados { get; set; } = [];
    public int TotalProductos { get; set; }
    public int TotalCategorias { get; set; }
    public int StockDisponible { get; set; }
    public EcoWeatherResponse? Clima { get; set; }
    public string CiudadConsulta { get; set; } = "Lima";
    public PerfilResponse? Perfil { get; set; }
    public IReadOnlyList<RetoResponse> Retos { get; set; } = [];
    public IReadOnlyList<RankingResponse> Ranking { get; set; } = [];
}

public class RetosViewModel
{
    public PerfilResponse? Perfil { get; set; }
    public IReadOnlyList<RetoResponse> Retos { get; set; } = [];
    public CrearRetoRequest NuevoReto { get; set; } = new();
}

public class PerfilViewModel
{
    public PerfilResponse? Perfil { get; set; }
    public PerfilEditRequest Editar { get; set; } = new();
    public IReadOnlyList<RetoResponse> RetosCompletados { get; set; } = [];
}

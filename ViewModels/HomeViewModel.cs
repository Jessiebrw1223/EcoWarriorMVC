using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Producto> Destacados    { get; set; } = [];
    public int TotalProductos                    { get; set; }
    public int TotalCategorias                   { get; set; }
    public int StockDisponible                   { get; set; }
    public EcoWeatherResponse? Clima             { get; set; }

    // Stats del usuario (desde BD, no mockup)
    public int PuntosUsuario                     { get; set; }
    public int RetosCompletados                  { get; set; }
    public double ReduccionCO2Kg                 { get; set; }
    public int NivelUsuario                      { get; set; }
    public int PuntosParaSiguienteNivel          { get; set; }
    public double PorcentajeProgreso             { get; set; }
}

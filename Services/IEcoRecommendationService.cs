using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IEcoRecommendationService
{
    string ObtenerMensajeEco(CurrentWeatherResponse climaActual);
    string ClasificarTexto(string texto);
    string RecomendarPorTexto(string texto);
    void Warmup();
}
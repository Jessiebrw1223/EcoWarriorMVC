using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IEcoRecommendationService
{
    string ObtenerMensajeEco(CurrentWeatherResponse climaActual);
}

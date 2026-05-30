using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IEcoRecommendationService
{
    string ObtenerMensajeEco(CurrentWeatherResponse climaActual);
    /// <summary>
    /// Inicializa (entrena) el modelo ML de forma explícita. Diseñado
    /// para llamadas de warmup en entornos de desarrollo.
    /// </summary>
    void Warmup();
}

using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IWeatherService
{
    Task<EcoWeatherResponse?> ObtenerClimaPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default);
}

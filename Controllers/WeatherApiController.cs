using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

/// <summary>
/// API REST del clima — usada por el JS del dashboard vía fetch().
/// GET /api/clima?ciudad=Lima  →  EcoWeatherResponse (JSON camelCase)
/// </summary>
[ApiController]
[Route("api/clima")]
public class WeatherApiController(
    IWeatherService weatherService,
    ILogger<WeatherApiController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerClima(
        [FromQuery] string? ciudad = null,
        CancellationToken ct = default)
    {
        var ciudad2 = (ciudad?.Trim() is { Length: > 0 } c) ? c : "Lima";
        logger.LogInformation("API clima solicitada para: {Ciudad}", ciudad2);

        var resultado = await weatherService.ObtenerClimaPorCiudadAsync(ciudad2, ct);

        if (resultado is null)
            return StatusCode(503, new { error = "Servicio de clima no disponible." });

        return Ok(resultado);
    }
}

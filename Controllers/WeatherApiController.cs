using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

[ApiController]
[Route("api/clima")]
public class WeatherApiController(IWeatherService weatherService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<EcoWeatherResponse>> ObtenerClima([FromQuery] string ciudad = "Lima", CancellationToken cancellationToken = default)
    {
        var resultado = await weatherService.ObtenerClimaPorCiudadAsync(ciudad, cancellationToken);
        if (resultado is null)
        {
            return NotFound(new OperacionResponse
            {
                Exito = false,
                Mensaje = "No se encontro informacion del clima para una ciudad de Peru."
            });
        }

        return Ok(resultado);
    }
}

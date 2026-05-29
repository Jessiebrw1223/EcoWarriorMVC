using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

[ApiController]
[Route("api/home")]
public class HomeApiController(IHomeService homeService) : ControllerBase
{
    [HttpGet("resumen")]
    public ActionResult<HomeResumenResponse> ObtenerResumen() =>
        Ok(homeService.ObtenerResumen());

    [HttpGet("retos")]
    public ActionResult<IReadOnlyList<RetoResponse>> ObtenerRetos() =>
        Ok(homeService.ObtenerRetos());

    [HttpGet("ranking")]
    public ActionResult<IReadOnlyList<RankingResponse>> ObtenerRanking() =>
        Ok(homeService.ObtenerRanking());

    [HttpGet("perfil")]
    public ActionResult<PerfilResponse> ObtenerPerfil([FromQuery] string? correo = null)
    {
        var perfil = homeService.ObtenerPerfil(correo);
        return perfil is null ? NotFound(new { mensaje = "Perfil no encontrado." }) : Ok(perfil);
    }

    [HttpPost("completar-reto")]
    public ActionResult<OperacionResponse> CompletarReto(
        [FromBody] CompletarRetoRequest req,
        [FromHeader(Name = "X-User-Correo")] string? correoHeader = null)
    {
        // Fallback: si no viene header, aceptar desde query
        var correo = correoHeader ?? Request.Query["correo"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correo))
            return BadRequest(new OperacionResponse { Exito = false, Mensaje = "Usuario no identificado." });

        var resultado = homeService.CompletarReto(correo, req.RetoId);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}

using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

/// <summary>
/// API del agente IA EcoBot (Semantic Kernel + LLM).
/// El frontend llama estos endpoints vía fetch() desde Index.cshtml.
/// </summary>
[ApiController]
[Route("api/ecobot")]
public class AiAgentApiController(IEcoAiAgentService agentService) : ControllerBase
{
    [HttpPost("consultar")]
    public async Task<ActionResult<EcoBotResponse>> Consultar(
        [FromBody] EcoBotRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Pregunta))
            return BadRequest(new { mensaje = "La pregunta no puede estar vacía." });

        var respuesta = await agentService.ConsultarAsync(
            request.Pregunta.Trim(), request.Ciudad, ct);

        return Ok(new EcoBotResponse { Respuesta = respuesta });
    }

    [HttpPost("reto-personalizado")]
    public async Task<ActionResult<EcoBotResponse>> GenerarReto(
        [FromBody] RetoRequest request,
        CancellationToken ct)
    {
        var json = await agentService.GenerarRetoPersonalizadoAsync(
            request.Puntos, request.CategoriaFavorita ?? "General", ct);

        return Ok(new EcoBotResponse { Respuesta = json });
    }
}

public sealed record EcoBotRequest(string Pregunta, string? Ciudad = null);
public sealed record RetoRequest(int Puntos, string? CategoriaFavorita = null);
public sealed record EcoBotResponse(string Respuesta);

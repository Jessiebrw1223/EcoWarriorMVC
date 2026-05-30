using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

/// <summary>
/// API del agente IA EcoBot (Semantic Kernel + LLM).
/// </summary>
[ApiController]
[Route("api/ecobot")]
public class AiAgentApiController : ControllerBase
{
    private readonly IEcoAiAgentService _agentService;

    public AiAgentApiController(IEcoAiAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("consultar")]
    public async Task<ActionResult<EcoBotResponse>> Consultar(
        [FromBody] EcoBotRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Pregunta))
        {
            return BadRequest(
                new EcoBotResponse(
                    "La pregunta no puede estar vacía."));
        }

        var respuesta = await _agentService.ConsultarAsync(
            request.Pregunta.Trim(),
            request.Ciudad,
            ct);

        return Ok(
            new EcoBotResponse(respuesta));
    }

    [HttpPost("reto-personalizado")]
    public async Task<ActionResult<EcoBotResponse>> GenerarReto(
        [FromBody] RetoRequest request,
        CancellationToken ct)
    {
        var json =
            await _agentService.GenerarRetoPersonalizadoAsync(
                request.Puntos,
                request.CategoriaFavorita ?? "General",
                ct);

        return Ok(
            new EcoBotResponse(json));
    }
}

public sealed record EcoBotRequest(
    string Pregunta,
    string? Ciudad = null);

public sealed record RetoRequest(
    int Puntos,
    string? CategoriaFavorita = null);

public sealed record EcoBotResponse(
    string Respuesta);
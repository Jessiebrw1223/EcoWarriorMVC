using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

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
        var pregunta = request.ObtenerPregunta();

        if (string.IsNullOrWhiteSpace(pregunta))
        {
            return Ok(new EcoBotResponse(
                "Hola 👋 Soy EcoBot IA. Puedo ayudarte con clima en Perú, retos, puntos, reciclaje, agua, energía, CO2 y hábitos sostenibles. ¿Qué te gustaría hacer hoy?"));
        }

        try
        {
            // La intención debe clasificarse usando SOLO la pregunta actual.
            // El historial se conserva en el frontend para continuidad visual, pero no se mezcla aquí
            // porque puede contaminar la clasificación de ML.NET y hacer que "puntos" se interprete como "clima".
            var respuesta = await _agentService.ConsultarAsync(
                pregunta.Trim(),
                request.Ciudad,
                ct);

            return Ok(new EcoBotResponse(respuesta));
        }
        catch
        {
            return Ok(new EcoBotResponse(
                "Estoy en modo local 🌱. Puedo seguir ayudándote con reciclaje, agua, energía, CO2, clima, retos y puntos. ¿Quieres una misión ecológica para hoy?"));
        }
    }

    [HttpPost("reto-personalizado")]
    public async Task<ActionResult<EcoBotResponse>> GenerarReto(
        [FromBody] RetoRequest request,
        CancellationToken ct)
    {
        try
        {
            var json = await _agentService.GenerarRetoPersonalizadoAsync(
                request.Puntos,
                request.CategoriaFavorita ?? "General",
                ct);

            return Ok(new EcoBotResponse(json));
        }
        catch
        {
            return Ok(new EcoBotResponse(
                "{\"titulo\":\"Reto Eco Rápido\",\"descripcion\":\"Evita plásticos de un solo uso durante 24 horas y separa tus residuos reciclables.\",\"puntos\":75,\"dificultad\":\"Media\",\"beneficio\":\"Reduce residuos y mejora tus hábitos sostenibles.\"}"));
        }
    }

    private static string ConstruirPreguntaConContexto(
        string pregunta,
        IReadOnlyList<EcoBotHistoryMessage>? historial)
    {
        if (historial is null || historial.Count == 0)
        {
            return pregunta;
        }

        var ultimos = historial
            .Where(x => !string.IsNullOrWhiteSpace(x.Texto))
            .TakeLast(6)
            .Select(x => $"{x.Rol}: {x.Texto}");

        return "Contexto reciente del chat:\n" +
               string.Join("\n", ultimos) +
               "\n\nPregunta actual del usuario:\n" +
               pregunta;
    }
}

public sealed record EcoBotRequest(
    string? Pregunta = null,
    string? Mensaje = null,
    string? Ciudad = null,
    IReadOnlyList<EcoBotHistoryMessage>? Historial = null)
{
    public string? ObtenerPregunta()
    {
        return !string.IsNullOrWhiteSpace(Pregunta)
            ? Pregunta
            : Mensaje;
    }
}

public sealed record EcoBotHistoryMessage(
    string Rol,
    string Texto);

public sealed record RetoRequest(
    int Puntos,
    string? CategoriaFavorita = null);

public sealed record EcoBotResponse(
    string Respuesta);

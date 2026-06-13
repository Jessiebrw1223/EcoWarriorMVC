using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcoWarriorMVC.Services;

public class EcoAiAgentService : IEcoAiAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService? _chat;
    private readonly bool _llmActivo;
    private readonly ILogger<EcoAiAgentService> _logger;
    private readonly IEcoRecommendationService _recommendationService;

    private const string SystemPrompt = """
Eres EcoBot, el agente IA conversacional de EcoWarrior, una plataforma ecológica para Perú.

Contexto de EcoWarrior:
- El usuario puede consultar clima por ciudad, provincia o departamento del Perú.
- El sistema recomienda acciones ecológicas usando ML.NET.
- El usuario puede completar retos o misiones para ganar puntos.
- El dashboard muestra puntos, retos completados, reducción de CO2 y ranking.
- Los temas principales son reciclaje, agua, energía, CO2, movilidad sostenible, residuos, productos ecoamigables, bebidas sostenibles, economía circular, clima y misiones.

Reglas de conversación:
- Responde siempre en español peruano, de forma natural, humana y profesional.
- No muestres etiquetas técnicas como "Clasificación: General" o "ML.NET" al usuario.
- Usa la categoría ML.NET solo como contexto interno.
- Máximo 3 párrafos cortos.
- Termina con una pregunta útil para continuar el flujo.
- Si el usuario pregunta algo fuera del tema, redirígelo amablemente hacia sostenibilidad.
- Si pregunta sobre comida, bebidas o negocios, responde desde enfoque ecológico: envases, residuos, ahorro de agua, energía, consumo responsable y reciclaje.
- No ayudes con delitos, fraude, lavado de dinero, violencia, drogas, armas ni actividades dañinas.
""";

    public EcoAiAgentService(
        IConfiguration config,
        ILogger<EcoAiAgentService> logger,
        IEcoRecommendationService recommendationService)
    {
        _logger = logger;
        _recommendationService = recommendationService;

        var apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
        var modelId = config["OpenAI:ModelId"] ?? "gpt-4o-mini";

        var builder = Kernel.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "YOUR_OPENAI_KEY_HERE")
        {
            builder.AddOpenAIChatCompletion(modelId, apiKey);
            _llmActivo = true;
        }
        else
        {
            _logger.LogWarning("No se configuró OpenAI API Key. EcoBot usará ML.NET + fallback conversacional gratuito.");
        }

        _kernel = builder.Build();
        CargarPluginEcoBot();

        _chat = _llmActivo
            ? _kernel.GetRequiredService<IChatCompletionService>()
            : null;

        _kernel.Plugins.AddFromObject(new EcoConsejosPlugin(), "EcoConsejos");
    }

    public async Task<string> ConsultarAsync(
        string pregunta,
        string? contextoCiudad = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pregunta))
        {
            return "Hola 👋 Soy EcoBot IA. Puedo ayudarte con clima en Perú, retos ecológicos, puntos, reciclaje, ahorro de agua, energía y reducción de CO2. ¿Qué te gustaría hacer hoy?";
        }

        var categoria = _recommendationService.ClasificarTexto(pregunta);
        var ciudad = string.IsNullOrWhiteSpace(contextoCiudad) ? "Lima" : contextoCiudad.Trim();

        if (_chat is null)
        {
            return _recommendationService.RecomendarPorTexto(pregunta);
        }

        try
        {
            var arguments = new KernelArguments
            {
                ["input"] = pregunta,
                ["categoria"] = categoria,
                ["ciudad"] = ciudad
            };

            var resultadoPlugin = await _kernel.InvokeAsync(
                "EcoBot",
                "ResponderConsulta",
                arguments,
                cancellationToken: ct);

            var respuestaPlugin = resultadoPlugin.GetValue<string>();

            if (!string.IsNullOrWhiteSpace(respuestaPlugin))
            {
                return LimpiarRespuesta(respuestaPlugin);
            }

            return await ConsultarConChatAsync(pregunta, categoria, ciudad, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error usando Plugin Semantic Kernel EcoBot. Se usará ML.NET conversacional.");
            return _recommendationService.RecomendarPorTexto(pregunta);
        }
    }

    private async Task<string> ConsultarConChatAsync(
        string pregunta,
        string categoria,
        string ciudad,
        CancellationToken ct)
    {
        if (_chat is null)
        {
            return _recommendationService.RecomendarPorTexto(pregunta);
        }

        var history = new ChatHistory(SystemPrompt);
        history.AddSystemMessage($"Ciudad o contexto del usuario: {ciudad}, Perú.");
        history.AddSystemMessage($"ML.NET clasificó la consulta como: {categoria}. No muestres esta etiqueta al usuario.");
        history.AddUserMessage(pregunta);

        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 420,
            Temperature = 0.75,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = await _chat.GetChatMessageContentAsync(
            history,
            settings,
            _kernel,
            ct);

        var respuesta = result.Content;

        return string.IsNullOrWhiteSpace(respuesta)
            ? _recommendationService.RecomendarPorTexto(pregunta)
            : LimpiarRespuesta(respuesta);
    }

    public async Task<string> GenerarRetoPersonalizadoAsync(
        int puntosUsuario,
        string categoriaFavorita,
        CancellationToken ct = default)
    {
        if (_chat is null)
        {
            return FallbackReto(categoriaFavorita);
        }

        try
        {
            var arguments = new KernelArguments
            {
                ["puntos"] = puntosUsuario.ToString(),
                ["categoria"] = categoriaFavorita,
                ["ciudad"] = "Lima"
            };

            var resultadoPlugin = await _kernel.InvokeAsync(
                "EcoBot",
                "GenerarReto",
                arguments,
                cancellationToken: ct);

            var respuesta = resultadoPlugin.GetValue<string>();

            return string.IsNullOrWhiteSpace(respuesta)
                ? FallbackReto(categoriaFavorita)
                : respuesta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reto con Semantic Kernel Plugin.");
            return FallbackReto(categoriaFavorita);
        }
    }

    private void CargarPluginEcoBot()
    {
        var pluginPath = Path.Combine(
            AppContext.BaseDirectory,
            "Plugins",
            "EcoBotPlugin");

        if (!Directory.Exists(pluginPath))
        {
            _logger.LogWarning("No se encontró el plugin EcoBot en: {PluginPath}", pluginPath);
            return;
        }

        _kernel.ImportPluginFromPromptDirectory(pluginPath, "EcoBot");
        _logger.LogInformation("Plugin Semantic Kernel EcoBot cargado");
    }

    private static string FallbackReto(string categoria)
    {
        var reto = new
        {
            titulo = "Reto Eco del Día",
            descripcion = $"Realiza una acción sostenible relacionada con {categoria}.",
            puntos = 75,
            dificultad = "Media",
            beneficio = "Reduce tu impacto ambiental diario."
        };

        return JsonSerializer.Serialize(reto);
    }

    private static string LimpiarRespuesta(string respuesta)
    {
        return respuesta
            .Replace("Clasificación:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("ML.NET", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
}

internal sealed class EcoConsejosPlugin
{
    [KernelFunction("obtener_equivalencia_co2")]
    public string ObtenerEquivalenciaCO2(double kg)
    {
        var arboles = Math.Round(kg / 21.77, 1);
        var kmAuto = Math.Round(kg / 0.12, 0);

        return $"{kg} kg de CO2 equivalen a {arboles} árboles plantados o {kmAuto} km menos en automóvil.";
    }

    [KernelFunction("obtener_consejo_por_clima")]
    public string ObtenerConsejoPorClima(string clima)
    {
        clima = clima.ToLowerInvariant();

        if (clima.Contains("lluvia"))
        {
            return "Aprovecha el agua de lluvia para regar plantas y evita traslados innecesarios.";
        }

        if (clima.Contains("sol"))
        {
            return "Seca ropa al sol, usa botella reutilizable y aprovecha la luz natural.";
        }

        if (clima.Contains("viento"))
        {
            return "Ventila naturalmente tu hogar y evita usar equipos eléctricos innecesarios.";
        }

        return "Mantén hábitos sostenibles: recicla, ahorra agua y reduce el consumo eléctrico.";
    }
}

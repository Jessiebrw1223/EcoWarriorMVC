using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Agente IA con Semantic Kernel + GPT-4o-mini.
/// Actúa como asesor ecológico experto en el contexto peruano.
/// </summary>
public class EcoAiAgentService : IEcoAiAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService? _chat;
    private readonly bool _llmActivo;
    private readonly ILogger<EcoAiAgentService> _logger;

    private const string SystemPrompt = """
Eres EcoBot, un agente experto en sostenibilidad ambiental con enfoque en Perú.

Tu misión es ayudar a los usuarios de EcoWarrior a reducir su huella de carbono.

Reglas:
- Responde siempre en español peruano.
- Sé amigable y motivador.
- Máximo 3 párrafos cortos.
- Incluye datos ecológicos relevantes cuando sea útil.
- Adapta consejos al clima del Perú.
""";

    public EcoAiAgentService(
        IConfiguration config,
        ILogger<EcoAiAgentService> logger)
    {
        _logger = logger;

        var apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
        var modelId = config["OpenAI:ModelId"] ?? "gpt-4o-mini";

        var builder = Kernel.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(apiKey) &&
            apiKey != "YOUR_OPENAI_KEY_HERE")
        {
            builder.AddOpenAIChatCompletion(modelId, apiKey);
            _llmActivo = true;
        }
        else
        {
            _logger.LogWarning(
                "No se configuró OpenAI API Key. Se usarán respuestas fallback.");
        }

        _kernel = builder.Build();

        _chat = _llmActivo
            ? _kernel.GetRequiredService<IChatCompletionService>()
            : null;

        _kernel.Plugins.AddFromObject(
            new EcoConsejosPlugin(),
            "EcoConsejos");
    }

    /// <summary>
    /// Consulta libre al agente IA.
    /// </summary>
    public async Task<string> ConsultarAsync(
        string pregunta,
        string? contextoCiudad = null,
        CancellationToken ct = default)
    {
        if (_chat is null)
        {
            return RespuestaFallback();
        }

        try
        {
            var history = new ChatHistory(SystemPrompt);

            if (!string.IsNullOrWhiteSpace(contextoCiudad))
            {
                history.AddSystemMessage(
                    $"El usuario está en {contextoCiudad}, Perú.");
            }

            history.AddUserMessage(pregunta);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 400,
                Temperature = 0.7,
                ToolCallBehavior =
                    ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result =
                await _chat.GetChatMessageContentAsync(
                    history,
                    settings,
                    _kernel,
                    ct);

            return result.Content
                   ?? RespuestaFallback();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error en EcoAiAgentService.");

            return RespuestaFallback();
        }
    }

    /// <summary>
    /// Genera un reto ecológico personalizado.
    /// </summary>
    public async Task<string> GenerarRetoPersonalizadoAsync(
        int puntosUsuario,
        string categoriaFavorita,
        CancellationToken ct = default)
    {
        var nivel = puntosUsuario switch
        {
            >= 3000 => "Leyenda eco",
            >= 2000 => "Guardián verde",
            >= 1000 => "Agente sostenible",
            _ => "Nuevo recluta"
        };

        if (_chat is null)
        {
            return FallbackReto(categoriaFavorita);
        }

        var prompt =
            "Crea UN reto ecológico personalizado.\n\n" +
            $"Nivel del usuario: {nivel}\n" +
            $"Puntos: {puntosUsuario}\n" +
            $"Categoría favorita: {categoriaFavorita}\n\n" +
            "Debe incluir:\n" +
            "- titulo\n" +
            "- descripcion\n" +
            "- puntos\n" +
            "- dificultad\n\n" +
            "Devuelve SOLO JSON válido.";

        try
        {
            var history = new ChatHistory(SystemPrompt);

            history.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 200,
                Temperature = 0.8
            };

            var result =
                await _chat.GetChatMessageContentAsync(
                    history,
                    settings,
                    _kernel,
                    ct);

            return result.Content
                   ?? FallbackReto(categoriaFavorita);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generando reto.");

            return FallbackReto(categoriaFavorita);
        }
    }

    private static string RespuestaFallback()
    {
        return
            "Hola, soy EcoBot 🌱. " +
            "Usar transporte público, reciclar y reducir plásticos " +
            "ayuda muchísimo al planeta.";
    }

    private static string FallbackReto(string categoria)
    {
        return
            "{ " +
            "\"titulo\": \"Reto Eco del Día\", " +
            "\"descripcion\": \"Realiza una acción sostenible relacionada con " + categoria + ".\", " +
            "\"puntos\": 75, " +
            "\"dificultad\": \"Media\" " +
            "}";
    }
}

/// <summary>
/// Plugin ecológico para Semantic Kernel.
/// </summary>
internal sealed class EcoConsejosPlugin
{
    [KernelFunction("obtener_equivalencia_co2")]
    public string ObtenerEquivalenciaCO2(double kg)
    {
        var arboles = Math.Round(kg / 21.77, 1);

        var kmAuto = Math.Round(kg / 0.12, 0);

        return
            $"{kg} kg de CO2 equivalen a " +
            $"{arboles} árboles plantados o " +
            $"{kmAuto} km menos en automóvil.";
    }

    [KernelFunction("obtener_consejo_por_clima")]
    public string ObtenerConsejoPorClima(string clima)
    {
        clima = clima.ToLowerInvariant();

        if (clima.Contains("lluvia"))
        {
            return
                "Aprovecha el agua de lluvia para regar plantas.";
        }

        if (clima.Contains("sol"))
        {
            return
                "Seca ropa al sol y ahorra electricidad.";
        }

        if (clima.Contains("viento"))
        {
            return
                "Ventila naturalmente tu hogar.";
        }

        return
            "Mantén hábitos sostenibles todos los días.";
    }
};
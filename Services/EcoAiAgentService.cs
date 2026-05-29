using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Agente IA con Semantic Kernel + GPT-4o-mini.
/// Actúa como asesor ecológico experto en el contexto peruano.
/// Usa funciones semánticas (prompts) + plugins nativos.
/// </summary>
public class EcoAiAgentService : IEcoAiAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ILogger<EcoAiAgentService> _logger;

    private const string SystemPrompt = """
        Eres EcoBot, un agente experto en sostenibilidad ambiental con enfoque en Perú.
        Tu misión es ayudar a los usuarios de EcoWarrior a reducir su huella de carbono.
        
        Reglas:
        - Responde siempre en español peruano, de manera amigable y motivadora.
        - Sé conciso: máximo 3 párrafos cortos por respuesta.
        - Incluye datos o estadísticas cuando sea relevante (CO2, agua, energía).
        - Adapta consejos al contexto climático de Perú (costa, sierra, selva).
        - Si no sabes algo, dilo claramente y sugiere recursos confiables.
        """;

    public EcoAiAgentService(IConfiguration config, ILogger<EcoAiAgentService> logger)
    {
        _logger = logger;
        var apiKey  = config["OpenAI:ApiKey"] ?? string.Empty;
        var modelId = config["OpenAI:ModelId"] ?? "gpt-4o-mini";

        var builder = Kernel.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "YOUR_OPENAI_KEY_HERE")
        {
            builder.AddOpenAIChatCompletion(modelId, apiKey);
        }
        else
        {
            // Modo demo sin API key real: usar implementación stub
            _logger.LogWarning("OpenAI API key no configurada. EcoAiAgent usará respuestas predefinidas.");
        }

        _kernel = builder.Build();
        _chat   = _kernel.GetRequiredService<IChatCompletionService>();

        // Registrar plugin nativo de eco-consejos
        _kernel.Plugins.AddFromObject(new EcoConsejosPlugin(), "EcoConsejos");
    }

    /// <summary>Consulta libre al agente eco.</summary>
    public async Task<string> ConsultarAsync(
        string pregunta,
        string? contextoCiudad = null,
        CancellationToken ct = default)
    {
        try
        {
            var history = new ChatHistory(SystemPrompt);

            if (!string.IsNullOrWhiteSpace(contextoCiudad))
                history.AddSystemMessage($"El usuario está en: {contextoCiudad}, Perú.");

            history.AddUserMessage(pregunta);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens        = 400,
                Temperature      = 0.7,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await _chat.GetChatMessageContentAsync(
                history, settings, _kernel, ct);

            return result.Content ?? RespuestaFallback(pregunta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en EcoAiAgent.ConsultarAsync.");
            return RespuestaFallback(pregunta);
        }
    }

    /// <summary>Genera un reto personalizado según perfil del usuario.</summary>
    public async Task<string> GenerarRetoPersonalizadoAsync(
        int puntosUsuario,
        string categoriaFavorita,
        CancellationToken ct = default)
    {
        var nivel = puntosUsuario switch
        {
            >= 3000 => "Leyenda eco (experto avanzado)",
            >= 2000 => "Guardián verde (nivel intermedio-alto)",
            >= 1000 => "Agente sostenible (nivel intermedio)",
            _       => "Nuevo recluta (principiante)"
        };

        var prompt = $"""
            Crea UN reto ecológico personalizado para un usuario EcoWarrior.
            
            Perfil del usuario:
            - Nivel: {nivel} ({puntosUsuario} puntos)
            - Categoría favorita: {categoriaFavorita}
            
            El reto debe:
            1. Ser alcanzable en 1-7 días
            2. Estar adaptado al nivel del usuario
            3. Incluir: Título, Descripción (2 oraciones), Puntos estimados (50-200)
            4. Formato JSON: {{"titulo":"...","descripcion":"...","puntos":N,"dificultad":"Fácil|Media|Alta"}}
            
            Responde SOLO con el JSON, sin markdown ni explicaciones.
            """;

        try
        {
            var history = new ChatHistory(SystemPrompt);
            history.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens   = 200,
                Temperature = 0.8
            };

            var result = await _chat.GetChatMessageContentAsync(
                history, settings, _kernel, ct);

            return result.Content ?? FallbackReto(categoriaFavorita);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reto personalizado.");
            return FallbackReto(categoriaFavorita);
        }
    }

    private static string RespuestaFallback(string pregunta) =>
        "¡Hola! Soy EcoBot. Ahora mismo tengo problemas de conexión, pero puedo decirte que " +
        "pequeñas acciones diarias como usar bolsas reutilizables, reducir el uso de plástico " +
        "y optar por transporte público hacen una gran diferencia para nuestro planeta. 🌱";

    private static string FallbackReto(string categoria) =>
        $$$"""{"titulo":"Reto Eco del Día","descripcion":"Realiza una acción sostenible relacionada con {{{categoria}}}. Documenta tu progreso y compártelo.","puntos":75,"dificultad":"Media"}""";
}

/// <summary>
/// Plugin nativo de Semantic Kernel: funciones que el agente puede invocar.
/// </summary>
internal sealed class EcoConsejosPlugin
{
    [KernelFunction("obtener_equivalencia_co2")]
    [System.ComponentModel.Description("Calcula equivalencias de CO2 ahorrado en términos cotidianos")]
    public string ObtenerEquivalenciaCO2(
        [System.ComponentModel.Description("Kilogramos de CO2 ahorrados")]
        double kg)
    {
        var arboles    = Math.Round(kg / 21.77, 1);   // un árbol absorbe ~21.77 kg/año
        var kmEnAuto   = Math.Round(kg / 0.12, 0);    // ~120g CO2/km en auto promedio
        var horasLed   = Math.Round(kg / 0.01, 0);    // una bombilla LED 10W = ~10g/hora

        return $"{kg} kg de CO2 equivale a: plantar {arboles} árboles, " +
               $"ahorrar {kmEnAuto} km en auto, o {horasLed} horas de bombilla LED apagada.";
    }

    [KernelFunction("obtener_consejo_por_clima")]
    [System.ComponentModel.Description("Da un consejo eco específico según condición climática")]
    public string ObtenerConsejoPorClima(
        [System.ComponentModel.Description("Descripción del clima actual")]
        string clima)
    {
        return clima.ToLowerInvariant() switch
        {
            var c when c.Contains("lluvia") || c.Contains("llovizna") =>
                "Aprovecha el agua de lluvia para regar plantas. Coloca recipientes en balcones.",
            var c when c.Contains("sol") || c.Contains("despejado") =>
                "Ideal para secar ropa al sol (sin secadora) y cargar dispositivos con energía solar.",
            var c when c.Contains("viento") =>
                "El viento natural puede ventilar tu hogar. Apaga el aire acondicionado y abre ventanas.",
            var c when c.Contains("niebla") =>
                "La niebla reduce la radiación UV. Reduce el uso de ventiladores y aprovecha la temperatura natural.",
            _ => "Mantén hábitos sostenibles independientemente del clima: recicla, reutiliza y reduce."
        };
    }
}

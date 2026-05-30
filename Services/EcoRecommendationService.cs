using EcoWarriorMVC.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Servicio ML.NET para recomendaciones ecológicas inteligentes.
/// El modelo se entrena de forma Lazy, una sola vez, usando datos sintéticos.
/// No depende de archivos CSV ni ZIP, por eso funciona bien en Render.
/// </summary>
public class EcoRecommendationService : IEcoRecommendationService
{
    private readonly Lazy<(MLContext Ctx, ITransformer Model, DataViewSchema Schema)> _lazy;
    private readonly ILogger<EcoRecommendationService>? _logger;
    private readonly Random _rng = new();

    public EcoRecommendationService(ILogger<EcoRecommendationService>? logger = null)
    {
        _logger = logger;

        _lazy = new Lazy<(MLContext, ITransformer, DataViewSchema)>(
            EntrenarModelo,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string ObtenerMensajeEco(CurrentWeatherResponse climaActual)
    {
        try
        {
            var (ctx, model, schema) = _lazy.Value;

            // PredictionEngine no es thread-safe, por eso se crea por llamada.
            var engine = ctx.Model.CreatePredictionEngine<ClimaEntrada, RecomendacionSalida>(
                model,
                inputSchema: schema);

            var prediccion = engine.Predict(new ClimaEntrada
            {
                Temperatura = (float)climaActual.Temperatura,
                Humedad = (float)climaActual.Humedad,
                Viento = (float)climaActual.VelocidadViento,
                Precipitacion = (float)climaActual.Precipitacion,
                CodigoClima = climaActual.CodigoClima
            });

            return MensajePorCategoria(prediccion.Categoria);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error ejecutando recomendación ML.NET.");
            return MensajePorCategoria("Default");
        }
    }

    public void Warmup()
    {
        try
        {
            // Forzar la evaluación lazy para entrenar el modelo ahora.
            _ = _lazy.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Warmup ML.NET falló.");
        }
    }

    private static (MLContext, ITransformer, DataViewSchema) EntrenarModelo()
    {
        var ctx = new MLContext(seed: 42);
        var datos = ObtenerDatosEntrenamiento();
        var dataView = ctx.Data.LoadFromEnumerable(datos);
        var inputSchema = dataView.Schema;

        var pipeline = ctx.Transforms.Conversion.MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(ClimaEntrada.Categoria))
            .Append(ctx.Transforms.Concatenate(
                "Features",
                nameof(ClimaEntrada.Temperatura),
                nameof(ClimaEntrada.Humedad),
                nameof(ClimaEntrada.Viento),
                nameof(ClimaEntrada.Precipitacion),
                nameof(ClimaEntrada.CodigoClima)))
            .Append(ctx.Transforms.NormalizeMinMax("Features"))
            .Append(ctx.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                maximumNumberOfIterations: 100))
            .Append(ctx.Transforms.Conversion.MapKeyToValue(
                outputColumnName: nameof(RecomendacionSalida.Categoria),
                inputColumnName: "PredictedLabel"));

        var model = pipeline.Fit(dataView);
        return (ctx, model, inputSchema);
    }

    private static List<ClimaEntrada> ObtenerDatosEntrenamiento() =>
    [
        // Movilidad sostenible: clima templado, poco viento, sin lluvia.
        new() { Temperatura = 18, Humedad = 50, Viento = 5,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 20, Humedad = 55, Viento = 7,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 21, Humedad = 58, Viento = 6,  Precipitacion = 0, CodigoClima = 1, Categoria = "Movilidad" },
        new() { Temperatura = 22, Humedad = 60, Viento = 8,  Precipitacion = 0, CodigoClima = 1, Categoria = "Movilidad" },
        new() { Temperatura = 23, Humedad = 55, Viento = 9,  Precipitacion = 0, CodigoClima = 2, Categoria = "Movilidad" },
        new() { Temperatura = 24, Humedad = 58, Viento = 9,  Precipitacion = 0, CodigoClima = 2, Categoria = "Movilidad" },
        new() { Temperatura = 25, Humedad = 52, Viento = 6,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 26, Humedad = 50, Viento = 6,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },

        // Ahorro de energía: nublado/frío/viento.
        new() { Temperatura = 8,  Humedad = 85, Viento = 18, Precipitacion = 0, CodigoClima = 3,  Categoria = "Energia" },
        new() { Temperatura = 10, Humedad = 80, Viento = 20, Precipitacion = 0, CodigoClima = 3,  Categoria = "Energia" },
        new() { Temperatura = 12, Humedad = 78, Viento = 22, Precipitacion = 0, CodigoClima = 45, Categoria = "Energia" },
        new() { Temperatura = 14, Humedad = 75, Viento = 24, Precipitacion = 0, CodigoClima = 48, Categoria = "Energia" },
        new() { Temperatura = 16, Humedad = 78, Viento = 28, Precipitacion = 0, CodigoClima = 45, Categoria = "Energia" },
        new() { Temperatura = 18, Humedad = 70, Viento = 30, Precipitacion = 0, CodigoClima = 48, Categoria = "Energia" },
        new() { Temperatura = 19, Humedad = 66, Viento = 25, Precipitacion = 0, CodigoClima = 3,  Categoria = "Energia" },

        // Interior: lluvia/tormenta/precipitación.
        new() { Temperatura = 9,  Humedad = 98, Viento = 15, Precipitacion = 10, CodigoClima = 61, Categoria = "Interior" },
        new() { Temperatura = 11, Humedad = 96, Viento = 18, Precipitacion = 20, CodigoClima = 63, Categoria = "Interior" },
        new() { Temperatura = 12, Humedad = 95, Viento = 20, Precipitacion = 12, CodigoClima = 61, Categoria = "Interior" },
        new() { Temperatura = 13, Humedad = 94, Viento = 30, Precipitacion = 20, CodigoClima = 95, Categoria = "Interior" },
        new() { Temperatura = 14, Humedad = 92, Viento = 22, Precipitacion = 15, CodigoClima = 63, Categoria = "Interior" },
        new() { Temperatura = 15, Humedad = 90, Viento = 18, Precipitacion = 8,  CodigoClima = 65, Categoria = "Interior" },
        new() { Temperatura = 16, Humedad = 88, Viento = 16, Precipitacion = 7,  CodigoClima = 80, Categoria = "Interior" },

        // Hidratación: temperaturas altas.
        new() { Temperatura = 28, Humedad = 60, Viento = 4,  Precipitacion = 0, CodigoClima = 0, Categoria = "Hidratacion" },
        new() { Temperatura = 30, Humedad = 70, Viento = 5,  Precipitacion = 0, CodigoClima = 0, Categoria = "Hidratacion" },
        new() { Temperatura = 31, Humedad = 72, Viento = 6,  Precipitacion = 0, CodigoClima = 1, Categoria = "Hidratacion" },
        new() { Temperatura = 32, Humedad = 75, Viento = 4,  Precipitacion = 0, CodigoClima = 1, Categoria = "Hidratacion" },
        new() { Temperatura = 33, Humedad = 78, Viento = 5,  Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" },
        new() { Temperatura = 34, Humedad = 80, Viento = 6,  Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" },
        new() { Temperatura = 35, Humedad = 80, Viento = 6,  Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" },

        // Casos variados para robustez (mezcla y extremos)
        new() { Temperatura = 5,  Humedad = 90, Viento = 10, Precipitacion = 0, CodigoClima = 71, Categoria = "Interior" },
        new() { Temperatura = 27, Humedad = 50, Viento = 12, Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 3,  Humedad = 85, Viento = 35, Precipitacion = 0, CodigoClima = 95, Categoria = "Interior" },
        new() { Temperatura = 29, Humedad = 65, Viento = 10, Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" }
    ];

    private static readonly Dictionary<string, string[]> _plantillas = new()
    {
        ["Interior"] = new[]
        {
            "🏠 Buen momento para actividades bajo techo. Aprovecha para organizar residuos, reutilizar materiales y ahorrar electricidad.",
            "🌧️ Está lluvioso afuera — ideal para actividades interiores: lee sobre reciclaje y planifica acciones sostenibles.",
            "🛋️ Día para quedarse dentro. Aprovecha para reducir consumo eléctrico con gestos simples y reutilizar materiales."
        },
        ["Movilidad"] = new[]
        {
            "🚲 Clima ideal para caminar, usar bicicleta o transporte público. Hoy puedes reducir tu huella de carbono.",
            "🚶 Aprovecha el buen tiempo: una caminata corta reemplaza viajes en coche y suma salud al planeta.",
            "🛴 Considera compartir transporte o bicicleta: menos emisiones y más vida en la ciudad." 
        },
        ["Energia"] = new[]
        {
            "💨 Aprovecha la ventilación natural y evita usar equipos eléctricos innecesarios. Cada kWh ahorrado cuenta.",
            "🔌 Evita picos de consumo: desconecta cargadores y ajusta termostatos para ahorrar energía.",
            "💡 Usa iluminación eficiente y apaga lo que no uses — pequeño cambio, gran impacto." 
        },
        ["Hidratacion"] = new[]
        {
            "☀️ Temperatura alta: hidrátate con botella reutilizable y evita plásticos de un solo uso.",
            "💧 Mantente hidratado y busca sombra. Lleva tu propia botella reutilizable para reducir residuos.",
            "🌞 Evita actividades muy intensas al sol y recuerda proteger el entorno evitando plásticos desechables." 
        },
        ["Default"] = new[]
        {
            "🌱 Mantén hábitos sostenibles: recicla, reutiliza, ahorra agua y reduce tu consumo energético.",
            "♻️ Pequeños gestos diarios suman: separa residuos y piensa en la reutilización antes de comprar.",
            "🌿 Considera acciones locales: participa en limpiezas y promueve la movilidad activa." 
        }
    };

    private string MensajePorCategoria(string? categoria)
    {
        var key = string.IsNullOrWhiteSpace(categoria) ? "Default" : categoria;
        if (!_plantillas.TryGetValue(key, out var opciones))
        {
            opciones = _plantillas["Default"];
        }

        int idx;
        lock (_rng)
        {
            idx = _rng.Next(opciones.Length);
        }

        return opciones[idx];
    }

    private sealed class ClimaEntrada
    {
        public float Temperatura { get; set; }
        public float Humedad { get; set; }
        public float Viento { get; set; }
        public float Precipitacion { get; set; }
        public float CodigoClima { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }

    private sealed class RecomendacionSalida
    {
        [ColumnName("PredictedLabel")]
        public string Categoria { get; set; } = string.Empty;
    }
}

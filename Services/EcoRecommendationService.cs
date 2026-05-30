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
        new() { Temperatura = 20, Humedad = 55, Viento = 7,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 22, Humedad = 60, Viento = 8,  Precipitacion = 0, CodigoClima = 1, Categoria = "Movilidad" },
        new() { Temperatura = 24, Humedad = 58, Viento = 9,  Precipitacion = 0, CodigoClima = 2, Categoria = "Movilidad" },
        new() { Temperatura = 26, Humedad = 50, Viento = 6,  Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },

        // Ahorro de energía: nublado/frío/viento.
        new() { Temperatura = 14, Humedad = 75, Viento = 24, Precipitacion = 0, CodigoClima = 3,  Categoria = "Energia" },
        new() { Temperatura = 16, Humedad = 78, Viento = 28, Precipitacion = 0, CodigoClima = 45, Categoria = "Energia" },
        new() { Temperatura = 18, Humedad = 70, Viento = 30, Precipitacion = 0, CodigoClima = 48, Categoria = "Energia" },
        new() { Temperatura = 19, Humedad = 66, Viento = 25, Precipitacion = 0, CodigoClima = 3,  Categoria = "Energia" },

        // Interior: lluvia/tormenta/precipitación.
        new() { Temperatura = 12, Humedad = 95, Viento = 20, Precipitacion = 12, CodigoClima = 61, Categoria = "Interior" },
        new() { Temperatura = 14, Humedad = 92, Viento = 22, Precipitacion = 15, CodigoClima = 63, Categoria = "Interior" },
        new() { Temperatura = 15, Humedad = 90, Viento = 18, Precipitacion = 8,  CodigoClima = 65, Categoria = "Interior" },
        new() { Temperatura = 17, Humedad = 88, Viento = 16, Precipitacion = 7,  CodigoClima = 80, Categoria = "Interior" },
        new() { Temperatura = 13, Humedad = 94, Viento = 30, Precipitacion = 20, CodigoClima = 95, Categoria = "Interior" },

        // Hidratación: temperaturas altas.
        new() { Temperatura = 30, Humedad = 70, Viento = 5, Precipitacion = 0, CodigoClima = 0, Categoria = "Hidratacion" },
        new() { Temperatura = 32, Humedad = 75, Viento = 4, Precipitacion = 0, CodigoClima = 1, Categoria = "Hidratacion" },
        new() { Temperatura = 35, Humedad = 80, Viento = 6, Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" }
    ];

    private static string MensajePorCategoria(string? categoria) => categoria switch
    {
        "Interior" =>
            "🏠 Buen momento para actividades bajo techo. Aprovecha para organizar residuos, reutilizar materiales y ahorrar electricidad.",

        "Movilidad" =>
            "🚲 Clima ideal para caminar, usar bicicleta o transporte público. Hoy puedes reducir tu huella de carbono.",

        "Energia" =>
            "💨 Aprovecha la ventilación natural y evita usar equipos eléctricos innecesarios. Cada kWh ahorrado cuenta.",

        "Hidratacion" =>
            "☀️ Temperatura alta: hidrátate con botella reutilizable y evita plásticos de un solo uso.",

        _ =>
            "🌱 Mantén hábitos sostenibles: recicla, reutiliza, ahorra agua y reduce tu consumo energético."
    };

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

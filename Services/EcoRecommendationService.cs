using EcoWarriorMVC.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Servicio ML.NET: clasifica condiciones climáticas en categorías eco
/// usando SDCA Multiclass con features enriquecidas (temperatura, humedad,
/// viento, precipitación, código de clima).
/// Modelo entrenado lazy + thread-safe con Lazy<T>.
/// </summary>
public class EcoRecommendationService : IEcoRecommendationService
{
    private readonly Lazy<(MLContext Ctx, ITransformer Model, DataViewSchema Schema)> _lazy;
    private readonly ILogger<EcoRecommendationService>? _logger;

    public EcoRecommendationService(ILogger<EcoRecommendationService>? logger = null)
    {
        _logger = logger;
        _lazy = new Lazy<(MLContext, ITransformer, DataViewSchema)>(
            EntrenarModelo, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string ObtenerMensajeEco(CurrentWeatherResponse clima)
    {
        try
        {
            var (ctx, model, schema) = _lazy.Value;

            // PredictionEngine NO es thread-safe → crear por invocación
            // (en producción, usar PredictionEnginePool<>)
            var engine = ctx.Model.CreatePredictionEngine<ClimaEntrada, RecomendacionSalida>(
                model, inputSchema: schema);

            var prediccion = engine.Predict(new ClimaEntrada
            {
                Temperatura   = (float)clima.Temperatura,
                Humedad       = (float)clima.Humedad,
                Viento        = (float)clima.VelocidadViento,
                Precipitacion = (float)clima.Precipitacion,
                CodigoClima   = clima.CodigoClima
            });

            return MensajePorCategoria(prediccion.Categoria ?? "Default");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en predicción ML.NET.");
            return MensajePorCategoria("Default");
        }
    }

    // ──────────────────────────────────────────────────────────────
    // ENTRENAMIENTO (datos sintéticos enriquecidos para Lima/Perú)
    // ──────────────────────────────────────────────────────────────

    private static (MLContext, ITransformer, DataViewSchema) EntrenarModelo()
    {
        var ctx = new MLContext(seed: 42);

        var datos = new List<ClimaEntrada>
        {
            // MOVILIDAD SOSTENIBLE – días soleados y cálidos sin lluvia
            new() { Temperatura=24, Humedad=55, Viento=8,  Precipitacion=0,   CodigoClima=0,  Categoria="MovilidadSostenible" },
            new() { Temperatura=22, Humedad=58, Viento=10, Precipitacion=0,   CodigoClima=1,  Categoria="MovilidadSostenible" },
            new() { Temperatura=20, Humedad=62, Viento=9,  Precipitacion=0,   CodigoClima=2,  Categoria="MovilidadSostenible" },
            new() { Temperatura=26, Humedad=50, Viento=7,  Precipitacion=0,   CodigoClima=0,  Categoria="MovilidadSostenible" },
            new() { Temperatura=23, Humedad=60, Viento=11, Precipitacion=0,   CodigoClima=1,  Categoria="MovilidadSostenible" },
            new() { Temperatura=21, Humedad=65, Viento=8,  Precipitacion=0,   CodigoClima=0,  Categoria="MovilidadSostenible" },
            new() { Temperatura=25, Humedad=52, Viento=6,  Precipitacion=0,   CodigoClima=2,  Categoria="MovilidadSostenible" },

            // AHORRO ENERGIA – nublado, viento alto, sin lluvia
            new() { Temperatura=18, Humedad=70, Viento=25, Precipitacion=0,   CodigoClima=3,  Categoria="AhorroEnergia" },
            new() { Temperatura=16, Humedad=75, Viento=30, Precipitacion=0,   CodigoClima=3,  Categoria="AhorroEnergia" },
            new() { Temperatura=19, Humedad=68, Viento=28, Precipitacion=0,   CodigoClima=1,  Categoria="AhorroEnergia" },
            new() { Temperatura=17, Humedad=72, Viento=32, Precipitacion=0,   CodigoClima=2,  Categoria="AhorroEnergia" },
            new() { Temperatura=20, Humedad=66, Viento=27, Precipitacion=0,   CodigoClima=3,  Categoria="AhorroEnergia" },
            new() { Temperatura=15, Humedad=78, Viento=35, Precipitacion=0,   CodigoClima=45, Categoria="AhorroEnergia" },

            // INTERIOR – lluvia, tormenta, condiciones adversas
            new() { Temperatura=16, Humedad=90, Viento=18, Precipitacion=5,   CodigoClima=61, Categoria="Interior" },
            new() { Temperatura=15, Humedad=92, Viento=20, Precipitacion=10,  CodigoClima=63, Categoria="Interior" },
            new() { Temperatura=14, Humedad=95, Viento=22, Precipitacion=15,  CodigoClima=65, Categoria="Interior" },
            new() { Temperatura=17, Humedad=88, Viento=16, Precipitacion=8,   CodigoClima=80, Categoria="Interior" },
            new() { Temperatura=18, Humedad=85, Viento=14, Precipitacion=6,   CodigoClima=81, Categoria="Interior" },
            new() { Temperatura=13, Humedad=72, Viento=30, Precipitacion=0,   CodigoClima=95, Categoria="Interior" },
            new() { Temperatura=12, Humedad=74, Viento=32, Precipitacion=12,  CodigoClima=96, Categoria="Interior" },
            new() { Temperatura=11, Humedad=96, Viento=25, Precipitacion=20,  CodigoClima=99, Categoria="Interior" },
            new() { Temperatura=14, Humedad=91, Viento=19, Precipitacion=7,   CodigoClima=82, Categoria="Interior" },

            // HIDRATACION – temperaturas muy altas (costa norte Perú)
            new() { Temperatura=32, Humedad=80, Viento=5,  Precipitacion=0,   CodigoClima=0,  Categoria="Hidratacion" },
            new() { Temperatura=35, Humedad=75, Viento=4,  Precipitacion=0,   CodigoClima=1,  Categoria="Hidratacion" },
            new() { Temperatura=30, Humedad=85, Viento=6,  Precipitacion=0,   CodigoClima=0,  Categoria="Hidratacion" },
            new() { Temperatura=33, Humedad=78, Viento=3,  Precipitacion=0,   CodigoClima=2,  Categoria="Hidratacion" },
        };

        var dataView = ctx.Data.LoadFromEnumerable(datos);
        var inputSchema = dataView.Schema;

        var pipeline = ctx.Transforms
            .Conversion.MapValueToKey("Label", nameof(ClimaEntrada.Categoria))
            .Append(ctx.Transforms.Concatenate("Features",
                nameof(ClimaEntrada.Temperatura),
                nameof(ClimaEntrada.Humedad),
                nameof(ClimaEntrada.Viento),
                nameof(ClimaEntrada.Precipitacion),
                nameof(ClimaEntrada.CodigoClima)))
            .Append(ctx.Transforms.NormalizeMinMax("Features"))
            .Append(ctx.MulticlassClassification.Trainers
                .SdcaMaximumEntropy(maximumNumberOfIterations: 100))
            .Append(ctx.Transforms.Conversion.MapKeyToValue(
                nameof(RecomendacionSalida.Categoria), "PredictedLabel"));

        var model = pipeline.Fit(dataView);
        return (ctx, model, inputSchema);
    }

    private static string MensajePorCategoria(string cat) => cat switch
    {
        "Interior"           => "🏠 Perfecto para actividades bajo techo. Aprovecha para organizar tu espacio y reducir el consumo eléctrico.",
        "MovilidadSostenible"=> "🚲 Clima ideal para bicicleta, caminata o transporte público. ¡Reduce tu huella de carbono hoy!",
        "AhorroEnergia"      => "💨 Ventila tu hogar naturalmente y optimiza el uso de calefacción. Cada kWh ahorrado cuenta.",
        "Hidratacion"        => "☀️ Temperatura alta: hidrátate bien, evita plásticos de un solo uso y usa botellas reutilizables.",
        _                    => "🌱 Clima estable. Planifica rutas sostenibles y busca maneras de reducir tu impacto ambiental."
    };

    // ──────────────────────────────────────────────────────────────
    // DATA CLASSES
    // ──────────────────────────────────────────────────────────────

    private sealed class ClimaEntrada
    {
        [LoadColumn(0)] public float Temperatura   { get; set; }
        [LoadColumn(1)] public float Humedad        { get; set; }
        [LoadColumn(2)] public float Viento         { get; set; }
        [LoadColumn(3)] public float Precipitacion  { get; set; }
        [LoadColumn(4)] public float CodigoClima    { get; set; }
        [LoadColumn(5)] public string Categoria     { get; set; } = string.Empty;
    }

    private sealed class RecomendacionSalida
    {
        [ColumnName("PredictedLabel")]
        public string Categoria { get; set; } = string.Empty;
    }
}

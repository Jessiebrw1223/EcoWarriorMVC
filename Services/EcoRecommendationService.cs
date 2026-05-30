
using EcoWarriorMVC.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Servicio ML.NET para recomendaciones ecológicas inteligentes.
/// Usa clasificación multicategoría basada en clima actual.
/// </summary>
public class EcoRecommendationService : IEcoRecommendationService
{
    private readonly Lazy<
        (MLContext Ctx,
         ITransformer Model,
         DataViewSchema Schema)> _lazy;

    private readonly ILogger<EcoRecommendationService>? _logger;

    public EcoRecommendationService(
        ILogger<EcoRecommendationService>? logger = null)
    {
        _logger = logger;

        _lazy =
            new Lazy<
                (MLContext,
                 ITransformer,
                 DataViewSchema)>(
                EntrenarModelo,
                LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string ObtenerMensajeEco(CurrentWeatherResponse clima)
    {
        try
        {
            var (ctx, model, schema) = _lazy.Value;

            var engine =
                ctx.Model.CreatePredictionEngine
                    <ClimaEntrada, RecomendacionSalida>(
                        model,
                        inputSchema: schema);

            var prediccion =
                engine.Predict(new ClimaEntrada
                {
                    Temperatura =
                        (float)clima.Temperatura,

                    Humedad =
                        (float)clima.Humedad,

                    Viento =
                        (float)clima.VelocidadViento,

                    Precipitacion =
                        (float)clima.Precipitacion,

                    CodigoClima =
                        clima.CodigoClima
                });

            return ObtenerMensajePorCategoria(
                prediccion.Categoria);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error ML.NET");

            return ObtenerMensajePorCategoria(
                "Default");
        }
    }

    // =========================================================
    // ENTRENAMIENTO DEL MODELO
    // =========================================================

    private static (
    MLContext,
    ITransformer,
    DataViewSchema)
    EntrenarModelo()
{
    var ctx = new MLContext(seed: 42);

    var datos = ObtenerDatosEntrenamiento();

    var dataView =
        ctx.Data.LoadFromEnumerable(datos);

    var inputSchema = dataView.Schema;

    var pipeline =
        ctx.Transforms.Conversion.MapValueToKey(
            outputColumnName: "Label",
            inputColumnName:
                nameof(ClimaEntrada.Categoria))

        .Append(
            ctx.Transforms.Concatenate(
                "Features",
                nameof(ClimaEntrada.Temperatura),
                nameof(ClimaEntrada.Humedad),
                nameof(ClimaEntrada.Viento),
                nameof(ClimaEntrada.Precipitacion),
                nameof(ClimaEntrada.CodigoClima)))

        .Append(
            ctx.Transforms.NormalizeMinMax(
                "Features"))

        .Append(
            ctx.MulticlassClassification.Trainers
                .SdcaMaximumEntropy(
                    maximumNumberOfIterations: 100))

        .Append(
            ctx.Transforms.Conversion.MapKeyToValue(
                outputColumnName:
                    nameof(RecomendacionSalida.Categoria),
                inputColumnName:
                    "PredictedLabel"));

    var model = pipeline.Fit(dataView);

    return (ctx, model, inputSchema);
}
    // =========================================================
    // DATASET DE ENTRENAMIENTO
    // =========================================================

    private static List<ClimaEntrada>
        ObtenerDatosEntrenamiento()
    {
        return new List<ClimaEntrada>
        {
            // Movilidad sostenible
            new()
            {
                Temperatura = 24,
                Humedad = 55,
                Viento = 8,
                Precipitacion = 0,
                CodigoClima = 0,
                Categoria = "Movilidad"
            },

            new()
            {
                Temperatura = 26,
                Humedad = 50,
                Viento = 6,
                Precipitacion = 0,
                CodigoClima = 1,
                Categoria = "Movilidad"
            },

            new()
            {
                Temperatura = 22,
                Humedad = 60,
                Viento = 10,
                Precipitacion = 0,
                CodigoClima = 2,
                Categoria = "Movilidad"
            },

            // Ahorro energía
            new()
            {
                Temperatura = 18,
                Humedad = 70,
                Viento = 25,
                Precipitacion = 0,
                CodigoClima = 3,
                Categoria = "Energia"
            },

            new()
            {
                Temperatura = 15,
                Humedad = 75,
                Viento = 30,
                Precipitacion = 0,
                CodigoClima = 45,
                Categoria = "Energia"
            },

            // Interior
            new()
            {
                Temperatura = 16,
                Humedad = 90,
                Viento = 18,
                Precipitacion = 10,
                CodigoClima = 61,
                Categoria = "Interior"
            },

            new()
            {
                Temperatura = 14,
                Humedad = 95,
                Viento = 22,
                Precipitacion = 15,
                CodigoClima = 65,
                Categoria = "Interior"
            },

            // Hidratación
            new()
            {
                Temperatura = 34,
                Humedad = 80,
                Viento = 5,
                Precipitacion = 0,
                CodigoClima = 0,
                Categoria = "Hidratacion"
            },

            new()
            {
                Temperatura = 36,
                Humedad = 78,
                Viento = 4,
                Precipitacion = 0,
                CodigoClima = 1,
                Categoria = "Hidratacion"
            }
        };
    }

    // =========================================================
    // RESPUESTAS ECO
    // =========================================================

    private static string ObtenerMensajePorCategoria(
        string? categoria)
    {
        return categoria switch
        {
            "Interior" =>
                "🏠 Buen momento para actividades dentro de casa y ahorro energético.",

            "Movilidad" =>
                "🚲 Clima ideal para caminar, usar bicicleta o transporte público.",

            "Energia" =>
                "💨 Aprovecha ventilación natural y reduce el uso de electricidad.",

            "Hidratacion" =>
                "☀️ Mantente hidratado y utiliza botellas reutilizables.",

            _ =>
                "🌱 Mantén hábitos sostenibles y reduce tu impacto ambiental."
        };
    }

    // =========================================================
    // CLASES ML.NET
    // =========================================================

    private sealed class ClimaEntrada
    {
        public float Temperatura { get; set; }

        public float Humedad { get; set; }

        public float Viento { get; set; }

        public float Precipitacion { get; set; }

        public float CodigoClima { get; set; }

        public string Categoria { get; set; }
            = string.Empty;
    }

    private sealed class RecomendacionSalida
    {
        [ColumnName("PredictedLabel")]
        public string Categoria { get; set; }
            = string.Empty;
    }
}

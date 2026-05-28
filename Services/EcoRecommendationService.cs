using EcoWarriorMVC.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EcoWarriorMVC.Services;

public class EcoRecommendationService : IEcoRecommendationService
{
    private readonly Lazy<(MLContext Context, ITransformer Model)> _modelo;

    public EcoRecommendationService()
    {
        _modelo = new Lazy<(MLContext, ITransformer)>(EntrenarModelo, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string ObtenerMensajeEco(CurrentWeatherResponse climaActual)
    {
        var (context, model) = _modelo.Value;
        var predictor = context.Model.CreatePredictionEngine<ClimaEntrada, RecomendacionSalida>(model);

        var prediccion = predictor.Predict(new ClimaEntrada
        {
            Temperatura = (float)climaActual.Temperatura,
            Humedad = (float)climaActual.Humedad,
            Viento = (float)climaActual.VelocidadViento,
            CodigoClima = climaActual.CodigoClima
        });

        return MensajePorCategoria(prediccion.Categoria);
    }

    private static (MLContext Context, ITransformer Model) EntrenarModelo()
    {
        var context = new MLContext(seed: 42);

        var datos = new List<ClimaEntrada>
        {
            new() { Temperatura = 24f, Humedad = 55f, Viento = 8f, CodigoClima = 0, Categoria = "MovilidadSostenible" },
            new() { Temperatura = 21f, Humedad = 60f, Viento = 12f, CodigoClima = 1, Categoria = "MovilidadSostenible" },
            new() { Temperatura = 19f, Humedad = 65f, Viento = 10f, CodigoClima = 2, Categoria = "MovilidadSostenible" },
            new() { Temperatura = 22f, Humedad = 70f, Viento = 9f, CodigoClima = 3, Categoria = "AhorroEnergia" },
            new() { Temperatura = 17f, Humedad = 88f, Viento = 16f, CodigoClima = 61, Categoria = "Interior" },
            new() { Temperatura = 16f, Humedad = 90f, Viento = 18f, CodigoClima = 63, Categoria = "Interior" },
            new() { Temperatura = 15f, Humedad = 92f, Viento = 20f, CodigoClima = 65, Categoria = "Interior" },
            new() { Temperatura = 18f, Humedad = 80f, Viento = 14f, CodigoClima = 80, Categoria = "Interior" },
            new() { Temperatura = 20f, Humedad = 76f, Viento = 13f, CodigoClima = 81, Categoria = "Interior" },
            new() { Temperatura = 19f, Humedad = 78f, Viento = 14f, CodigoClima = 82, Categoria = "Interior" },
            new() { Temperatura = 23f, Humedad = 58f, Viento = 32f, CodigoClima = 0, Categoria = "AhorroEnergia" },
            new() { Temperatura = 21f, Humedad = 62f, Viento = 35f, CodigoClima = 1, Categoria = "AhorroEnergia" },
            new() { Temperatura = 13f, Humedad = 70f, Viento = 28f, CodigoClima = 95, Categoria = "Interior" },
            new() { Temperatura = 12f, Humedad = 72f, Viento = 30f, CodigoClima = 96, Categoria = "Interior" }
        };

        var dataView = context.Data.LoadFromEnumerable(datos);

        var pipeline = context.Transforms.Conversion.MapValueToKey("Label", nameof(ClimaEntrada.Categoria))
            .Append(context.Transforms.Concatenate("Features",
                nameof(ClimaEntrada.Temperatura),
                nameof(ClimaEntrada.Humedad),
                nameof(ClimaEntrada.Viento),
                nameof(ClimaEntrada.CodigoClima)))
            .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(context.Transforms.Conversion.MapKeyToValue(nameof(RecomendacionSalida.Categoria), "PredictedLabel"));

        var model = pipeline.Fit(dataView);
        return (context, model);
    }

    private static string MensajePorCategoria(string categoria) => categoria switch
    {
        "Interior" => "Buen momento para priorizar actividades bajo techo y reducir desplazamientos innecesarios.",
        "MovilidadSostenible" => "Clima ideal para usar bicicleta, caminar o hacer actividades al aire libre.",
        "AhorroEnergia" => "Hay condiciones para optimizar consumo: revisa ventilacion, ventanas y uso eficiente de energia.",
        _ => "Clima estable. Aprovecha para planificar rutas sostenibles y ahorrar energia."
    };

    private sealed class ClimaEntrada
    {
        [LoadColumn(0)]
        public float Temperatura { get; set; }

        [LoadColumn(1)]
        public float Humedad { get; set; }

        [LoadColumn(2)]
        public float Viento { get; set; }

        [LoadColumn(3)]
        public float CodigoClima { get; set; }

        [LoadColumn(4)]
        public string Categoria { get; set; } = string.Empty;
    }

    private sealed class RecomendacionSalida
    {
        [ColumnName("PredictedLabel")]
        public string Categoria { get; set; } = string.Empty;
    }
}

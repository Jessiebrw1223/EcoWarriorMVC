using Microsoft.ML;
using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public class MLTrainingService
{
    public static ITransformer TrainModel(MLContext mlContext)
    {
        var dataPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "MLModels",
            "eco_data.csv");

        if (!File.Exists(dataPath))
        {
            throw new FileNotFoundException(
                $"No se encontró el dataset ML.NET en: {dataPath}");
        }
Console.WriteLine($"ML PATH: {dataPath}");
Console.WriteLine($"EXISTS: {File.Exists(dataPath)}");
        var data = mlContext.Data.LoadFromTextFile<EcoData>(
            path: dataPath,
            hasHeader: true,
            separatorChar: ',');

        var pipeline =
            mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(EcoData.Recommendation))

            .Append(
                mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(EcoData.Temperature),
                    nameof(EcoData.Humidity),
                    nameof(EcoData.WindSpeed)))

            .Append(
                mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy())

            .Append(
                mlContext.Transforms.Conversion.MapKeyToValue(
                    "PredictedLabel"));

        var model = pipeline.Fit(data);

        return model;
    }
}
using Microsoft.ML;
using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public static class MLTrainingService
{
    public static void TrainModel()
    {
        var mlContext = new MLContext();

        var dataPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "MLModels",
            "eco_data.csv");

        var data = mlContext.Data.LoadFromTextFile<EcoData>(
            dataPath,
            hasHeader: true,
            separatorChar: ',');

        var pipeline =
            mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(EcoData.Recomendacion))
            .Append(mlContext.Transforms.Concatenate(
                "Features",
                nameof(EcoData.Temperatura),
                nameof(EcoData.Humedad),
                nameof(EcoData.Viento)))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(data);

        var modelPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "MLModels",
            "ecowarrior_model.zip");

        mlContext.Model.Save(model, data.Schema, modelPath);
    }
}
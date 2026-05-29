using Microsoft.ML.Data;

namespace EcoWarriorMVC.Models;

public class EcoData
{
    [LoadColumn(0)]
    public float Temperatura { get; set; }

    [LoadColumn(1)]
    public float Humedad { get; set; }

    [LoadColumn(2)]
    public float Viento { get; set; }

    [LoadColumn(3)]
    public string Recomendacion { get; set; } = string.Empty;
}

public class EcoPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedRecommendation { get; set; } = string.Empty;
}
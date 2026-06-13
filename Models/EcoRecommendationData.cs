namespace EcoWarriorMVC.Models;

public class EcoRecommendationData
{
    public float Temperatura { get; set; }

    public float Humedad { get; set; }

    public string Recomendacion { get; set; } = string.Empty;
}

public class EcoRecommendationPrediction
{
    public string PredictedLabel { get; set; } = string.Empty;
}
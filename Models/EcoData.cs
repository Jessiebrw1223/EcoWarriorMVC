using Microsoft.ML.Data;

namespace EcoWarriorMVC.Models;

public class EcoData
{
    [LoadColumn(0)]
    public float Temperature { get; set; }

    [LoadColumn(1)]
    public float Humidity { get; set; }

    [LoadColumn(2)]
    public float WindSpeed { get; set; }

    [LoadColumn(3)]
    public string Recommendation { get; set; } = "";
}
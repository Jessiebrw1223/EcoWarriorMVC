namespace EcoWarriorMVC.Models;

public class EcoWeatherResponse
{
    public string Ciudad { get; set; } = string.Empty;
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public string Pais { get; set; } = string.Empty;
    public string? NombreRegion { get; set; }
    public CurrentWeatherResponse? ClimaActual { get; set; }
    public string MensajeEco { get; set; } = string.Empty;
}

public class CurrentWeatherResponse
{
    public double Temperatura { get; set; }
    public double SensacionTermica { get; set; }
    public double Humedad { get; set; }
    public double VelocidadViento { get; set; }
    public int CodigoClima { get; set; }
    public string EstadoClima { get; set; } = string.Empty;
}

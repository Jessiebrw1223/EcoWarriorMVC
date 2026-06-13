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

    // Cuando la API externa falla, no fingimos que el dato es real:
    // mostramos modo local y recomendaciones estimadas por zona.
    public bool ModoLocal { get; set; }
    public string FuenteDatos { get; set; } = "Open-Meteo";
    public string? Observacion { get; set; }
}

public class CurrentWeatherResponse
{
    public double Temperatura { get; set; }
    public double SensacionTermica { get; set; }
    public double Humedad { get; set; }
    public double VelocidadViento { get; set; }
    public double Precipitacion { get; set; }
    public int CodigoClima { get; set; }
    public string EstadoClima { get; set; } = string.Empty;
}

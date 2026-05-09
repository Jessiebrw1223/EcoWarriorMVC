using System.Globalization;
using System.Net.Http.Json;
using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public class WeatherService(HttpClient httpClient, IEcoRecommendationService ecoRecommendationService) : IWeatherService
{
    private const string PaisObjetivo = "Peru";

    public async Task<EcoWeatherResponse?> ObtenerClimaPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ciudad))
        {
            ciudad = "Lima";
        }

        var ciudadConsulta = ciudad.Trim();
        if (!ciudadConsulta.Contains(PaisObjetivo, StringComparison.OrdinalIgnoreCase) &&
            !ciudadConsulta.Contains("Perú", StringComparison.OrdinalIgnoreCase))
        {
            ciudadConsulta = $"{ciudadConsulta}, {PaisObjetivo}";
        }

        var ciudadNormalizada = Uri.EscapeDataString(ciudadConsulta);
        var geocodingUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={ciudadNormalizada}&count=1&language=es&format=json";
        var geocoding = await httpClient.GetFromJsonAsync<OpenMeteoGeocodingResponse>(geocodingUrl, cancellationToken);

        var location = geocoding?.Results?.FirstOrDefault();
        if (location is null || !EsPeru(location.Country))
        {
            return null;
        }

        var forecastUrl = string.Format(
            CultureInfo.InvariantCulture,
            "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,wind_speed_10m&timezone=auto&language=es",
            location.Latitude,
            location.Longitude);

        var forecast = await httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(forecastUrl, cancellationToken);
        var current = forecast?.Current;
        if (current is null)
        {
            return null;
        }

        var climaActual = new CurrentWeatherResponse
        {
            Temperatura = current.Temperature2m,
            SensacionTermica = current.ApparentTemperature,
            Humedad = current.RelativeHumidity2m,
            VelocidadViento = current.WindSpeed10m,
            CodigoClima = current.WeatherCode,
            EstadoClima = ObtenerDescripcionClima(current.WeatherCode)
        };

        return new EcoWeatherResponse
        {
            Ciudad = location.Name,
            Latitud = location.Latitude,
            Longitud = location.Longitude,
            Pais = "Perú",
            NombreRegion = location.Admin1,
            ClimaActual = climaActual,
            MensajeEco = ecoRecommendationService.ObtenerMensajeEco(climaActual)
        };
    }

    private static bool EsPeru(string pais) =>
        pais.Equals(PaisObjetivo, StringComparison.OrdinalIgnoreCase) ||
        pais.Equals("Perú", StringComparison.OrdinalIgnoreCase);

    private static string ObtenerDescripcionClima(int weatherCode) => weatherCode switch
    {
        0 => "Despejado",
        1 or 2 => "Parcialmente nublado",
        3 => "Nublado",
        45 or 48 => "Niebla",
        51 or 53 or 55 => "Llovizna",
        61 or 63 or 65 => "Lluvia",
        71 or 73 or 75 => "Nieve",
        80 or 81 or 82 => "Chubascos",
        95 => "Tormenta",
        96 or 99 => "Tormenta con granizo",
        _ => "Condiciones variables"
    };

    private sealed class OpenMeteoGeocodingResponse
    {
        public List<OpenMeteoLocation>? Results { get; set; }
    }

    private sealed class OpenMeteoLocation
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; } = string.Empty;
        public string? Admin1 { get; set; }
    }

    private sealed class OpenMeteoForecastResponse
    {
        public OpenMeteoCurrent? Current { get; set; }
    }

    private sealed class OpenMeteoCurrent
    {
        public double Temperature2m { get; set; }
        public double ApparentTemperature { get; set; }
        public double RelativeHumidity2m { get; set; }
        public double WindSpeed10m { get; set; }
        public int WeatherCode { get; set; }
    }
}

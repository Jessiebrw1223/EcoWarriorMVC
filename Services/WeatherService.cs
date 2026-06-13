using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoWarriorMVC.Models;
using Microsoft.Extensions.Caching.Memory;

namespace EcoWarriorMVC.Services;

public class WeatherService(
    HttpClient httpClient,
    IEcoRecommendationService ecoRecommendationService,
    IMemoryCache cache,
    ILogger<WeatherService> logger) : IWeatherService
{
    private const string PaisObjetivo = "Peru";

    public async Task<EcoWeatherResponse?> ObtenerClimaPorCiudadAsync(
        string ciudad,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ciudad))
        {
            ciudad = "Lima";
        }

        var ciudadEntrada = ciudad.Trim();
        var cacheKey = $"weather:{ciudadEntrada.ToLowerInvariant()}";

        if (cache.TryGetValue(cacheKey, out EcoWeatherResponse? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var ciudadConsulta = ciudadEntrada;

            if (!ciudadConsulta.Contains(PaisObjetivo, StringComparison.OrdinalIgnoreCase) &&
                !ciudadConsulta.Contains("Perú", StringComparison.OrdinalIgnoreCase))
            {
                ciudadConsulta = $"{ciudadConsulta}, {PaisObjetivo}";
            }

            var ciudadNormalizada = Uri.EscapeDataString(ciudadConsulta);

            var geocodingUrl =
                $"https://geocoding-api.open-meteo.com/v1/search?name={ciudadNormalizada}&count=1&language=es&format=json";

            using var geocodingResponse =
                await httpClient.GetAsync(geocodingUrl, cancellationToken);

            if (geocodingResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return CrearRespuestaFallback(ciudadEntrada, "API meteorológica saturada");
            }

            if (!geocodingResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Error geocoding Open-Meteo. StatusCode: {StatusCode}",
                    geocodingResponse.StatusCode);

                return CrearRespuestaFallback(ciudadEntrada, "No se pudo obtener ubicación");
            }

            var geocoding =
                await geocodingResponse.Content.ReadFromJsonAsync<OpenMeteoGeocodingResponse>(
                    cancellationToken);

            var location = geocoding?.Results?.FirstOrDefault();

            if (location is null || !EsPeru(location.Country))
            {
                return CrearRespuestaFallback(ciudadEntrada, "Ciudad no encontrada");
            }

            var forecastUrl = string.Format(
                CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m,apparent_temperature,relative_humidity_2m,precipitation,weather_code,wind_speed_10m&timezone=auto",
                location.Latitude,
                location.Longitude);

            using var forecastResponse =
                await httpClient.GetAsync(forecastUrl, cancellationToken);

            if (forecastResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return CrearRespuestaFallback(location.Name, "API meteorológica saturada");
            }

            if (!forecastResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Error forecast Open-Meteo. StatusCode: {StatusCode}",
                    forecastResponse.StatusCode);

                return CrearRespuestaFallback(location.Name, "No se pudo obtener el clima");
            }

            var forecast =
                await forecastResponse.Content.ReadFromJsonAsync<OpenMeteoForecastResponse>(
                    cancellationToken);

            var current = forecast?.Current;

            if (current is null)
            {
                return CrearRespuestaFallback(location.Name, "Datos climáticos no disponibles");
            }

            var climaActual = new CurrentWeatherResponse
            {
                Temperatura = current.Temperature2m,
                SensacionTermica = current.ApparentTemperature,
                Humedad = current.RelativeHumidity2m,
                VelocidadViento = current.WindSpeed10m,
                Precipitacion = current.Precipitation,
                CodigoClima = current.WeatherCode,
                EstadoClima = ObtenerDescripcionClima(current.WeatherCode)
            };

            var result = new EcoWeatherResponse
            {
                Ciudad = location.Name,
                Latitud = location.Latitude,
                Longitud = location.Longitude,
                Pais = "Perú",
                NombreRegion = location.Admin1,
                ClimaActual = climaActual,
                MensajeEco = ecoRecommendationService.ObtenerMensajeEco(climaActual)
            };

            cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            return result;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Timeout consultando clima.");
            return CrearRespuestaFallback(ciudadEntrada, "Tiempo de espera agotado");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Error HTTP consultando clima.");
            return CrearRespuestaFallback(ciudadEntrada, "Error de conexión con el servicio climático");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado consultando clima.");
            return CrearRespuestaFallback(ciudadEntrada, "Condiciones variables");
        }
    }

    private EcoWeatherResponse CrearRespuestaFallback(string ciudad, string estado)
    {
        var ciudadNormalizada = string.IsNullOrWhiteSpace(ciudad) ? "Lima" : ciudad.Trim();
        var estimado = ObtenerEstimadoLocal(ciudadNormalizada);

        var climaFallback = new CurrentWeatherResponse
        {
            Temperatura = estimado.Temperatura,
            SensacionTermica = estimado.SensacionTermica,
            Humedad = estimado.Humedad,
            VelocidadViento = estimado.Viento,
            Precipitacion = estimado.Precipitacion,
            CodigoClima = estimado.CodigoClima,
            EstadoClima = "MODO LOCAL"
        };

        return new EcoWeatherResponse
        {
            Ciudad = NormalizarNombreCiudad(ciudadNormalizada),
            Pais = "Perú",
            NombreRegion = estimado.Region,
            Latitud = null,
            Longitud = null,
            ClimaActual = climaFallback,
            ModoLocal = true,
            FuenteDatos = "Modo local por saturación o caída de API",
            Observacion = $"{estado}. Datos estimados por zona; no son lectura meteorológica en tiempo real.",
            MensajeEco = ConstruirConsejoLocal(ciudadNormalizada, climaFallback)
        };
    }

    private static (double Temperatura, double SensacionTermica, double Humedad, double Viento, double Precipitacion, int CodigoClima, string Region)
        ObtenerEstimadoLocal(string ciudad)
    {
        var c = ciudad.Trim().ToLowerInvariant()
            .Replace("á", "a").Replace("é", "e").Replace("í", "i")
            .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");

        return c switch
        {
            var x when x.Contains("tumbes") => (28, 30, 78, 12, 0, 1, "Costa norte"),
            var x when x.Contains("piura") => (29, 31, 70, 14, 0, 1, "Costa norte"),
            var x when x.Contains("chiclayo") => (25, 26, 72, 16, 0, 2, "Costa norte"),
            var x when x.Contains("trujillo") => (22, 22, 75, 15, 0, 2, "Costa norte"),
            var x when x.Contains("lima") => (22, 22, 78, 11, 0, 3, "Costa central"),
            var x when x.Contains("ica") => (25, 25, 55, 13, 0, 1, "Costa sur"),
            var x when x.Contains("arequipa") => (18, 18, 42, 10, 0, 0, "Sierra sur"),
            var x when x.Contains("cusco") || x.Contains("cuzco") => (15, 14, 60, 8, 0, 2, "Sierra sur"),
            var x when x.Contains("puno") => (11, 10, 58, 12, 0, 2, "Altiplano"),
            var x when x.Contains("huancayo") => (16, 15, 64, 9, 0, 2, "Sierra central"),
            var x when x.Contains("iquitos") => (30, 33, 84, 7, 2, 61, "Selva norte"),
            var x when x.Contains("tarapoto") => (29, 32, 82, 6, 1, 61, "Selva alta"),
            _ => (22, 22, 65, 8, 0, 3, "Zona estimada")
        };
    }

    private static string ConstruirConsejoLocal(string ciudad, CurrentWeatherResponse clima)
    {
        var nombre = NormalizarNombreCiudad(ciudad);

        if (clima.Temperatura >= 27)
        {
            return $"🌿 Modo local para {nombre}: prioriza hidratación con tomatodo, evita plásticos de un solo uso y ahorra agua en casa.";
        }

        if (clima.Precipitacion > 0 || clima.CodigoClima is 61 or 63 or 65)
        {
            return $"🌧️ Modo local para {nombre}: aprovecha agua de lluvia para riego, evita traslados innecesarios y organiza residuos bajo techo.";
        }

        if (clima.Temperatura <= 16)
        {
            return $"🌱 Modo local para {nombre}: usa abrigo antes de calefacción, aprovecha luz natural y separa residuos reciclables.";
        }

        return $"🌱 Modo local para {nombre}: buen momento para caminar trayectos cortos, usar transporte público y mantener reciclaje activo.";
    }

    private static string NormalizarNombreCiudad(string ciudad)
    {
        var limpia = string.IsNullOrWhiteSpace(ciudad) ? "Lima" : ciudad.Trim();
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(limpia.ToLowerInvariant());
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
        [JsonPropertyName("results")]
        public List<OpenMeteoLocation>? Results { get; set; }
    }

    private sealed class OpenMeteoLocation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("admin1")]
        public string? Admin1 { get; set; }
    }

    private sealed class OpenMeteoForecastResponse
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrent? Current { get; set; }
    }

    private sealed class OpenMeteoCurrent
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double RelativeHumidity2m { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; set; }

        [JsonPropertyName("precipitation")]
        public double Precipitation { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }
}

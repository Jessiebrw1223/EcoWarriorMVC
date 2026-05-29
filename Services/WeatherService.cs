using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoWarriorMVC.Models;
using Microsoft.Extensions.Caching.Memory;

namespace EcoWarriorMVC.Services;

/// <summary>
/// Servicio de clima usando Open-Meteo (geocoding + forecast).
/// Incluye caché en memoria, retry con fallback al microservicio Go, 
/// y recomendación eco vía ML.NET.
/// </summary>
public class WeatherService(
    HttpClient httpClient,
    IEcoRecommendationService ecoRecommendationService,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<WeatherService> logger) : IWeatherService
{
    private const string PaisObjetivo = "Peru";
    private static readonly string[] PaisVariants = ["Peru", "Perú", "PERU", "PERÚ"];

    public async Task<EcoWeatherResponse?> ObtenerClimaPorCiudadAsync(
        string ciudad,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ciudad))
            ciudad = configuration["Weather:DefaultCity"] ?? "Lima";

        var cacheKey = $"clima:{ciudad.Trim().ToLowerInvariant()}";

        // --- Cache hit ---
        if (cache.TryGetValue(cacheKey, out EcoWeatherResponse? cached))
        {
            logger.LogDebug("Clima para {Ciudad} obtenido desde caché.", ciudad);
            return cached;
        }

        var resultado = await ObtenerClimaInternoAsync(ciudad, cancellationToken)
                        ?? await ObtenerClimaGoFallbackAsync(ciudad, cancellationToken)
                        ?? CrearRespuestaFallback(ciudad, "Servicio no disponible");

        // Cache solo si hay datos reales (temperatura != 0)
        if (resultado.ClimaActual?.Temperatura != 0)
        {
            var duracion = TimeSpan.FromMinutes(
                configuration.GetValue<int>("Weather:CacheDurationMinutes", 10));
            cache.Set(cacheKey, resultado, duracion);
        }

        return resultado;
    }

    // ──────────────────────────────────────────────────────────────
    // FLUJO PRINCIPAL: Open-Meteo geocoding → forecast
    // ──────────────────────────────────────────────────────────────

    private async Task<EcoWeatherResponse?> ObtenerClimaInternoAsync(
        string ciudad, CancellationToken ct)
    {
        try
        {
            var ciudadConsulta = NormalizarCiudad(ciudad);
            var ciudadUri = Uri.EscapeDataString(ciudadConsulta);

            // 1. Geocoding
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={ciudadUri}&count=5&language=es&format=json";
            var timeout = configuration.GetValue<int>("Weather:TimeoutSeconds", 8);

            using var ctTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            ctTimeout.CancelAfter(TimeSpan.FromSeconds(timeout));

            var geoResp = await httpClient.GetAsync(geoUrl, ctTimeout.Token);

            if (geoResp.StatusCode == HttpStatusCode.TooManyRequests)
                return CrearRespuestaFallback(ciudad, "Límite de solicitudes alcanzado");

            if (!geoResp.IsSuccessStatusCode)
            {
                logger.LogWarning("Geocoding respondió {Status} para {Ciudad}.", geoResp.StatusCode, ciudad);
                return null;
            }

            var geo = await geoResp.Content.ReadFromJsonAsync<OpenMeteoGeocodingResponse>(ctTimeout.Token);
            var location = geo?.Results?.FirstOrDefault(r => EsPeru(r.Country));

            if (location is null)
            {
                logger.LogInformation("No se encontró {Ciudad} en Perú.", ciudad);
                return CrearRespuestaFallback(ciudad, "Ciudad no encontrada en Perú");
            }

            // 2. Forecast
            var forecastUrl = string.Format(
                CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}" +
                "&current=temperature_2m,apparent_temperature,relative_humidity_2m," +
                "weather_code,wind_speed_10m,precipitation&timezone=auto",
                location.Latitude, location.Longitude);

            var forecastResp = await httpClient.GetAsync(forecastUrl, ctTimeout.Token);

            if (forecastResp.StatusCode == HttpStatusCode.TooManyRequests)
                return CrearRespuestaFallback(ciudad, "API meteorológica saturada");

            if (!forecastResp.IsSuccessStatusCode)
                return null;

            var forecast = await forecastResp.Content
                .ReadFromJsonAsync<OpenMeteoForecastResponse>(ctTimeout.Token);
            var current = forecast?.Current;

            if (current is null)
                return CrearRespuestaFallback(ciudad, "Datos climáticos no disponibles");

            var climaActual = new CurrentWeatherResponse
            {
                Temperatura      = current.Temperature2m,
                SensacionTermica = current.ApparentTemperature,
                Humedad          = current.RelativeHumidity2m,
                VelocidadViento  = current.WindSpeed10m,
                Precipitacion    = current.Precipitation,
                CodigoClima      = current.WeatherCode,
                EstadoClima      = ObtenerDescripcionClima(current.WeatherCode)
            };

            return new EcoWeatherResponse
            {
                Ciudad       = location.Name,
                Latitud      = location.Latitude,
                Longitud     = location.Longitude,
                Pais         = "Perú",
                NombreRegion = location.Admin1,
                ClimaActual  = climaActual,
                MensajeEco   = ecoRecommendationService.ObtenerMensajeEco(climaActual),
                FechaConsulta = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Timeout al consultar clima para {Ciudad}.", ciudad);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error HTTP al consultar clima para {Ciudad}.", ciudad);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en WeatherService para {Ciudad}.", ciudad);
            return null;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // FALLBACK: Microservicio Go (si está disponible)
    // ──────────────────────────────────────────────────────────────

    private async Task<EcoWeatherResponse?> ObtenerClimaGoFallbackAsync(
        string ciudad, CancellationToken ct)
    {
        var baseUrl = configuration["GoWeatherService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;

        try
        {
            var url = $"{baseUrl}/api/clima?ciudad={Uri.EscapeDataString(ciudad)}";
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var resp = await httpClient.GetFromJsonAsync<EcoWeatherResponse>(url, cts.Token);
            if (resp is not null)
            {
                logger.LogInformation("Clima para {Ciudad} obtenido del microservicio Go.", ciudad);
                // Agregar recomendación ML si no viene del Go service
                if (string.IsNullOrWhiteSpace(resp.MensajeEco) && resp.ClimaActual is not null)
                    resp.MensajeEco = ecoRecommendationService.ObtenerMensajeEco(resp.ClimaActual);
            }
            return resp;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Go weather fallback no disponible para {Ciudad}.", ciudad);
            return null;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────────────────────

    private string NormalizarCiudad(string ciudad)
    {
        var trimmed = ciudad.Trim();
        if (!PaisVariants.Any(v =>
                trimmed.Contains(v, StringComparison.OrdinalIgnoreCase)))
        {
            return $"{trimmed}, {PaisObjetivo}";
        }
        return trimmed;
    }

    private static bool EsPeru(string pais) =>
        PaisVariants.Any(v => pais.Equals(v, StringComparison.OrdinalIgnoreCase));

    private EcoWeatherResponse CrearRespuestaFallback(string ciudad, string motivo)
    {
        logger.LogInformation("Usando fallback para {Ciudad}: {Motivo}.", ciudad, motivo);
        var climaFallback = new CurrentWeatherResponse
        {
            Temperatura      = 0,
            SensacionTermica = 0,
            Humedad          = 0,
            VelocidadViento  = 0,
            CodigoClima      = -1,
            EstadoClima      = motivo
        };
        return new EcoWeatherResponse
        {
            Ciudad       = ciudad,
            Pais         = "Perú",
            NombreRegion = string.Empty,
            Latitud      = 0,
            Longitud     = 0,
            ClimaActual  = climaFallback,
            MensajeEco   = "Recomendación no disponible en este momento.",
            FechaConsulta = DateTime.UtcNow
        };
    }

    private static string ObtenerDescripcionClima(int code) => code switch
    {
        0          => "Despejado",
        1 or 2     => "Parcialmente nublado",
        3          => "Nublado",
        45 or 48   => "Niebla",
        51 or 53 or 55 => "Llovizna",
        61 or 63 or 65 => "Lluvia",
        71 or 73 or 75 => "Nieve",
        80 or 81 or 82 => "Chubascos",
        95         => "Tormenta",
        96 or 99   => "Tormenta con granizo",
        _          => "Condiciones variables"
    };

    // ──────────────────────────────────────────────────────────────
    // DTOs INTERNOS (solo para deserialización Open-Meteo)
    // ──────────────────────────────────────────────────────────────

    private sealed class OpenMeteoGeocodingResponse
    {
        public List<OpenMeteoLocation>? Results { get; set; }
    }

    private sealed class OpenMeteoLocation
    {
        public string Name    { get; set; } = string.Empty;
        public double Latitude  { get; set; }
        public double Longitude { get; set; }
        public string Country   { get; set; } = string.Empty;
        public string? Admin1   { get; set; }
    }

    private sealed class OpenMeteoForecastResponse
    {
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

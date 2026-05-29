// GoWeatherService - Microservicio fallback en Go
// Expone GET /api/clima?ciudad=Lima usando Open-Meteo.
// Se levanta en :8090 y el MVC de C# lo llama si el servicio principal falla.
//
// Compilar: go build -o go-weather-service .
// Ejecutar: ./go-weather-service
// Docker:   docker build -t go-weather-service . && docker run -p 8090:8090 go-weather-service

package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"net/url"
	"os"
	"time"
)

// ─── Modelos ────────────────────────────────────────────────────────────────

type GeoResult struct {
	Name      string  `json:"name"`
	Latitude  float64 `json:"latitude"`
	Longitude float64 `json:"longitude"`
	Country   string  `json:"country"`
	Admin1    string  `json:"admin1"`
}

type GeoResponse struct {
	Results []GeoResult `json:"results"`
}

type OpenMeteoCurrent struct {
	Temperature2m       float64 `json:"temperature_2m"`
	ApparentTemperature float64 `json:"apparent_temperature"`
	RelativeHumidity2m  float64 `json:"relative_humidity_2m"`
	WindSpeed10m        float64 `json:"wind_speed_10m"`
	Precipitation       float64 `json:"precipitation"`
	WeatherCode         int     `json:"weather_code"`
}

type OpenMeteoForecast struct {
	Current OpenMeteoCurrent `json:"current"`
}

type ClimaActual struct {
	Temperatura      float64 `json:"temperatura"`
	SensacionTermica float64 `json:"sensacionTermica"`
	Humedad          float64 `json:"humedad"`
	VelocidadViento  float64 `json:"velocidadViento"`
	Precipitacion    float64 `json:"precipitacion"`
	CodigoClima      int     `json:"codigoClima"`
	EstadoClima      string  `json:"estadoClima"`
}

type EcoWeatherResponse struct {
	Ciudad       string      `json:"ciudad"`
	Latitud      float64     `json:"latitud"`
	Longitud     float64     `json:"longitud"`
	Pais         string      `json:"pais"`
	NombreRegion string      `json:"nombreRegion"`
	ClimaActual  ClimaActual `json:"climaActual"`
	MensajeEco   string      `json:"mensajeEco"`
	FechaConsulta string     `json:"fechaConsulta"`
}

// ─── HTTP Client compartido ──────────────────────────────────────────────────

var httpClient = &http.Client{Timeout: 8 * time.Second}

// ─── Handler principal ────────────────────────────────────────────────────────

func climaHandler(w http.ResponseWriter, r *http.Request) {
	ciudad := r.URL.Query().Get("ciudad")
	if ciudad == "" {
		ciudad = "Lima"
	}

	ctx, cancel := context.WithTimeout(r.Context(), 7*time.Second)
	defer cancel()

	resp, err := obtenerClima(ctx, ciudad)
	if err != nil {
		log.Printf("Error obteniendo clima para %s: %v", ciudad, err)
		http.Error(w, `{"error":"No se pudo obtener el clima"}`, http.StatusServiceUnavailable)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.Header().Set("Access-Control-Allow-Origin", "*")
	_ = json.NewEncoder(w).Encode(resp)
}

func healthHandler(w http.ResponseWriter, _ *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	fmt.Fprintln(w, `{"status":"ok","service":"go-weather-service"}`)
}

// ─── Lógica de clima ──────────────────────────────────────────────────────────

func obtenerClima(ctx context.Context, ciudad string) (*EcoWeatherResponse, error) {
	// 1. Geocoding
	geoURL := fmt.Sprintf(
		"https://geocoding-api.open-meteo.com/v1/search?name=%s,Peru&count=1&language=es&format=json",
		url.QueryEscape(ciudad),
	)
	geo, err := getJSON[GeoResponse](ctx, geoURL)
	if err != nil || len(geo.Results) == 0 {
		return nil, fmt.Errorf("geocoding fallido para %s: %w", ciudad, err)
	}
	loc := geo.Results[0]

	// 2. Forecast
	forecastURL := fmt.Sprintf(
		"https://api.open-meteo.com/v1/forecast?latitude=%f&longitude=%f&current=temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,wind_speed_10m,precipitation&timezone=auto",
		loc.Latitude, loc.Longitude,
	)
	forecast, err := getJSON[OpenMeteoForecast](ctx, forecastURL)
	if err != nil {
		return nil, fmt.Errorf("forecast fallido: %w", err)
	}

	cur := forecast.Current
	estado := codigoADescripcion(cur.WeatherCode)

	return &EcoWeatherResponse{
		Ciudad:       loc.Name,
		Latitud:      loc.Latitude,
		Longitud:     loc.Longitude,
		Pais:         "Perú",
		NombreRegion: loc.Admin1,
		ClimaActual: ClimaActual{
			Temperatura:      cur.Temperature2m,
			SensacionTermica: cur.ApparentTemperature,
			Humedad:          cur.RelativeHumidity2m,
			VelocidadViento:  cur.WindSpeed10m,
			Precipitacion:    cur.Precipitation,
			CodigoClima:      cur.WeatherCode,
			EstadoClima:      estado,
		},
		MensajeEco:    mensajeEco(cur.Temperature2m, cur.Precipitation, cur.WindSpeed10m),
		FechaConsulta: time.Now().UTC().Format(time.RFC3339),
	}, nil
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

func getJSON[T any](ctx context.Context, rawURL string) (*T, error) {
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, rawURL, nil)
	if err != nil {
		return nil, err
	}
	resp, err := httpClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("HTTP %d desde %s", resp.StatusCode, rawURL)
	}
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}
	var result T
	if err := json.Unmarshal(body, &result); err != nil {
		return nil, err
	}
	return &result, nil
}

func codigoADescripcion(code int) string {
	switch {
	case code == 0:
		return "Despejado"
	case code <= 2:
		return "Parcialmente nublado"
	case code == 3:
		return "Nublado"
	case code == 45 || code == 48:
		return "Niebla"
	case code >= 51 && code <= 55:
		return "Llovizna"
	case code >= 61 && code <= 65:
		return "Lluvia"
	case code >= 80 && code <= 82:
		return "Chubascos"
	case code == 95:
		return "Tormenta"
	case code == 96 || code == 99:
		return "Tormenta con granizo"
	default:
		return "Condiciones variables"
	}
}

func mensajeEco(temp, precip, viento float64) string {
	switch {
	case precip > 3:
		return "🏠 Lluvias activas. Ideal para actividades bajo techo y captar agua de lluvia."
	case temp > 30:
		return "☀️ Temperatura alta: usa botella reutilizable y evita el plástico descartable."
	case viento > 25:
		return "💨 Aprovecha la ventilación natural. Apaga aires acondicionados y ventiladores eléctricos."
	case temp >= 18 && temp <= 26 && precip == 0:
		return "🚲 Clima perfecto para bicicleta o caminata. ¡Deja el auto en casa!"
	default:
		return "🌱 Buen momento para revisar tu huella de carbono y planificar acciones sostenibles."
	}
}

// ─── main ────────────────────────────────────────────────────────────────────

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "8090"
	}

	mux := http.NewServeMux()
	mux.HandleFunc("/api/clima", climaHandler)
	mux.HandleFunc("/health",    healthHandler)

	log.Printf("🌿 Go Weather Service escuchando en :%s", port)
	if err := http.ListenAndServe(":"+port, mux); err != nil {
		log.Fatalf("Error iniciando servidor: %v", err)
	}
}

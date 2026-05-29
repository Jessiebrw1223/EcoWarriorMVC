# Go Weather Service — Microservicio de Clima

Microservicio de respaldo escrito en **Go 1.22** que expone la misma API de clima
que el `WeatherService` de C#. EcoWarrior MVC lo llama como fallback si Open-Meteo
no responde dentro del timeout configurado.

## Endpoints

| Método | Ruta             | Descripción                          |
|--------|------------------|--------------------------------------|
| GET    | `/api/clima`     | `?ciudad=Lima` → EcoWeatherResponse  |
| GET    | `/health`        | Health check JSON                    |

## Desarrollo local

```bash
cd GoWeatherService
go run main.go
# Escucha en :8090
curl "http://localhost:8090/api/clima?ciudad=Arequipa"
```

## Compilar binario

```bash
go build -o go-weather-service .
./go-weather-service
```

## Docker

```bash
docker build -t go-weather-service .
docker run -p 8090:8090 go-weather-service
```

## Variable de entorno

| Variable | Default | Descripción               |
|----------|---------|---------------------------|
| `PORT`   | `8090`  | Puerto en que escucha     |

## Integración con MVC C#

En `appsettings.json` configura:
```json
{
  "GoWeatherService": {
    "BaseUrl": "http://localhost:8090"
  }
}
```
Si la URL está vacía o en blanco, el fallback Go se desactiva silenciosamente.

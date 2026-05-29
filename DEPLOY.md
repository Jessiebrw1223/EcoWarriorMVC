# 🚀 Guía de Despliegue — EcoWarrior MVC

## ✅ Stack Técnico
- **ASP.NET Core 10 MVC** (C#)
- **PostgreSQL** (Neon, Supabase, o Render)
- **ML.NET 4** — Recomendaciones ecológicas por clima
- **Semantic Kernel 1.30 + GPT-4o-mini** — Agente EcoBot
- **Open-Meteo API** — Clima gratuito sin clave
- **Go 1.22** — Microservicio fallback de clima
- **Despliegue:** Render.com (gratuito)

---

## 🔧 Configuración Previa (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=TU_HOST;Port=5432;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "ModelId": "gpt-4o-mini"
  },
  "GoWeatherService": {
    "BaseUrl": ""
  },
  "Weather": {
    "CacheDurationMinutes": 10,
    "TimeoutSeconds": 8,
    "DefaultCity": "Lima"
  }
}
```

> **Nota:** Si `OpenAI:ApiKey` está vacío, el EcoBot funciona en modo fallback con respuestas predefinidas. El clima NO necesita API key (usa Open-Meteo gratuito).

---

## 🌐 Despliegue en Render.com (GRATUITO)

### Paso 1 — Base de datos PostgreSQL

1. Ir a **render.com** → New → **PostgreSQL**
2. Nombre: `ecowarrior-db`
3. Plan: **Free**
4. Copiar el **Connection String** que da Render

### Paso 2 — Web Service

1. Ir a **render.com** → New → **Web Service**
2. Conectar tu repositorio GitHub (sube el proyecto primero)
3. Configuración:
   - **Runtime:** `Docker` O `Native (.NET)`
   - **Build Command:** `dotnet publish -c Release -o out`
   - **Start Command:** `dotnet out/EcoWarriorMVC.dll`
   - **Branch:** `main`

### Paso 3 — Variables de entorno en Render

En el Web Service → **Environment** → Add Variable:

| Clave | Valor |
|-------|-------|
| `ConnectionStrings__DefaultConnection` | (el string de Render PostgreSQL) |
| `OpenAI__ApiKey` | `sk-...` (opcional) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

> **Render usa `__` (doble guión bajo) como separador de secciones JSON.**

### Paso 4 — Deploy

1. Render detecta el `EcoWarriorMVC.csproj` automáticamente
2. Primer deploy tarda ~3-5 min
3. Las migraciones y seed de BD se aplican automáticamente al iniciar

---

## 🐳 Despliegue con Docker (alternativa)

```bash
# Crear imagen
docker build -t ecowarrior-mvc .

# Ejecutar localmente
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=..." \
  -e OpenAI__ApiKey="sk-..." \
  ecowarrior-mvc
```

### Dockerfile (crear en raíz si no existe):
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EcoWarriorMVC.dll"]
```

---

## 🦺 Microservicio Go (opcional)

El microservicio Go es un **fallback** — solo se usa si la API .NET falla.

```bash
cd GoWeatherService
go build -o go-weather-service .
./go-weather-service          # Escucha en :8090

# O con Docker:
docker build -t go-weather .
docker run -p 8090:8090 go-weather
```

En Render: deploy como segundo Web Service con imagen Docker en puerto 8090, luego configura la variable `GoWeatherService__BaseUrl` con su URL.

---

## 🧪 Credenciales de Prueba

| Usuario | Contraseña | Puntos |
|---------|-----------|--------|
| admin@ecowarrior.com | Eco12345 | 3,200 |
| luisa@ecowarrior.com | Eco12345 | 2,750 |
| marco@ecowarrior.com | Eco12345 | 1,980 |

---

## 🔥 Funciones que Operan en Producción

| Función | Estado | Notas |
|---------|--------|-------|
| Login / Registro | ✅ | BD PostgreSQL |
| Clima en tiempo real | ✅ | Open-Meteo gratuito |
| Búsqueda de ciudad | ✅ | Qualquier ciudad del Perú |
| ML.NET Recomendación | ✅ | Sin clave, entrena al arrancar |
| EcoBot IA | ✅* | *Requiere OpenAI API key |
| Retos + Completar | ✅ | Actualiza puntos en BD |
| Ranking real | ✅ | Top de BD en vivo |
| Perfil dinámico | ✅ | Insignias calculadas |
| Podio animado | ✅ | Top 3 con animación |
| Compartir stats | ✅ | Web Share API |
| Microservicio Go | ✅ | Fallback automático |

# EcoWarrior MVC — Mejoras Implementadas

## 🐛 Bugs Corregidos

### 1. `Index.cshtml` — Errores de Razor y datos falsos
- **`@Model.TotalCategorias.2 kg`** → Error de compilación Razor. Corregido a `@Model.ReduccionCO2Kg kg`
- **KPIs mockeados** → Todos los KPIs ahora usan datos reales del `HomeViewModel`:
  - `PuntosUsuario` (desde BD del perfil)
  - `RetosCompletados` (desde BD)
  - `ReduccionCO2Kg` (calculado: retos × 1.05 kg estimado)
- **Puntos en header** → Antes mostraba `StockDisponible` (inventario). Ahora muestra `PuntosUsuario`
- **Clima fallback a 0°C** → La vista ahora muestra un mensaje de error útil con botón "Reintentar"

### 2. `WeatherService.cs`
- Agregado `JsonPropertyName` en DTOs internos para mapeo correcto con Open-Meteo
- Nuevo campo `Precipitacion` en `CurrentWeatherResponse`
- Caché en memoria (`IMemoryCache`) para no spamear Open-Meteo (10 min por defecto)
- Timeout configurable desde `appsettings.json`
- Logging estructurado en cada paso (geocoding, forecast, fallback)
- `NormalizarCiudad()` mejorado para evitar duplicar "Peru" en la query
- Fallback al microservicio Go si el servicio principal falla

### 3. `EcoRecommendationService.cs` — ML.NET
- Dataset de entrenamiento ampliado de 14 a 27 muestras
- Nueva feature: `Precipitacion` (mejora precisión en días lluviosos)
- Nueva categoría: **`Hidratacion`** para temperaturas muy altas (costa norte)
- `NormalizeMinMax` aplicado antes del clasificador SDCA
- `PredictionEngine` creado por invocación (thread-safety correcta)
- `inputSchema` preservado para evitar rebuilds innecesarios

---

## ✨ Nuevas Funcionalidades

### 4. Agente IA — Semantic Kernel + OpenAI GPT-4o-mini
**Archivo:** `Services/EcoAiAgentService.cs`

- Agente conversacional con system prompt especializado en ecología peruana
- **Plugin nativo** `EcoConsejosPlugin` con 2 funciones:
  - `obtener_equivalencia_co2(kg)` → equivalencias cotidianas de CO2
  - `obtener_consejo_por_clima(clima)` → consejos según condición climática
- Auto-invocación de funciones del kernel (`ToolCallBehavior.AutoInvokeKernelFunctions`)
- Generador de **retos personalizados** según nivel y categoría del usuario (retorna JSON)
- Modo degradado (stub) cuando no hay API key configurada

**API Endpoints:**
```
POST /api/ecobot/consultar          { pregunta, ciudad }
POST /api/ecobot/reto-personalizado { puntos, categoriaFavorita }
```

### 5. Microservicio Go — Fallback de Clima
**Carpeta:** `GoWeatherService/`

- Servidor HTTP nativo en Go 1.22 (sin frameworks externos)
- Misma lógica Open-Meteo que el servicio C# (geocoding → forecast)
- Generic `getJSON[T]()` con contexto y timeout
- Health endpoint `/health`
- Puerto configurable via `PORT` env var
- Dockerfile multi-stage (alpine, imagen final < 15MB)
- Se activa automáticamente cuando C# falla (configurable en appsettings)

### 6. Dashboard UI
- **EcoBot Chat Widget** embebido en panel con historial visual
- **Reto IA** generado dinámicamente por Semantic Kernel al cargar
- **Ranking** cargado desde `/api/home/ranking` (datos reales de BD)
- **Compartir estadísticas** con Web Share API (fallback a clipboard)
- **Lluvia** mostrada como métrica adicional cuando `Precipitacion > 0`
- Timestamp de última actualización del clima

---

## 📁 Arquitectura de Capas

```
MVC (ASP.NET Core 10)
├── Controllers/
│   ├── HomeController.cs          ← MVC views
│   ├── HomeApiController.cs       ← REST: resumen, retos, ranking, perfil
│   ├── WeatherApiController.cs    ← REST: /api/clima
│   └── AiAgentApiController.cs    ← REST: /api/ecobot (NUEVO)
├── Services/
│   ├── WeatherService.cs          ← Open-Meteo + caché + fallback Go
│   ├── EcoRecommendationService   ← ML.NET SDCA multiclass (MEJORADO)
│   └── EcoAiAgentService.cs       ← Semantic Kernel + GPT (NUEVO)
├── GoWeatherService/              ← Microservicio Go fallback (NUEVO)
│   ├── main.go
│   ├── go.mod
│   └── Dockerfile
└── Views/Home/
    └── Index.cshtml               ← Bugs corregidos + EcoBot UI
```

---

## ⚙️ Configuración Requerida

### `appsettings.json`
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",          // Tu clave OpenAI
    "ModelId": "gpt-4o-mini"
  },
  "GoWeatherService": {
    "BaseUrl": "http://localhost:8090"   // Dejar vacío para desactivar
  },
  "Weather": {
    "CacheDurationMinutes": 10,
    "TimeoutSeconds": 8,
    "DefaultCity": "Lima"
  }
}
```

### Paquetes NuGet Nuevos
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.30.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.30.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0-preview.3.25171.6" />
```

# Mejoras EcoBot Conversacional

Se mejoró EcoBot para que funcione como asistente ecológico profesional y no como respuesta estática.

## Cambios incluidos

- Respuestas humanas y variadas usando ML.NET + fallback conversacional gratuito.
- Integración con Semantic Kernel Plugin mediante `Plugins/EcoBotPlugin`.
- Prompt actualizado con contexto del sitio: clima, retos, puntos, ranking, reciclaje y misiones.
- Preguntas de seguimiento en cada respuesta para mantener el flujo.
- Manejo de temas fuera del objetivo ecológico con redirección amable.
- Soporte para temas de bebidas, comida y negocios desde enfoque sostenible.
- Frontend del chatbot con historial en `localStorage`.
- Reintentos automáticos para evitar pérdida de conversación.
- Estado visual: conectado, reintentando y modo local.
- Opciones rápidas: clima, retos, puntos y reciclaje.

## Archivos modificados

- `Services/EcoAiAgentService.cs`
- `Services/EcoRecommendationService.cs`
- `Controllers/AiAgentApiController.cs`
- `Views/Shared/_Layout.cshtml`
- `wwwroot/css/site.css`
- `Plugins/EcoBotPlugin/ResponderConsulta/skprompt.txt`
- `Plugins/EcoBotPlugin/GenerarReto/skprompt.txt`
- `Plugins/EcoBotPlugin/ClasificarPregunta/skprompt.txt`

## Validación esperada

Ejecutar:

```bash
dotnet build
```

Luego subir a GitHub y Render.

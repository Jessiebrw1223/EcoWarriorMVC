# EcoWarriorMVC - Correcciones aplicadas

## Cambios principales

- Proyecto migrado a .NET 8 estable.
- Se eliminó la cadena real de PostgreSQL del `appsettings.json`.
- `DefaultConnection` quedó preparado para variables de entorno en Render.
- Se corrigió `appsettings.Development.json` para usar `ConnectionStrings:DefaultConnection`.
- Se registró `IEcoAiAgentService` en `Program.cs`.
- EcoBot ahora funciona con OpenAI si hay API Key y usa fallback si no existe.
- Se agregó soporte opcional de Redis para sesiones distribuidas.
- Se agregó CRUD MVC funcional de productos.
- Se agregó API REST `/api/productos`.
- Se agregó vista `Contacto`.
- Se agregó vista `Avances` con lo solicitado por semanas.
- Se corrigió seguridad básica de contraseñas usando PBKDF2.

## Variables para Render

Configurar en Environment:

```txt
ConnectionStrings__DefaultConnection=Host=TU_HOST;Port=5432;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
OpenAI__ApiKey=TU_API_KEY_OPCIONAL
OpenAI__ModelId=gpt-4o-mini
Redis__ConnectionString=TU_REDIS_URL_OPCIONAL
Weather__TimeoutSeconds=10
```

## Usuario demo

```txt
Correo: admin@ecowarrior.com
Contraseña: Eco12345
```

## Rutas principales

- `/Home/Login`
- `/Home/Registro`
- `/Home/Index`
- `/Productos`
- `/Productos/Crear`
- `/Home/Avances`
- `/Home/Contacto`

## APIs principales

- `GET /api/home/dashboard`
- `GET /api/productos`
- `POST /api/productos`
- `GET /api/badges`
- `POST /api/ecobot/consultar`
- `GET /api/clima?ciudad=Lima`

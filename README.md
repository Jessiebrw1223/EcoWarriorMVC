# EcoWarriorMVC

Proyecto base en **ASP.NET Core MVC** creado tomando como referencia tu proyecto `EcoWarrior`.

## Qué incluye

- Estructura MVC limpia
- `HomeController` y `ProductosController`
- Modelos y ViewModels
- Persistencia de productos con PostgreSQL (EF Core)
- Vistas responsive con Bootstrap
- Formulario de contacto con validaciones

## Estructura

```text
EcoWarriorMVC/
├── Controllers/
├── Models/
├── Services/
├── ViewModels/
├── Views/
├── wwwroot/
├── Program.cs
└── EcoWarriorMVC.csproj
```

## Ejecutar

```bash
dotnet restore
dotnet run
```

## PostgreSQL + DBeaver

1. Crea la base de datos en PostgreSQL:

```sql
CREATE DATABASE ecowarrior_db;
```

2. Verifica o ajusta la cadena de conexion en [appsettings.Development.json](appsettings.Development.json):

```json
"ConnectionStrings": {
	"PostgreSql": "Host=localhost;Port=5432;Database=ecowarrior_db;Username=postgres;Password=postgres"
}
```

3. Ejecuta la app para crear la tabla automaticamente y sembrar productos iniciales:

```bash
dotnet run
```

4. En DBeaver crea una nueva conexion PostgreSQL con:
- Host: `localhost`
- Port: `5432`
- Database: `ecowarrior_db`
- Username: `postgres`
- Password: `postgres`

5. Abre el esquema `public` y la tabla `productos`.

## Backend Home (API)

Se agregaron endpoints JSON para la funcionalidad principal de Home:

- `GET /api/home/dashboard`
- `GET /api/home/resumen`
- `GET /api/home/retos`
- `GET /api/home/ranking`
- `GET /api/home/perfil?correo=usuario@correo.com`
- `POST /api/home/login`
- `POST /api/home/registro`
- `POST /api/home/contacto`
- `GET /api/home/nosotros`

### Ejemplo rápido

```http
POST /api/home/login
Content-Type: application/json

{
  "correo": "admin@ecowarrior.com",
  "contrasena": "Eco12345"
}
```

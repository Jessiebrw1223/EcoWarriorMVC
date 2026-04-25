# EcoWarrior MVC - ASP.NET Core + PostgreSQL + DBeaver + Render

Proyecto base funcional para el trabajo grupal: Contacto, Catálogo/Retos, Login, Base de Datos, Sesiones, Render, APIs, ML.NET, Semantic Kernel + LLM.

## Stack
- ASP.NET Core MVC .NET 8
- PostgreSQL
- Entity Framework Core
- DBeaver para administrar la BD
- Render para deploy
- ML.NET para clasificación/recomendación base
- Semantic Kernel preparado para agente IA

## Base de datos local
Crear en PostgreSQL:

```sql
CREATE DATABASE ecowarrior_db;
```

Luego ajustar `appsettings.Development.json`:

```json
"Host=localhost;Port=5432;Database=ecowarrior_db;Username=postgres;Password=TU_PASSWORD"
```

## Comandos

```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Abrir:

```txt
https://localhost:5001
```

## Usuario demo

```txt
Admin: admin@ecowarrior.com
Password: Admin123*
```

```txt
Usuario: demo@ecowarrior.com
Password: Demo123*
```

## Ramas sugeridas

```bash
git checkout -b develop
git checkout -b feature/login
git checkout -b feature/catalogo
git checkout -b feature/contacto
git checkout -b feature/database
git checkout -b feature/render
git checkout -b feature/mlnet
git checkout -b feature/semantic-kernel
```

## APIs incluidas

```txt
GET  /api/challenges
GET  /api/ranking
POST /api/recommendations
POST /api/classification
POST /AiAgent/Ask
```

## Render

Este proyecto incluye `Dockerfile` y `render.yaml`. En Render, conectar GitHub y crear Blueprint desde `render.yaml`.

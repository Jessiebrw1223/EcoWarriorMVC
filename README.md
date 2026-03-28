# EcoWarriorMVC

Proyecto base en **ASP.NET Core MVC** creado tomando como referencia tu proyecto `EcoWarrior`.

## Qué incluye

- Estructura MVC limpia
- `HomeController` y `ProductosController`
- Modelos y ViewModels
- Servicio en memoria para productos
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

## Próximo paso recomendado

Agregar:
- `Data/ApplicationDbContext.cs`
- Entity Framework Core
- SQL Server
- CRUD completo de productos y categorías
- autenticación y panel admin

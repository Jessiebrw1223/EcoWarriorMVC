using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Data;

public static class DbInitializer
{
    public static void EnsureSeeded(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Aplicar las migraciones pendientes
        db.Database.Migrate();

        if (!db.Productos.Any())
        {
            db.Productos.AddRange(
                new Producto
                {
                    Nombre = "Botella reutilizable Eco",
                    Categoria = "Hogar sostenible",
                    Precio = 39.90m,
                    Stock = 120,
                    Descripcion = "Botella termica reutilizable fabricada con acero inoxidable para reducir el uso de plastico.",
                    ImagenUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=1200&q=80",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Kit de reciclaje domestico",
                    Categoria = "Reciclaje",
                    Precio = 89.00m,
                    Stock = 45,
                    Descripcion = "Set de contenedores compactos para separar residuos organicos, plasticos y papel.",
                    ImagenUrl = "https://images.unsplash.com/photo-1532996122724-e3c354a0b15b?auto=format&fit=crop&w=1200&q=80",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Panel solar portatil",
                    Categoria = "Energia verde",
                    Precio = 249.90m,
                    Stock = 18,
                    Descripcion = "Solucion liviana para cargar dispositivos con energia renovable en viajes y actividades al aire libre.",
                    ImagenUrl = "https://images.unsplash.com/photo-1509391366360-2e959784a276?auto=format&fit=crop&w=1200&q=80",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Bolsa compostable premium",
                    Categoria = "Consumo responsable",
                    Precio = 19.50m,
                    Stock = 250,
                    Descripcion = "Alternativa biodegradable para compras diarias y empaques ecologicos.",
                    ImagenUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=1200&q=80",
                    Activo = true
                }
            );
        }

        if (!db.Usuarios.Any())
        {
            db.Usuarios.AddRange(
                new Usuario
                {
                    Nombre = "Admin Eco",
                    Correo = "admin@ecowarrior.com",
                    Contrasena = "Eco12345",
                    Puntos = 3200,
                    RetosCompletados = 22,
                    CategoriaFavorita = "Energia verde"
                },
                new Usuario
                {
                    Nombre = "Luisa Verde",
                    Correo = "luisa@ecowarrior.com",
                    Contrasena = "Eco12345",
                    Puntos = 2750,
                    RetosCompletados = 19,
                    CategoriaFavorita = "Reciclaje"
                },
                new Usuario
                {
                    Nombre = "Marco Solar",
                    Correo = "marco@ecowarrior.com",
                    Contrasena = "Eco12345",
                    Puntos = 1980,
                    RetosCompletados = 14,
                    CategoriaFavorita = "Hogar sostenible"
                }
            );
        }

        if (!db.Badges.Any())
        {
            db.Badges.AddRange(
                new Badge
                {
                    Nombre = "Reciclador Principiante",
                    Descripcion = "Alcanza 100 puntos en actividades de reciclaje",
                    IconoUrl = "/images/badges/reciclador-principiante.png",
                    PuntosRequeridos = 100,
                    Categoria = "Reciclaje",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Reciclador Experto",
                    Descripcion = "Alcanza 500 puntos en actividades de reciclaje",
                    IconoUrl = "/images/badges/reciclador-experto.png",
                    PuntosRequeridos = 500,
                    Categoria = "Reciclaje",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Ahorrador de Energía",
                    Descripcion = "Alcanza 150 puntos en eficiencia energética",
                    IconoUrl = "/images/badges/ahorrador-energia.png",
                    PuntosRequeridos = 150,
                    Categoria = "Energía",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Guerrero Eco",
                    Descripcion = "Alcanza 1000 puntos totales en actividades eco-friendly",
                    IconoUrl = "/images/badges/guerrero-eco.png",
                    PuntosRequeridos = 1000,
                    Categoria = "Comunidad",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Transportista Verde",
                    Descripcion = "Alcanza 200 puntos en transporte sostenible",
                    IconoUrl = "/images/badges/transportista-verde.png",
                    PuntosRequeridos = 200,
                    Categoria = "Transporte",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Consumidor Consciente",
                    Descripcion = "Alcanza 300 puntos en consumo responsable",
                    IconoUrl = "/images/badges/consumidor-consciente.png",
                    PuntosRequeridos = 300,
                    Categoria = "Consumo",
                    Activo = true
                },
                new Badge
                {
                    Nombre = "Campeon Ambiental",
                    Descripcion = "Alcanza 2000 puntos totales y completa 20 retos",
                    IconoUrl = "/images/badges/campeon-ambiental.png",
                    PuntosRequeridos = 2000,
                    Categoria = "Comunidad",
                    Activo = true
                }
            );
        }

        db.SaveChanges();
    }
}

using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcoWarriorMVC.Data;

public static class DbInitializer
{
    public static void EnsureSeeded(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Aplicar las migraciones pendientes
        db.Database.Migrate();

        // Tablas nuevas para retos dinámicos. Se crean aquí para mantener compatibilidad
        // con bases ya desplegadas en Render sin depender de una migración manual.
        EnsureDynamicChallengeTables(db);

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
                    Contrasena = CrearHashContrasena("Eco12345"),
                    Puntos = 3200,
                    RetosCompletados = 22,
                    CategoriaFavorita = "Energia verde",
                    Ciudad = "Lima",
                    FotoUrl = ""
                },
                new Usuario
                {
                    Nombre = "Luisa Verde",
                    Correo = "luisa@ecowarrior.com",
                    Contrasena = CrearHashContrasena("Eco12345"),
                    Puntos = 2750,
                    RetosCompletados = 19,
                    CategoriaFavorita = "Reciclaje",
                    Ciudad = "Lima",
                    FotoUrl = ""
                },
                new Usuario
                {
                    Nombre = "Marco Solar",
                    Correo = "marco@ecowarrior.com",
                    Contrasena = CrearHashContrasena("Eco12345"),
                    Puntos = 1980,
                    RetosCompletados = 14,
                    CategoriaFavorita = "Hogar sostenible",
                    Ciudad = "Arequipa",
                    FotoUrl = ""
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


        if (!db.Retos.Any())
        {
            db.Retos.AddRange(
                new Reto
                {
                    Titulo = "Reciclar plastico",
                    Descripcion = "Recolecta y separa botellas PET en puntos de acopio autorizados.",
                    Puntos = 50,
                    Dificultad = "Easy",
                    Categoria = "Reciclaje",
                    Progreso = 85,
                    Participantes = 428,
                    Activo = true
                },
                new Reto
                {
                    Titulo = "Usar bicicleta",
                    Descripcion = "Sustituye el auto por bicicleta o transporte publico en un trayecto de al menos 5 km.",
                    Puntos = 150,
                    Dificultad = "Medium",
                    Categoria = "Movilidad",
                    Progreso = 60,
                    Participantes = 166,
                    Activo = true
                },
                new Reto
                {
                    Titulo = "Plantar un arbol",
                    Descripcion = "Participa en una jornada de reforestacion o siembra una planta nativa.",
                    Puntos = 500,
                    Dificultad = "Hard",
                    Categoria = "CO2",
                    Progreso = 24,
                    Participantes = 12,
                    Activo = true
                },
                new Reto
                {
                    Titulo = "Limpieza publica",
                    Descripcion = "Dedica 15 minutos a limpiar un espacio publico o parque cercano.",
                    Puntos = 100,
                    Dificultad = "Medium",
                    Categoria = "Residuos",
                    Progreso = 45,
                    Participantes = 312,
                    Activo = true
                }
            );
        }

        db.SaveChanges();
    }


    private static void EnsureDynamicChallengeTables(ApplicationDbContext db)
    {
        db.Database.ExecuteSqlRaw("""
            ALTER TABLE usuarios
            ADD COLUMN IF NOT EXISTS ciudad varchar(80) NOT NULL DEFAULT 'Lima';
        """);

        db.Database.ExecuteSqlRaw("""
            ALTER TABLE usuarios
            ADD COLUMN IF NOT EXISTS foto_url varchar(500) NOT NULL DEFAULT '';
        """);

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS retos (
                id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                titulo varchar(120) NOT NULL,
                descripcion varchar(500) NOT NULL,
                puntos integer NOT NULL DEFAULT 100,
                dificultad varchar(40) NOT NULL,
                categoria varchar(80) NOT NULL,
                progreso integer NOT NULL DEFAULT 0,
                participantes integer NOT NULL DEFAULT 0,
                activo boolean NOT NULL DEFAULT true,
                fecha_creacion timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
        """);

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS usuario_retos (
                id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                usuario_id integer NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
                reto_id integer NOT NULL REFERENCES retos(id) ON DELETE CASCADE,
                completado boolean NOT NULL DEFAULT false,
                fecha_completado timestamp with time zone NULL,
                CONSTRAINT uq_usuario_reto UNIQUE (usuario_id, reto_id)
            );
        """);
    }

    private static string CrearHashContrasena(string contrasena)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            contrasena,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

}

using EcoWarriorMVC.Data;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Services;

public class HomeService(IProductoService productoService, ApplicationDbContext dbContext) : IHomeService
{
    // Retos se pueden mover a BD en siguiente iteración; por ahora en memoria con estado real
    private static readonly List<RetoResponse> _retosBase =
    [
        new() { Id = 1, Titulo = "Semana sin plástico",    Descripcion = "Evita plásticos de un solo uso durante 7 días y registra cada vez que los rechazas.", Puntos = 120, Dificultad = "Media",  IconoEmoji = "♻️",  Categoria = "Reciclaje"   },
        new() { Id = 2, Titulo = "Reto de movilidad",      Descripcion = "Usa bicicleta o transporte público en lugar del auto durante 5 días consecutivos.",    Puntos = 90,  Dificultad = "Fácil",  IconoEmoji = "🚲",  Categoria = "Movilidad"   },
        new() { Id = 3, Titulo = "Huerto en casa",         Descripcion = "Inicia un mini huerto con al menos 3 plantas comestibles o aromáticas en casa.",         Puntos = 150, Dificultad = "Alta",   IconoEmoji = "🌱",  Categoria = "Naturaleza"  },
        new() { Id = 4, Titulo = "Limpieza pública",       Descripcion = "Dedica 30 minutos a recoger basura en un parque o espacio público de tu barrio.",        Puntos = 100, Dificultad = "Fácil",  IconoEmoji = "🧹",  Categoria = "Comunidad"   },
        new() { Id = 5, Titulo = "Ahorro de agua",         Descripcion = "Reduce tu consumo de agua a la mitad durante una semana. Duchas cortas y llave cerrada.", Puntos = 80,  Dificultad = "Media",  IconoEmoji = "💧",  Categoria = "AhorroAgua"  },
        new() { Id = 6, Titulo = "Menú cero residuos",     Descripcion = "Prepara al menos 3 comidas usando ingredientes locales, sin generar residuos plásticos.", Puntos = 110, Dificultad = "Media",  IconoEmoji = "🥗",  Categoria = "Alimentacion"},
    ];

    public HomeResumenResponse ObtenerResumen()
    {
        var productos = productoService.ObtenerTodos();
        return new HomeResumenResponse
        {
            Destacados        = productoService.ObtenerDestacados(),
            TotalProductos    = productos.Count,
            TotalCategorias   = productos.Select(x => x.Categoria).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            StockDisponible   = productos.Sum(x => x.Stock)
        };
    }

    public IReadOnlyList<RetoResponse> ObtenerRetos() =>
        _retosBase.Select(r => r with { }).ToList();

    public IReadOnlyList<RankingResponse> ObtenerRanking()
    {
        var usuarios = dbContext.Usuarios
            .AsNoTracking()
            .OrderByDescending(u => u.Puntos)
            .ToList();

        return usuarios
            .Select((u, i) => new RankingResponse
            {
                Posicion = i + 1,
                Usuario  = u.Nombre,
                Correo   = u.Correo,
                Puntos   = u.Puntos,
                RetosCompletados = u.RetosCompletados,
                Nivel    = CalcularNombreNivel(u.Puntos)
            })
            .ToList();
    }

    public PerfilResponse? ObtenerPerfil(string? correo = null)
    {
        Usuario? usuario;
        if (string.IsNullOrWhiteSpace(correo))
            usuario = dbContext.Usuarios.AsNoTracking().OrderByDescending(x => x.Puntos).FirstOrDefault();
        else
        {
            var cn = NormalizarCorreo(correo);
            usuario = dbContext.Usuarios.AsNoTracking().FirstOrDefault(x => x.Correo == cn);
        }
        return usuario is null ? null : ConstruirPerfil(usuario);
    }

    public LoginResponse? IniciarSesion(LoginViewModel model)
    {
        var cn = NormalizarCorreo(model.Correo);
        var usuario = dbContext.Usuarios.AsNoTracking()
            .FirstOrDefault(x => x.Correo == cn && x.Contrasena == model.Contrasena);
        if (usuario is null) return null;
        return new LoginResponse { Nombre = usuario.Nombre, Correo = usuario.Correo, Token = $"token-{Guid.NewGuid():N}" };
    }

    public OperacionResponse RegistrarUsuario(RegistroRequest request)
    {
        var cn = NormalizarCorreo(request.Correo);
        if (dbContext.Usuarios.Any(x => x.Correo == cn))
            return new OperacionResponse { Exito = false, Mensaje = "Ya existe una cuenta con ese correo." };

        dbContext.Usuarios.Add(new Usuario
        {
            Nombre = request.Nombre, Correo = cn, Contrasena = request.Contrasena,
            Puntos = 0, RetosCompletados = 0, CategoriaFavorita = "General"
        });
        dbContext.SaveChanges();
        return new OperacionResponse { Exito = true, Mensaje = "Registro completado." };
    }

    public OperacionResponse CompletarReto(string correo, int retoId)
    {
        var cn = NormalizarCorreo(correo);
        var usuario = dbContext.Usuarios.FirstOrDefault(x => x.Correo == cn);
        if (usuario is null) return new OperacionResponse { Exito = false, Mensaje = "Usuario no encontrado." };

        var reto = _retosBase.FirstOrDefault(r => r.Id == retoId);
        if (reto is null) return new OperacionResponse { Exito = false, Mensaje = "Reto no encontrado." };

        usuario.Puntos += reto.Puntos;
        usuario.RetosCompletados += 1;
        dbContext.SaveChanges();
        return new OperacionResponse { Exito = true, Mensaje = $"¡Reto completado! +{reto.Puntos} pts" };
    }

    public ContactoResponse RegistrarContacto(ContactoViewModel model) =>
        new() { Ticket = Guid.NewGuid().ToString("N"), FechaRegistroUtc = DateTime.UtcNow,
                Mensaje = $"Gracias {model.Nombre}, recibimos tu mensaje." };

    // ── helpers ──────────────────────────────────────────────────────────────
    private static PerfilResponse ConstruirPerfil(Usuario u) => new()
    {
        Nombre            = u.Nombre,
        Correo            = u.Correo,
        Puntos            = u.Puntos,
        RetosCompletados  = u.RetosCompletados,
        CategoriaFavorita = u.CategoriaFavorita,
        Nivel             = CalcularNombreNivel(u.Puntos),
        ReduccionCO2Kg    = Math.Round(u.RetosCompletados * 1.05, 1)
    };

    private static string CalcularNombreNivel(int puntos) => puntos switch
    {
        >= 3000 => "Leyenda Eco",
        >= 2000 => "Guardián Verde",
        >= 1000 => "Agente Sostenible",
        >= 500  => "Eco Explorer",
        _       => "Nuevo Recluta"
    };

    private static string NormalizarCorreo(string c) => c.Trim().ToLowerInvariant();
}

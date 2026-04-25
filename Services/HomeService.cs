using EcoWarriorMVC.Data;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Services;

public class HomeService(IProductoService productoService, ApplicationDbContext dbContext) : IHomeService
{
    private readonly List<RetoResponse> _retos =
    [
        new() { Id = 1, Titulo = "Semana sin plastico", Descripcion = "Evita plasticos de un solo uso durante 7 dias.", Puntos = 120, Dificultad = "Media", Completado = false },
        new() { Id = 2, Titulo = "Reto de movilidad", Descripcion = "Usa bicicleta o transporte publico por 5 dias.", Puntos = 90, Dificultad = "Facil", Completado = true },
        new() { Id = 3, Titulo = "Huerto en casa", Descripcion = "Inicia un mini huerto con al menos 3 plantas.", Puntos = 150, Dificultad = "Alta", Completado = false }
    ];

    public HomeResumenResponse ObtenerResumen()
    {
        var productos = productoService.ObtenerTodos();

        return new HomeResumenResponse
        {
            Destacados = productoService.ObtenerDestacados(),
            TotalProductos = productos.Count,
            TotalCategorias = productos.Select(x => x.Categoria).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            StockDisponible = productos.Sum(x => x.Stock)
        };
    }

    public IReadOnlyList<RetoResponse> ObtenerRetos()
    {
        return _retos
            .Select(r => new RetoResponse
            {
                Id = r.Id,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                Puntos = r.Puntos,
                Dificultad = r.Dificultad,
                Completado = r.Completado
            })
            .ToList();
    }

    public IReadOnlyList<RankingResponse> ObtenerRanking()
    {
        return dbContext.Usuarios
            .AsNoTracking()
            .OrderByDescending(u => u.Puntos)
            .Select((u, indice) => new RankingResponse
            {
                Posicion = indice + 1,
                Usuario = u.Nombre,
                Puntos = u.Puntos
            })
            .ToList();
    }

    public PerfilResponse? ObtenerPerfil(string? correo = null)
    {
        Usuario? usuario;

        if (string.IsNullOrWhiteSpace(correo))
        {
            usuario = dbContext.Usuarios
                .AsNoTracking()
                .OrderByDescending(x => x.Puntos)
                .FirstOrDefault();
        }
        else
        {
            var correoNormalizado = NormalizarCorreo(correo);
            usuario = dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefault(x => x.Correo == correoNormalizado);
        }

        return usuario is null ? null : ConstruirPerfil(usuario);
    }

    public LoginResponse? IniciarSesion(LoginViewModel model)
    {
        var correoNormalizado = NormalizarCorreo(model.Correo);
        var usuario = dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefault(x =>
                x.Correo == correoNormalizado &&
                x.Contrasena == model.Contrasena);

        if (usuario is null)
        {
            return null;
        }

        return new LoginResponse
        {
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Token = $"demo-token-{Guid.NewGuid():N}"
        };
    }

    public OperacionResponse RegistrarUsuario(RegistroRequest request)
    {
        var correoNormalizado = NormalizarCorreo(request.Correo);
        var existe = dbContext.Usuarios.Any(x => x.Correo == correoNormalizado);
        if (existe)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "Ya existe una cuenta con ese correo."
            };
        }

        dbContext.Usuarios.Add(new Usuario
        {
            Nombre = request.Nombre,
            Correo = correoNormalizado,
            Contrasena = request.Contrasena,
            Puntos = 0,
            RetosCompletados = 0,
            CategoriaFavorita = "Sin categoria"
        });
        dbContext.SaveChanges();

        return new OperacionResponse
        {
            Exito = true,
            Mensaje = "Registro completado correctamente."
        };
    }

    public ContactoResponse RegistrarContacto(ContactoViewModel model)
    {
        return new ContactoResponse
        {
            Ticket = Guid.NewGuid().ToString("N"),
            FechaRegistroUtc = DateTime.UtcNow,
            Mensaje = $"Gracias {model.Nombre}, recibimos tu mensaje y lo atenderemos pronto."
        };
    }

    private static PerfilResponse ConstruirPerfil(Usuario usuario)
    {
        return new PerfilResponse
        {
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Puntos = usuario.Puntos,
            RetosCompletados = usuario.RetosCompletados,
            CategoriaFavorita = usuario.CategoriaFavorita,
            Nivel = CalcularNivel(usuario.Puntos)
        };
    }

    private static string CalcularNivel(int puntos)
    {
        if (puntos >= 3000)
        {
            return "Leyenda eco";
        }

        if (puntos >= 2000)
        {
            return "Guardian verde";
        }

        if (puntos >= 1000)
        {
            return "Agente sostenible";
        }

        return "Nuevo recluta";
    }

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}

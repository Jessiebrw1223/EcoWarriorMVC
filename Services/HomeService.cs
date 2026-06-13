using EcoWarriorMVC.Data;
using EcoWarriorMVC.Models;
using EcoWarriorMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcoWarriorMVC.Services;

public class HomeService(IProductoService productoService, ApplicationDbContext dbContext) : IHomeService
{
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

    public IReadOnlyList<RetoResponse> ObtenerRetos(string? correo = null)
    {
        var usuario = ObtenerUsuarioPorCorreo(correo);
        var usuarioId = usuario?.Id ?? 0;

        var completados = usuarioId == 0
            ? new HashSet<int>()
            : dbContext.UsuarioRetos
                .AsNoTracking()
                .Where(x => x.UsuarioId == usuarioId && x.Completado)
                .Select(x => x.RetoId)
                .ToHashSet();

        return dbContext.Retos
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderByDescending(x => x.FechaCreacion)
            .ToList()
            .Select(r => new RetoResponse
            {
                Id = r.Id,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                Puntos = r.Puntos,
                Dificultad = r.Dificultad,
                Categoria = r.Categoria,
                Progreso = r.Progreso,
                Participantes = r.Participantes,
                Completado = completados.Contains(r.Id)
            })
            .ToList();
    }

    public OperacionResponse CrearReto(CrearRetoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "El titulo y la descripcion son obligatorios."
            };
        }

        var reto = new Reto
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Puntos = request.Puntos,
            Dificultad = string.IsNullOrWhiteSpace(request.Dificultad) ? "Media" : request.Dificultad.Trim(),
            Categoria = string.IsNullOrWhiteSpace(request.Categoria) ? "General" : request.Categoria.Trim(),
            Progreso = 0,
            Participantes = 0,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        dbContext.Retos.Add(reto);
        dbContext.SaveChanges();

        return new OperacionResponse
        {
            Exito = true,
            Mensaje = $"Mision '{reto.Titulo}' creada correctamente."
        };
    }

    public OperacionResponse CompletarReto(string? correo, int retoId)
    {
        var usuario = ObtenerUsuarioPorCorreo(correo);
        if (usuario is null)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "No se encontro un usuario activo."
            };
        }

        var reto = dbContext.Retos.FirstOrDefault(x => x.Id == retoId && x.Activo);
        if (reto is null)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "El reto seleccionado no existe."
            };
        }

        var usuarioReto = dbContext.UsuarioRetos
            .FirstOrDefault(x => x.UsuarioId == usuario.Id && x.RetoId == reto.Id);

        if (usuarioReto?.Completado == true)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "Este reto ya fue completado anteriormente."
            };
        }

        if (usuarioReto is null)
        {
            usuarioReto = new UsuarioReto
            {
                UsuarioId = usuario.Id,
                RetoId = reto.Id
            };
            dbContext.UsuarioRetos.Add(usuarioReto);
        }

        usuarioReto.Completado = true;
        usuarioReto.FechaCompletado = DateTime.UtcNow;

        usuario.Puntos += reto.Puntos;
        usuario.RetosCompletados += 1;

        if (!string.IsNullOrWhiteSpace(reto.Categoria))
        {
            usuario.CategoriaFavorita = reto.Categoria;
        }

        reto.Participantes += 1;
        reto.Progreso = Math.Min(100, Math.Max(reto.Progreso, reto.Progreso + 5));

        dbContext.SaveChanges();

        return new OperacionResponse
        {
            Exito = true,
            Mensaje = $"Reto completado. Sumaste {reto.Puntos} puntos."
        };
    }

    public IReadOnlyList<RankingResponse> ObtenerRanking()
    {
        return dbContext.Usuarios
            .AsNoTracking()
            .OrderByDescending(u => u.Puntos)
            .ThenBy(u => u.Nombre)
            .ToList()
            .Select((u, indice) => new RankingResponse
            {
                Posicion = indice + 1,
                Usuario = u.Nombre,
                Puntos = u.Puntos,
                Nivel = CalcularNivel(u.Puntos)
            })
            .ToList();
    }

    public PerfilResponse? ObtenerPerfil(string? correo = null)
    {
        var usuario = ObtenerUsuarioPorCorreo(correo);

        if (usuario is null && string.IsNullOrWhiteSpace(correo))
        {
            usuario = dbContext.Usuarios
                .AsNoTracking()
                .OrderByDescending(x => x.Puntos)
                .FirstOrDefault();
        }

        return usuario is null ? null : ConstruirPerfil(usuario);
    }

    public OperacionResponse ActualizarPerfil(string? correo, PerfilEditRequest request)
    {
        var usuario = ObtenerUsuarioPorCorreo(correo);
        if (usuario is null)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "No se encontro un usuario activo."
            };
        }

        var correoNormalizado = NormalizarCorreo(request.Correo);
        var correoUsado = dbContext.Usuarios
            .Any(x => x.Id != usuario.Id && x.Correo == correoNormalizado);

        if (correoUsado)
        {
            return new OperacionResponse
            {
                Exito = false,
                Mensaje = "Ya existe otro usuario con ese correo."
            };
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Correo = correoNormalizado;
        usuario.Ciudad = string.IsNullOrWhiteSpace(request.Ciudad) ? "Lima" : request.Ciudad.Trim();
        usuario.FotoUrl = string.IsNullOrWhiteSpace(request.FotoUrl) ? string.Empty : request.FotoUrl.Trim();
        usuario.CategoriaFavorita = string.IsNullOrWhiteSpace(request.CategoriaFavorita)
            ? "General"
            : request.CategoriaFavorita.Trim();

        dbContext.SaveChanges();

        return new OperacionResponse
        {
            Exito = true,
            Mensaje = "Perfil actualizado correctamente."
        };
    }

    public LoginResponse? IniciarSesion(LoginViewModel model)
    {
        var correoNormalizado = NormalizarCorreo(model.Correo);
        var usuario = dbContext.Usuarios
            .FirstOrDefault(x => x.Correo == correoNormalizado);

        if (usuario is null || !VerificarContrasena(model.Contrasena, usuario.Contrasena))
        {
            return null;
        }

        if (!usuario.Contrasena.StartsWith("PBKDF2$", StringComparison.Ordinal))
        {
            usuario.Contrasena = CrearHashContrasena(model.Contrasena);
            dbContext.SaveChanges();
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
            Contrasena = CrearHashContrasena(request.Contrasena),
            Puntos = 0,
            RetosCompletados = 0,
            CategoriaFavorita = "General",
            Ciudad = "Lima",
            FotoUrl = string.Empty
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

    private Usuario? ObtenerUsuarioPorCorreo(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return dbContext.Usuarios.OrderByDescending(x => x.Puntos).FirstOrDefault();
        }

        var correoNormalizado = NormalizarCorreo(correo);
        return dbContext.Usuarios.FirstOrDefault(x => x.Correo == correoNormalizado);
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
            Ciudad = string.IsNullOrWhiteSpace(usuario.Ciudad) ? "Lima" : usuario.Ciudad,
            FotoUrl = usuario.FotoUrl,
            Nivel = CalcularNivel(usuario.Puntos)
        };
    }

    private static string CalcularNivel(int puntos)
    {
        if (puntos >= 3000) return "Leyenda eco";
        if (puntos >= 2000) return "Guardian verde";
        if (puntos >= 1000) return "Agente sostenible";
        if (puntos >= 500) return "Explorador verde";
        return "Nuevo recluta";
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

    private static bool VerificarContrasena(string contrasenaIngresada, string contrasenaAlmacenada)
    {
        if (!contrasenaAlmacenada.StartsWith("PBKDF2$", StringComparison.Ordinal))
        {
            return contrasenaAlmacenada == contrasenaIngresada;
        }

        var partes = contrasenaAlmacenada.Split('$');
        if (partes.Length != 3)
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[1]);
        var hashAlmacenado = Convert.FromBase64String(partes[2]);
        var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(
            contrasenaIngresada,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(hashIngresado, hashAlmacenado);
    }

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}

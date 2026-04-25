using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMVC.Models;

public class HomeResumenResponse
{
    public IReadOnlyList<Producto> Destacados { get; set; } = [];
    public int TotalProductos { get; set; }
    public int TotalCategorias { get; set; }
    public int StockDisponible { get; set; }
}

public class RetoResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public string Dificultad { get; set; } = string.Empty;
    public bool Completado { get; set; }
}

public class RankingResponse
{
    public int Posicion { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int Puntos { get; set; }
}

public class PerfilResponse
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int RetosCompletados { get; set; }
    public string CategoriaFavorita { get; set; } = string.Empty;
}

public class RegistroRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ContactoResponse
{
    public string Ticket { get; set; } = string.Empty;
    public DateTime FechaRegistroUtc { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class OperacionResponse
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMVC.Models;

public class HomeResumenResponse
{
    public IReadOnlyList<Producto> Destacados { get; set; } = [];
    public int TotalProductos    { get; set; }
    public int TotalCategorias   { get; set; }
    public int StockDisponible   { get; set; }
}

public record RetoResponse
{
    public int    Id          { get; init; }
    public string Titulo      { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public int    Puntos      { get; init; }
    public string Dificultad  { get; init; } = string.Empty;
    public bool   Completado  { get; init; }
    public string IconoEmoji  { get; init; } = "🌱";
    public string Categoria   { get; init; } = string.Empty;
}

public class RankingResponse
{
    public int    Posicion         { get; set; }
    public string Usuario          { get; set; } = string.Empty;
    public string Correo           { get; set; } = string.Empty;
    public int    Puntos           { get; set; }
    public int    RetosCompletados { get; set; }
    public string Nivel            { get; set; } = string.Empty;
}

public class PerfilResponse
{
    public string Nombre            { get; set; } = string.Empty;
    public string Correo            { get; set; } = string.Empty;
    public string Nivel             { get; set; } = string.Empty;
    public int    Puntos            { get; set; }
    public int    RetosCompletados  { get; set; }
    public string CategoriaFavorita { get; set; } = string.Empty;
    public double ReduccionCO2Kg    { get; set; }
}

public class RegistroRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre    { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Correo    { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 6)]
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Token  { get; set; } = string.Empty;
}

public class ContactoResponse
{
    public string   Ticket          { get; set; } = string.Empty;
    public DateTime FechaRegistroUtc { get; set; }
    public string   Mensaje         { get; set; } = string.Empty;
}

public class OperacionResponse
{
    public bool   Exito   { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class CompletarRetoRequest
{
    public int RetoId { get; set; }
}

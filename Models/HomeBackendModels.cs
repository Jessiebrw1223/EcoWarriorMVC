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
    public string Categoria { get; set; } = string.Empty;
    public int Progreso { get; set; }
    public int Participantes { get; set; }
    public bool Completado { get; set; }
}

public class RankingResponse
{
    public int Posicion { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public bool EsUsuarioActual { get; set; }
}

public class PerfilResponse
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int RetosCompletados { get; set; }
    public string CategoriaFavorita { get; set; } = string.Empty;
    public string Ciudad { get; set; } = "Lima";
    public string FotoUrl { get; set; } = string.Empty;
}

public class CrearRetoRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(10, 1000, ErrorMessage = "Los puntos deben estar entre 10 y 1000.")]
    public int Puntos { get; set; } = 100;

    [Required(ErrorMessage = "La dificultad es obligatoria.")]
    public string Dificultad { get; set; } = "Media";

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public string Categoria { get; set; } = "General";
}

public class PerfilEditRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [StringLength(160, ErrorMessage = "El correo no debe superar los 160 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "La ciudad debe tener entre 2 y 80 caracteres.")]
    public string Ciudad { get; set; } = "Lima";

    [StringLength(500, ErrorMessage = "La URL de foto no debe superar los 500 caracteres.")]
    public string FotoUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "La preferencia ecológica es obligatoria.")]
    [StringLength(80, ErrorMessage = "La preferencia no debe superar los 80 caracteres.")]
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

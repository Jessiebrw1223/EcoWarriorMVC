namespace EcoWarriorMVC.Models;

public class Badge
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string IconoUrl { get; set; } = string.Empty;
    public int PuntosRequeridos { get; set; }
    public string Categoria { get; set; } = string.Empty; // Reciclaje, Energia, Transporte, etc.
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

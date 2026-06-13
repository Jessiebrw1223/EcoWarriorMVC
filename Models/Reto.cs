namespace EcoWarriorMVC.Models;

public class Reto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public string Dificultad { get; set; } = "Media";
    public string Categoria { get; set; } = "General";
    public int Progreso { get; set; } = 0;
    public int Participantes { get; set; } = 0;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

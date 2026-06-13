namespace EcoWarriorMVC.Models;

public class UsuarioReto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int RetoId { get; set; }
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }
}

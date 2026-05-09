namespace EcoWarriorMVC.Models;

public class UsuarioBadge
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int BadgeId { get; set; }
    public DateTime FechaObtencion { get; set; } = DateTime.UtcNow;
}

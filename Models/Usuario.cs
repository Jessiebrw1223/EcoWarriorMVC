namespace EcoWarriorMVC.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int RetosCompletados { get; set; }
    public string CategoriaFavorita { get; set; } = string.Empty;
}

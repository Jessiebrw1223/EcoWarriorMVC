namespace EcoWarriorMvc.Models;

public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredPoints { get; set; }
}

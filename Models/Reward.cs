namespace EcoWarriorMvc.Models;

public class Reward
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RequiredPoints { get; set; }
    public string Description { get; set; } = string.Empty;
}

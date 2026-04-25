namespace EcoWarriorMvc.Models;

public class AiLog
{
    public int Id { get; set; }
    public int? AppUserId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace EcoWarriorMvc.Models;

public class UserChallenge
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string Status { get; set; } = "Iniciado";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

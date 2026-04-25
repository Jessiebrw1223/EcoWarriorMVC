using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMvc.Models;

public class Challenge
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(700)] public string Description { get; set; } = string.Empty;
    [MaxLength(40)] public string Difficulty { get; set; } = "Fácil";
    [MaxLength(60)] public string Category { get; set; } = "Reciclaje";
    public int Points { get; set; }
    public int Participants { get; set; }
    public int EstimatedMinutes { get; set; }
    [MaxLength(180)] public string EcoImpact { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
    public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
}

using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMvc.Models;

public class AppUser
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(160)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(40)] public string Role { get; set; } = "Usuario";
    [MaxLength(80)] public string District { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Level { get; set; } = 1;
    public int DailyStreak { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
}

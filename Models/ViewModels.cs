using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMvc.Models;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required] public string District { get; set; } = string.Empty;
}

public class DashboardViewModel
{
    public AppUser? User { get; set; }
    public Challenge? RecommendedChallenge { get; set; }
    public int CompletedChallenges { get; set; }
    public int CurrentRank { get; set; }
}

public class AiQuestionViewModel
{
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
}

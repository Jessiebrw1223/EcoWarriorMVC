using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMvc.Models;

public class ContactMessage
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(160)] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(1200)] public string Message { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "Nuevo";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

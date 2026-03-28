using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMVC.ViewModels;

public class ContactoViewModel
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; } = string.Empty;
}

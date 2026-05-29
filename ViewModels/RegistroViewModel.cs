using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMVC.ViewModels;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Ingresa un nombre valido.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la contrasena.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contrasenas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}

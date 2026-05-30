using System.ComponentModel.DataAnnotations;

namespace EcoWarriorMVC.Models;

public class ProductoUpsertRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Precio { get; set; }

    [Range(0, 1000000)]
    public int Stock { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Descripcion { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    public string ImagenUrl { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

using EcoWarriorMVC.Models;

namespace EcoWarriorMVC.Services;

public interface IBadgeService
{
    IReadOnlyList<Badge> ObtenerTodos();
    Badge? ObtenerPorId(int id);
    IReadOnlyList<BadgeResponse> ObtenerBadgesUsuario(string correo);
    OperacionResponse DesbloquearBadge(int usuarioId, int badgeId);
    Badge Crear(Badge badge);
    Badge Actualizar(Badge badge);
}

public record BadgeResponse(
    int Id,
    string Nombre,
    string Descripcion,
    string IconoUrl,
    int PuntosRequeridos,
    string Categoria,
    DateTime FechaObtencion
);

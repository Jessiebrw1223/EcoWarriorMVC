using EcoWarriorMVC.Data;
using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Services;

public class BadgeService(ApplicationDbContext context) : IBadgeService
{
    public IReadOnlyList<Badge> ObtenerTodos()
    {
        return context.Badges
            .Where(b => b.Activo)
            .OrderBy(b => b.PuntosRequeridos)
            .ToList()
            .AsReadOnly();
    }

    public Badge? ObtenerPorId(int id)
    {
        return context.Badges.FirstOrDefault(b => b.Id == id && b.Activo);
    }

    public IReadOnlyList<BadgeResponse> ObtenerBadgesUsuario(string correo)
    {
        var usuario = context.Usuarios.FirstOrDefault(u => u.Correo == correo);
        if (usuario is null)
            return new List<BadgeResponse>().AsReadOnly();

        var badges = context.UsuarioBadges
            .Where(ub => ub.UsuarioId == usuario.Id)
            .Join(context.Badges,
                ub => ub.BadgeId,
                b => b.Id,
                (ub, b) => new BadgeResponse(
                    b.Id,
                    b.Nombre,
                    b.Descripcion,
                    b.IconoUrl,
                    b.PuntosRequeridos,
                    b.Categoria,
                    ub.FechaObtencion
                ))
            .OrderByDescending(b => b.FechaObtencion)
            .ToList();

        return badges.AsReadOnly();
    }

    public OperacionResponse DesbloquearBadge(int usuarioId, int badgeId)
    {
        var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        if (usuario is null)
            return new OperacionResponse { Exito = false, Mensaje = "Usuario no encontrado." };

        var badge = context.Badges.FirstOrDefault(b => b.Id == badgeId && b.Activo);
        if (badge is null)
            return new OperacionResponse { Exito = false, Mensaje = "Badge no encontrado." };

        if (usuario.Puntos < badge.PuntosRequeridos)
            return new OperacionResponse { Exito = false, Mensaje = $"Puntos insuficientes. Se requieren {badge.PuntosRequeridos} puntos." };

        var yaExiste = context.UsuarioBadges.Any(ub => ub.UsuarioId == usuarioId && ub.BadgeId == badgeId);
        if (yaExiste)
            return new OperacionResponse { Exito = false, Mensaje = "El usuario ya posee este badge." };

        var usuarioBadge = new UsuarioBadge
        {
            UsuarioId = usuarioId,
            BadgeId = badgeId,
            FechaObtencion = DateTime.UtcNow
        };

        context.UsuarioBadges.Add(usuarioBadge);
        context.SaveChanges();

        return new OperacionResponse { Exito = true, Mensaje = "Badge desbloqueado exitosamente." };
    }

    public Badge Crear(Badge badge)
    {
        badge.FechaCreacion = DateTime.UtcNow;
        context.Badges.Add(badge);
        context.SaveChanges();
        return badge;
    }

    public Badge Actualizar(Badge badge)
    {
        var existente = context.Badges.FirstOrDefault(b => b.Id == badge.Id);
        if (existente is null)
            throw new InvalidOperationException("Badge no encontrado.");

        existente.Nombre = badge.Nombre;
        existente.Descripcion = badge.Descripcion;
        existente.IconoUrl = badge.IconoUrl;
        existente.PuntosRequeridos = badge.PuntosRequeridos;
        existente.Categoria = badge.Categoria;
        existente.Activo = badge.Activo;

        context.SaveChanges();
        return existente;
    }
}

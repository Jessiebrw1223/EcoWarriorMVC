using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

[ApiController]
[Route("api/badges")]
public class BadgesApiController(IBadgeService badgeService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Badge>> ObtenerTodos()
    {
        var badges = badgeService.ObtenerTodos();
        return Ok(badges);
    }

    [HttpGet("{id}")]
    public ActionResult<Badge> ObtenerPorId(int id)
    {
        var badge = badgeService.ObtenerPorId(id);
        if (badge is null)
            return NotFound(new OperacionResponse
            {
                Exito = false,
                Mensaje = "Badge no encontrado."
            });

        return Ok(badge);
    }

    [HttpGet("usuario/{correo}")]
    public ActionResult<IReadOnlyList<BadgeResponse>> ObtenerBadgesUsuario(string correo)
    {
        var badges = badgeService.ObtenerBadgesUsuario(correo);
        return Ok(badges);
    }

    [HttpPost("desbloquear")]
    public ActionResult<OperacionResponse> DesbloquearBadge([FromBody] DesbloquearBadgeRequest request)
    {
        var resultado = badgeService.DesbloquearBadge(request.UsuarioId, request.BadgeId);
        
        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost]
    public ActionResult<Badge> Crear([FromBody] Badge badge)
    {
        var nuevoBadge = badgeService.Crear(badge);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoBadge.Id }, nuevoBadge);
    }

    [HttpPut("{id}")]
    public ActionResult<Badge> Actualizar(int id, [FromBody] Badge badge)
    {
        badge.Id = id;
        try
        {
            var badgeActualizado = badgeService.Actualizar(badge);
            return Ok(badgeActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new OperacionResponse
            {
                Exito = false,
                Mensaje = ex.Message
            });
        }
    }
}

public record DesbloquearBadgeRequest(int UsuarioId, int BadgeId);

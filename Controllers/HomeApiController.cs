using EcoWarriorMVC.Models;
using EcoWarriorMVC.Services;
using EcoWarriorMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMVC.Controllers;

[ApiController]
[Route("api/home")]
public class HomeApiController(IHomeService homeService) : ControllerBase
{
    [HttpGet("dashboard")]
    public ActionResult<HomeResumenResponse> Dashboard()
    {
        return Ok(homeService.ObtenerResumen());
    }

    [HttpGet("resumen")]
    public ActionResult<HomeResumenResponse> Resumen()
    {
        var resumen = homeService.ObtenerResumen();
        return Ok(resumen);
    }

    [HttpGet("retos")]
    public ActionResult<IReadOnlyList<RetoResponse>> Retos()
    {
        return Ok(homeService.ObtenerRetos());
    }

    [HttpGet("ranking")]
    public ActionResult<IReadOnlyList<RankingResponse>> Ranking()
    {
        return Ok(homeService.ObtenerRanking());
    }

    [HttpGet("perfil")]
    public ActionResult<PerfilResponse> Perfil([FromQuery] string? correo = null)
    {
        var perfil = homeService.ObtenerPerfil(correo);
        if (perfil is null)
        {
            return NotFound(new OperacionResponse
            {
                Exito = false,
                Mensaje = "No se encontro un perfil para el correo indicado."
            });
        }

        return Ok(perfil);
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginViewModel model)
    {
        var login = homeService.IniciarSesion(model);
        if (login is null)
        {
            return Unauthorized(new OperacionResponse
            {
                Exito = false,
                Mensaje = "Correo o contrasena incorrectos."
            });
        }

        return Ok(login);
    }

    [HttpPost("registro")]
    public ActionResult<OperacionResponse> Registro([FromBody] RegistroRequest request)
    {
        var resultado = homeService.RegistrarUsuario(request);
        if (!resultado.Exito)
        {
            return Conflict(resultado);
        }

        return CreatedAtAction(nameof(Perfil), new { correo = request.Correo }, resultado);
    }

    [HttpPost("contacto")]
    public ActionResult<ContactoResponse> Contacto([FromBody] ContactoViewModel model)
    {
        var respuesta = homeService.RegistrarContacto(model);
        return Accepted(respuesta);
    }

    [HttpGet("nosotros")]
    public IActionResult Nosotros()
    {
        return Ok(new
        {
            Proyecto = "EcoWarriorMVC",
            Descripcion = "Backend de soporte para las pantallas de Home (inicio, retos, ranking y perfil).",
            SiguientesPasos = new[]
            {
                "Agregar persistencia con Entity Framework",
                "Implementar autenticacion JWT real",
                "Conectar flujo de registro con base de datos"
            }
        });
    }
}

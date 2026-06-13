using EcoWarriorMVC.Models;
using EcoWarriorMVC.ViewModels;

namespace EcoWarriorMVC.Services;

public interface IHomeService
{
    HomeResumenResponse ObtenerResumen();
    IReadOnlyList<RetoResponse> ObtenerRetos(string? correo = null);
    IReadOnlyList<RankingResponse> ObtenerRanking();
    PerfilResponse? ObtenerPerfil(string? correo = null);
    LoginResponse? IniciarSesion(LoginViewModel model);
    OperacionResponse RegistrarUsuario(RegistroRequest request);
    ContactoResponse RegistrarContacto(ContactoViewModel model);
    OperacionResponse CrearReto(CrearRetoRequest request);
    OperacionResponse CompletarReto(string? correo, int retoId);
    OperacionResponse ActualizarPerfil(string? correo, PerfilEditRequest request);
}

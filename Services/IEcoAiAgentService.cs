namespace EcoWarriorMVC.Services;

public interface IEcoAiAgentService
{
    Task<string> ConsultarAsync(string pregunta, string? contextoCiudad = null, CancellationToken ct = default);
    Task<string> GenerarRetoPersonalizadoAsync(int puntosUsuario, string categoriaFavorita, CancellationToken ct = default);
}

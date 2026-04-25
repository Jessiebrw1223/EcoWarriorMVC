using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Services;

public class SemanticKernelService
{
    private readonly ApplicationDbContext _db;
    public SemanticKernelService(ApplicationDbContext db) => _db = db;

    public async Task<string> AskAsync(int? userId, string question)
    {
        var challenge = await _db.Challenges.OrderByDescending(c => c.IsPopular).ThenByDescending(c => c.Points).FirstOrDefaultAsync();
        var user = userId.HasValue ? await _db.Users.FindAsync(userId.Value) : null;
        var answer = $"EcoGuide AI: Te recomiendo completar '{challenge?.Name ?? "un reto ecológico"}'. Es ideal para ganar puntos, mantener tu progreso y generar impacto real en tu ciudad.";
        if (user != null) answer += $" Actualmente tienes {user.Points} puntos y nivel {user.Level}.";
        _db.AiLogs.Add(new AiLog { AppUserId = userId, Prompt = question, Response = answer });
        await _db.SaveChangesAsync();
        return answer;
    }
}

using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Services;

public class RecommendationService
{
    private readonly ApplicationDbContext _db;
    public RecommendationService(ApplicationDbContext db) => _db = db;
    public async Task<Challenge?> RecommendForUserAsync(int? userId)
    {
        var completedIds = userId.HasValue ? await _db.UserChallenges.Where(x => x.AppUserId == userId && x.Status == "Completado").Select(x => x.ChallengeId).ToListAsync() : new List<int>();
        return await _db.Challenges.Where(c => !completedIds.Contains(c.Id)).OrderByDescending(c => c.IsPopular).ThenByDescending(c => c.Points).FirstOrDefaultAsync();
    }
}

using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Services;

public class RankingService
{
    private readonly ApplicationDbContext _db;
    public RankingService(ApplicationDbContext db) => _db = db;
    public async Task<List<AppUser>> GetTopAsync() => await _db.Users.OrderByDescending(u => u.Points).ThenByDescending(u => u.DailyStreak).Take(20).ToListAsync();
    public async Task<int> GetRankAsync(int userId)
    {
        var ordered = await _db.Users.OrderByDescending(u => u.Points).Select(u => u.Id).ToListAsync();
        var idx = ordered.IndexOf(userId); return idx < 0 ? 0 : idx + 1;
    }
}

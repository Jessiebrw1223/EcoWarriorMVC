using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Controllers;

public class ChallengesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _auth;
    public ChallengesController(ApplicationDbContext db, AuthService auth) { _db = db; _auth = auth; }
    public async Task<IActionResult> Index(string? difficulty, string? category)
    {
        var query = _db.Challenges.AsQueryable();
        if (!string.IsNullOrWhiteSpace(difficulty)) query = query.Where(x => x.Difficulty == difficulty);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);
        return View(await query.OrderByDescending(x => x.IsPopular).ToListAsync());
    }
    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        if (!await _db.UserChallenges.AnyAsync(x => x.AppUserId == _auth.UserId && x.ChallengeId == id))
        {
            _db.UserChallenges.Add(new UserChallenge { AppUserId = _auth.UserId!.Value, ChallengeId = id });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
    [HttpGet("api/challenges")]
    public async Task<IActionResult> ApiChallenges() => Json(await _db.Challenges.ToListAsync());
}

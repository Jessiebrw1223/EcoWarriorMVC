using EcoWarriorMvc.Models;
using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class HomeController : Controller
{
    private readonly AuthService _auth;
    private readonly RecommendationService _recommendation;
    private readonly RankingService _ranking;
    public HomeController(AuthService auth, RecommendationService recommendation, RankingService ranking) { _auth = auth; _recommendation = recommendation; _ranking = ranking; }
    public async Task<IActionResult> Index()
    {
        var user = await _auth.CurrentUserAsync();
        var vm = new DashboardViewModel
        {
            User = user,
            RecommendedChallenge = await _recommendation.RecommendForUserAsync(user?.Id),
            CurrentRank = user == null ? 0 : await _ranking.GetRankAsync(user.Id),
            CompletedChallenges = user?.UserChallenges.Count(x => x.Status == "Completado") ?? 0
        };
        return View(vm);
    }
    public IActionResult Error() => View();
}

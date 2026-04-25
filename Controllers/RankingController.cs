using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class RankingController : Controller
{
    private readonly RankingService _ranking;
    public RankingController(RankingService ranking) => _ranking = ranking;
    public async Task<IActionResult> Index() => View(await _ranking.GetTopAsync());
    [HttpGet("api/ranking")]
    public async Task<IActionResult> ApiRanking() => Json(await _ranking.GetTopAsync());
}

using EcoWarriorMvc.Models;
using EcoWarriorMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class AiAgentController : Controller
{
    private readonly SemanticKernelService _sk;
    private readonly RecommendationService _recommendation;
    private readonly ClassificationService _classification;
    private readonly AuthService _auth;
    public AiAgentController(SemanticKernelService sk, RecommendationService recommendation, ClassificationService classification, AuthService auth)
    { _sk = sk; _recommendation = recommendation; _classification = classification; _auth = auth; }
    public IActionResult Index() => View(new AiQuestionViewModel());
    [HttpPost]
    public async Task<IActionResult> Ask(AiQuestionViewModel model)
    {
        model.Answer = await _sk.AskAsync(_auth.UserId, model.Question);
        return View("Index", model);
    }
    [HttpPost("api/recommendations")]
    public async Task<IActionResult> ApiRecommend() => Json(await _recommendation.RecommendForUserAsync(_auth.UserId));
    [HttpPost("api/classification")]
    public IActionResult ApiClassify([FromBody] Dictionary<string, string> payload) => Json(new { category = _classification.ClassifyChallenge(payload.GetValueOrDefault("text") ?? "") });
}

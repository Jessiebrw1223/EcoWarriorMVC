using EcoWarriorMvc.Data;
using EcoWarriorMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcoWarriorMvc.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;
    public ContactController(ApplicationDbContext db) => _db = db;
    public IActionResult Index() => View(new ContactMessage());
    [HttpPost]
    public async Task<IActionResult> Index(ContactMessage model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.ContactMessages.Add(model); await _db.SaveChangesAsync();
        ViewBag.Success = "Mensaje enviado correctamente.";
        return View(new ContactMessage());
    }
}

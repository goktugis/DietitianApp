using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietitianApp.Controllers
{
    [Authorize]
    public class MealLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MealLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danışan kendi yediklerini görür ve ekleyebilir
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Index()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var logs = await _context.MealLogs
                .Where(m => m.ClientId == clientId)
                .OrderByDescending(m => m.LogDate)
                .Take(20)
                .ToListAsync();

            return View(logs);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Add(MealLog model)
        {
            model.ClientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            model.LogDate = DateTime.UtcNow; // Veya alınan yerel saat

            if (ModelState.IsValid)
            {
                _context.MealLogs.Add(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Diyetisyen danışanının öğünlerini inceler
        [Authorize(Roles = "Dietitian")]
        public async Task<IActionResult> ClientMeals(string clientId)
        {
            var client = await _context.Users.FindAsync(clientId);
            if (client == null) return NotFound();

            var logs = await _context.MealLogs
                .Where(m => m.ClientId == clientId)
                .OrderByDescending(m => m.LogDate)
                .ToListAsync();

            ViewBag.ClientName = $"{client.Name} {client.Surname}";
            return View(logs);
        }
    }
}

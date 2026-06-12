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
            ViewBag.WaterGoal = client.DailyWaterGoal;
            
            // Bugünün su tüketimi
            var today = DateTime.Today;
            var loggedToday = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate.Date == today)
                .SumAsync(w => w.AmountMl);
            ViewBag.WaterLoggedToday = loggedToday;

            // Son 7 günün su tüketimi
            var last7Days = DateTime.Now.Date.AddDays(-6);
            var waterLogs = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate >= last7Days)
                .OrderBy(w => w.LogDate)
                .ToListAsync();

            var groupedWater = waterLogs
                .GroupBy(w => w.LogDate.Date)
                .Select(g => new {
                    Date = g.Key.ToString("dd MMM"),
                    Total = g.Sum(w => w.AmountMl)
                }).ToList();

            ViewBag.WaterLabels = groupedWater.Select(g => g.Date).ToList();
            ViewBag.WaterData = groupedWater.Select(g => g.Total).ToList();

            return View(logs);
        }
    }
}

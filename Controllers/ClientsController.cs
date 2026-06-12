using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietitianApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientUser = await _context.Users.FindAsync(clientId);

            // Yaklaşan randevular
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Dietitian)
                .Where(a => a.ClientId == clientId && a.Status == "Approved" && a.AppointmentDate >= DateTime.UtcNow.Date)
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingAppointments = upcomingAppointments;

            // Son 7 günün öğün istatistikleri (Grafik için)
            var last7Days = DateTime.Now.Date.AddDays(-6);
             var mealLogs = await _context.MealLogs
                .Where(m => m.ClientId == clientId && m.LogDate >= last7Days)
                .OrderBy(m => m.LogDate)
                .ToListAsync();

            // Gün bazında grupla (1 haftalık veri hazırlığı)
            var groupedLogs = mealLogs
                .GroupBy(m => m.LogDate.Date)
                .Select(g => new {
                    Date = g.Key.ToString("dd MMM"),
                    Count = g.Count()
                }).ToList();

            ViewBag.ChartLabels = groupedLogs.Select(g => g.Date).ToList();
            ViewBag.ChartData = groupedLogs.Select(g => g.Count).ToList();

            // Su takip istatistikleri
            var today = DateTime.Today;
            var loggedToday = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate.Date == today)
                .SumAsync(w => w.AmountMl);

            ViewBag.WaterGoal = clientUser?.DailyWaterGoal ?? 2000;
            ViewBag.WaterLogged = loggedToday;

            return View();
        }

        // Tüm diyetisyenleri listeleme (Herkes veya sadece giriş yapmış kişiler görebilir)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var dietitians = _context.DietitianProfiles
                .Include(dp => dp.User)
                .Where(dp => dp.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                dietitians = dietitians.Where(dp => 
                    (dp.User != null && dp.User.Name != null && dp.User.Name.Contains(searchString)) || 
                    (dp.User != null && dp.User.Surname != null && dp.User.Surname.Contains(searchString)) ||
                    (dp.Specializations != null && dp.Specializations.Contains(searchString)));
            }

            return View(await dietitians.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> DietitianDetails(int id)
        {
            var profile = await _context.DietitianProfiles
                .Include(dp => dp.User)
                .Include(dp => dp.Reviews).ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(dp => dp.Id == id && dp.IsApproved);

            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> GetWaterStatus()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Users.FindAsync(clientId);
            if (client == null) return NotFound();

            var today = DateTime.Today;
            var loggedToday = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate.Date == today)
                .SumAsync(w => w.AmountMl);

            return Json(new {
                goal = client.DailyWaterGoal,
                current = loggedToday
            });
        }

        [HttpPost]
        public async Task<IActionResult> LogWater([FromBody] LogWaterRequest model)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Users.FindAsync(clientId);
            if (client == null) return NotFound();

            if (model == null || model.AmountMl <= 0) return BadRequest("Geçersiz miktar");

            var log = new WaterLog
            {
                ClientId = clientId!,
                LogDate = DateTime.Now,
                AmountMl = model.AmountMl
            };

            _context.WaterLogs.Add(log);
            await _context.SaveChangesAsync();

            var today = DateTime.Today;
            var loggedToday = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate.Date == today)
                .SumAsync(w => w.AmountMl);

            return Json(new {
                goal = client.DailyWaterGoal,
                current = loggedToday
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWaterGoal([FromBody] UpdateGoalRequest model)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Users.FindAsync(clientId);
            if (client == null) return NotFound();

            if (model == null || model.GoalMl < 500 || model.GoalMl > 10000) return BadRequest("Hedef 500 ml ile 10000 ml arasında olmalıdır.");

            client.DailyWaterGoal = model.GoalMl;
            await _context.SaveChangesAsync();

            var today = DateTime.Today;
            var loggedToday = await _context.WaterLogs
                .Where(w => w.ClientId == clientId && w.LogDate.Date == today)
                .SumAsync(w => w.AmountMl);

            return Json(new {
                goal = client.DailyWaterGoal,
                current = loggedToday
            });
        }
    }

    public class LogWaterRequest
    {
        public int AmountMl { get; set; }
    }

    public class UpdateGoalRequest
    {
        public int GoalMl { get; set; }
    }
}

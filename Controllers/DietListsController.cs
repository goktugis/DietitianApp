using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietitianApp.Controllers
{
    [Authorize]
    public class DietListsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DietListsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danışanlar listelerini görebilir
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Index()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lists = await _context.DietLists
                .Include(d => d.Dietitian)
                .Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(lists);
        }

        // Diyetisyen bir danışana yeni liste atar
        [Authorize(Roles = "Dietitian")]
        [HttpGet]
        public async Task<IActionResult> Assign(string clientId)
        {
            var client = await _context.Users.FindAsync(clientId);
            if (client == null) return NotFound();

            ViewBag.ClientName = $"{client.Name} {client.Surname}";
            
            var model = new DietList
            {
                ClientId = clientId,
                ValidFrom = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(7)
            };
            return View(model);
        }

        [Authorize(Roles = "Dietitian")]
        [HttpPost]
        public async Task<IActionResult> Assign(DietList model)
        {
            model.DietitianId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            if (ModelState.IsValid)
            {
                _context.DietLists.Add(model);

                // Danışana Bildirim Gönder
                var dietitianUser = await _context.Users.FindAsync(model.DietitianId);
                var notif = new Notification
                {
                    UserId = model.ClientId,
                    Title = "Yeni Diyet Listesi",
                    Message = $"{(dietitianUser != null ? "Dyt. " + dietitianUser.Name : "Diyetisyeniniz")} sizin için yeni bir diyet listesi hazırladı.",
                    ActionUrl = "/DietLists"
                };
                _context.Notifications.Add(notif);

                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Diyet listesi başarıyla danışana atandı.";
                return RedirectToAction("Dashboard", "Dietitians");
            }

            return View(model);
        }
    }
}

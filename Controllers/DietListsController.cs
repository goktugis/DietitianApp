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

        // Danışanlar kendi listelerini görebilir, Diyetisyen de danışanının listelerini görebilir
        [Authorize(Roles = "Client,Dietitian")]
        public async Task<IActionResult> Index(string? clientId)
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isDietitian = User.IsInRole("Dietitian");

            if (!isDietitian)
            {
                clientId = myUserId;
            }
            
            if (string.IsNullOrEmpty(clientId)) return BadRequest("Danışan ID'si belirtilmedi.");

            var lists = await _context.DietLists
                .Include(d => d.Dietitian)
                .Include(d => d.Client)
                .Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            if (isDietitian)
            {
                var client = await _context.Users.FindAsync(clientId);
                if (client != null) ViewBag.DietitianClientName = $"{client.Name} {client.Surname}";
                ViewBag.IsDietitian = true;
                ViewBag.TargetClientId = clientId;
            }
            else
            {
                ViewBag.IsDietitian = false;
            }

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

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isDietitian = User.IsInRole("Dietitian");

            var dietList = await _context.DietLists
                .Include(d => d.Meals.OrderBy(m => m.Time))
                .Include(d => d.Client)
                .Include(d => d.Dietitian)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dietList == null) return NotFound();

            if (!isDietitian && dietList.ClientId != userId) return Forbid();
            if (isDietitian && dietList.DietitianId != userId) return Forbid();

            return View(dietList);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CompleteMeal(int mealId, IFormFile? photo)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var meal = await _context.DietMeals
                .Include(m => m.DietList)
                .FirstOrDefaultAsync(m => m.Id == mealId);

            if (meal == null || meal.DietList?.ClientId != clientId) return NotFound();

            meal.IsCompleted = true;

            if (photo != null && photo.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest("Sadece .jpg, .jpeg, .png, .gif veya .webp formatında resim yükleyebilirsiniz.");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "meals");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }

                meal.ClientPhotoUrl = "/images/meals/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = meal.DietListId });
        }
    }
}

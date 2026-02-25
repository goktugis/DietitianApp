using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DietitianApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Bekleyen (Onaylanmamış) diyetisyen profillerini getir
            var pendingDietitians = await _context.DietitianProfiles
                .Include(dp => dp.User)
                .Where(dp => !dp.IsApproved)
                .ToListAsync();

            return View(pendingDietitians);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDietitian(int profileId)
        {
            var profile = await _context.DietitianProfiles.Include(dp => dp.User).FirstOrDefaultAsync(dp => dp.Id == profileId);
            if (profile != null)
            {
                profile.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{profile.User?.Name} {profile.User?.Surname} adlı diyetisyen onaylandı.";
            }

            return RedirectToAction("Index");
        }
    }
}

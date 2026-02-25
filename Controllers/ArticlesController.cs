using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DietitianApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .Include(a => a.Dietitian)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(articles);
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var article = await _context.Articles
                .Include(a => a.Dietitian)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (article == null) return NotFound();

            return View(article);
        }

        // GET: Articles/Create (Only Dietitians)
        [Authorize(Roles = "Dietitian")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Dietitian")]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageUrl")] Article article)
        {
             var user = await _userManager.GetUserAsync(User);
             if (user == null) return Challenge();
             
             article.DietitianId = user.Id;
             article.CreatedAt = DateTime.Now;

             if (ModelState.IsValid)
             {
                 _context.Add(article);
                 await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Makale başarıyla paylaşıldı.";
                 return RedirectToAction(nameof(Index));
             }
             return View(article);
        }

        // GET: Articles/Delete/5
        [Authorize(Roles = "Dietitian,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var article = await _context.Articles
                .Include(a => a.Dietitian)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (article == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            // Sadece makaleyi yazan kişi veya Admin silebilir
            if (article.DietitianId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Dietitian,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Makale silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

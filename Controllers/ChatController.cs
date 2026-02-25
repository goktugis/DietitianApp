using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietitianApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Chat?dietitianId=123 (Danışan girer) 
        // GET: /Chat?clientId=456 (Diyetisyen girer)
        public async Task<IActionResult> Index(string? dietitianId, string? clientId)
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string otherUserId = dietitianId ?? clientId ?? "";

            if (string.IsNullOrEmpty(otherUserId))
            {
                return BadRequest("Geçersiz kullanıcı ID'si.");
            }

            var otherUser = await _context.Users.FindAsync(otherUserId);
            if (otherUser == null) return NotFound();

            // Geçmiş mesajları getir
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == myUserId && m.ReceiverId == otherUserId) || 
                            (m.SenderId == otherUserId && m.ReceiverId == myUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.OtherUser = otherUser;
            ViewBag.MyUserId = myUserId;

            return View(messages);
        }

        // GET: Chat/VideoCall/5
        [Authorize]
        public async Task<IActionResult> VideoCall(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var otherUser = await _userManager.FindByIdAsync(id);
            if (otherUser == null) return NotFound();

            var myUser = await _userManager.GetUserAsync(User);
            if (myUser == null) return Challenge();

            // Benzersiz bir Jitsi Meet oda adı oluştur. Alfabetik sıralayarak iki kişi için de oda isminin aynı olmasını sağla.
            var userIds = new List<string> { myUser.Id, otherUser.Id };
            userIds.Sort(); 
            var roomName = $"DietitianApp_Room_{userIds[0].Substring(0,8)}_{userIds[1].Substring(0,8)}";

            ViewBag.RoomName = roomName;
            ViewBag.OtherUser = otherUser;
            ViewBag.MyName = $"{myUser.Name} {myUser.Surname}";

            return View();
        }
    }
}

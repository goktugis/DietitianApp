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

        // GET: /Chat?userId=123
        public async Task<IActionResult> Index(string userId)
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string otherUserId = userId ?? "";

            if (string.IsNullOrEmpty(otherUserId))
            {
                return BadRequest("Geçersiz kullanıcı ID'si.");
            }

            var otherUser = await _context.Users.FindAsync(otherUserId);
            if (otherUser == null) return NotFound();

            // Güvenlik ve Gizlilik: Kimlerin Kiminle Konuşabileceği Kontrolü
            var myUser = await _userManager.GetUserAsync(User);
            if (myUser == null) return Challenge();
            
            var myRoles = await _userManager.GetRolesAsync(myUser);
            var otherRoles = await _userManager.GetRolesAsync(otherUser);

            bool isMyClient = myRoles.Contains("Client");
            bool isOtherClient = otherRoles.Contains("Client");

            // Müşteri -> Müşteri veya Diyetisyen -> Diyetisyen konuşmalarını engelle
            if (isMyClient == isOtherClient)
            {
                return Forbid(); // Sadece Danışan-Diyetisyen etkileşimine izin var.
            }

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

            // Güvenlik: Görüntülü görüşme sadece Danışan-Diyetisyen arasında olmalıdır
            var myRoles = await _userManager.GetRolesAsync(myUser);
            var otherRoles = await _userManager.GetRolesAsync(otherUser);

            bool isMyClient = myRoles.Contains("Client");
            bool isOtherClient = otherRoles.Contains("Client");

            if (isMyClient == isOtherClient)
            {
                return Forbid(); // Sadece yetkili eşleşmelere izin var.
            }

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

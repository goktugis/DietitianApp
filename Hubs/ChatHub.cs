using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DietitianApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
            {
                return;
            }

            // Veritabanına kaydet
            var chatMsg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMsg);

            // Alıcıya Bildirim Ekleyelim (SignalR hub olduğu için DbContext thread safe kullanılmalı ama Scoped olduğu için sorunsuz çalışır)
            var senderUser = await _context.Users.FindAsync(senderId);
            
            // Eğer gönderen Diyetisyen ise clientId parametresiyle link verilecek, Danışan ise dietitianId ile
            // Basitlik adına genel bir Chat url'si veya doğrudan gelen kişinin ID'si yazılır
            var notifUrl = $"/Chat?dietitianId={senderId}"; // Fallback (Tam doğrusu rol kontrolüyle olur)

            var notif = new Notification
            {
                UserId = receiverId,
                Title = "Yeni Mesaj",
                Message = $"{senderUser?.Name} size yeni bir mesaj gönderdi.",
                ActionUrl = $"/Chat?userId={senderId}"
            };
            _context.Notifications.Add(notif);

            await _context.SaveChangesAsync();

            // Sadece receiver online ise ona gönder
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, chatMsg.SentAt.ToString("HH:mm"));

            // Kendisine de göndersin (UI'de görünmesi için)
            // Veya UI bunu kendi local state'inden halledebilir. Biz Hub'dan onay olarak döndürelim:
            await Clients.Caller.SendAsync("MessageSent", chatMsg.Id, message, chatMsg.SentAt.ToString("HH:mm"));
        }
    }
}

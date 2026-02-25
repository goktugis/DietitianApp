namespace DietitianApp.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser? Receiver { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

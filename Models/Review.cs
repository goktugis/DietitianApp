namespace DietitianApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        public string DietitianId { get; set; } = string.Empty;
        public ApplicationUser? Dietitian { get; set; }

        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

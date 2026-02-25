namespace DietitianApp.Models
{
    public class DietList
    {
        public int Id { get; set; }
        
        public string DietitianId { get; set; } = string.Empty;
        public ApplicationUser? Dietitian { get; set; }

        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; } // Rich text or simple text
        public string? DocumentUrl { get; set; } // Orijinal PDF vs eklendiyse
        
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

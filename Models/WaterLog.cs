using System;

namespace DietitianApp.Models
{
    public class WaterLog
    {
        public int Id { get; set; }
        
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }
        
        public DateTime LogDate { get; set; }
        public int AmountMl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

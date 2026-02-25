namespace DietitianApp.Models
{
    public class MealLog
    {
        public int Id { get; set; }
        
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        public DateTime LogDate { get; set; }
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

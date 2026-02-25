using Microsoft.AspNetCore.Identity;

namespace DietitianApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // "Client" or "Dietitian" or "Admin" role is managed via IdentityRole, 
        // but we can add navigations for specific features.
        
        public DietitianProfile? DietitianProfile { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace DietitianApp.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen makale başlığı giriniz.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen makale içeriği giriniz.")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public string DietitianId { get; set; } = string.Empty;

        // Navigation property
        public ApplicationUser? Dietitian { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace DietitianApp.Models
{
    public class DietMeal
    {
        public int Id { get; set; }
        
        public int DietListId { get; set; }
        public DietList? DietList { get; set; }

        [Required(ErrorMessage = "Gün adı zorunludur.")]
        public string DayName { get; set; } = "Her Gün"; // Pazartesi, Salı vb. veya "Her Gün"

        [Required(ErrorMessage = "Öğün adı zorunludur.")]
        public string MealName { get; set; } = string.Empty; // Örn: Sabah Kahvaltısı, 1. Ara Öğün
        
        [Required(ErrorMessage = "Saat zorunludur.")]
        public TimeSpan Time { get; set; } // Örn: 08:30

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Content { get; set; } = string.Empty; // Örn: 2 yumurta, 1 dilim peynir
        
        public bool IsCompleted { get; set; } = false; // Danışan tamamladı mı?
        public string? ClientPhotoUrl { get; set; } // Tamamlandığında yüklenen fotoğraf
    }
}

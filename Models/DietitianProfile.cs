namespace DietitianApp.Models
{
    public class DietitianProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string? Specializations { get; set; }
        public string? Biography { get; set; }
        public string? DiplomaUrl { get; set; }
        public bool IsApproved { get; set; } = false;
        public decimal ConsultationFee { get; set; }
        public double AverageRating { get; set; } = 0;

        // Randevular (Bu profilin ait olduğu diyetisyene alınan randevular)
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<DietList> DietLists { get; set; } = new List<DietList>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

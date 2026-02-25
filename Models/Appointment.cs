namespace DietitianApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        public string DietitianId { get; set; } = string.Empty;
        public ApplicationUser? Dietitian { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        // Pending, Approved, Cancelled, Completed
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

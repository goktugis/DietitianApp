using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DietitianApp.Models;

namespace DietitianApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DietitianProfile> DietitianProfiles { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<DietList> DietLists { get; set; }
        public DbSet<MealLog> MealLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DietitianProfile>()
                .Property(p => p.ConsultationFee)
                .HasColumnType("decimal(18,2)");

            // Configure DietitianProfile -> ApplicationUser One-to-One
            builder.Entity<DietitianProfile>()
                .HasOne(dp => dp.User)
                .WithOne(u => u.DietitianProfile)
                .HasForeignKey<DietitianProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment limits tracking
            builder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Dietitian)
                .WithMany()
                .HasForeignKey(a => a.DietitianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<DietList>()
                .HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<DietList>()
                .HasOne(d => d.Dietitian)
                .WithMany()
                .HasForeignKey(d => d.DietitianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MealLog>()
                .HasOne(m => m.Client)
                .WithMany()
                .HasForeignKey(m => m.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Dietitian)
                .WithMany()
                .HasForeignKey(r => r.DietitianId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

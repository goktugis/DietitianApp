using DietitianApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DietitianApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure DB is updated
            await dbContext.Database.MigrateAsync();

            string[] roles = { "Admin", "Dietitian", "Client" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create Admin
            var adminUser = await userManager.FindByEmailAsync("admin@diyetisyenapp.com");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@diyetisyenapp.com",
                    Email = "admin@diyetisyenapp.com",
                    Name = "Sistem",
                    Surname = "Yöneticisi",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create Example Dietitian
            var dietitianUser = await userManager.FindByEmailAsync("uzman@diyetisyenapp.com");
            if (dietitianUser == null)
            {
                var dietitian = new ApplicationUser
                {
                    UserName = "uzman@diyetisyenapp.com",
                    Email = "uzman@diyetisyenapp.com",
                    Name = "Ayşe",
                    Surname = "Yılmaz",
                    EmailConfirmed = true,
                    DietitianProfile = new DietitianProfile
                    {
                        IsApproved = true,
                        ConsultationFee = 500,
                        Biography = "Hacettepe Üniversitesi Beslenme ve Diyetetik bölümü mezunuyum. 10 yıldır obezite cerrahisi sonrası beslenme, sporcu beslenmesi ve hastalıklarda diyet tedavisi üzerine çalışıyorum.",
                        Specializations = "Kilo Verme, Sporcu Beslenmesi, Diyabet",
                        AverageRating = 4.8,
                    }
                };
                await userManager.CreateAsync(dietitian, "Uzman123!");
                await userManager.AddToRoleAsync(dietitian, "Dietitian");
            }

            // Create Example Client
            var clientUser = await userManager.FindByEmailAsync("danisan@diyetisyenapp.com");
            if (clientUser == null)
            {
                var client = new ApplicationUser
                {
                    UserName = "danisan@diyetisyenapp.com",
                    Email = "danisan@diyetisyenapp.com",
                    Name = "Mehmet",
                    Surname = "Kaya",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(client, "Danisan123!");
                await userManager.AddToRoleAsync(client, "Client");
            }
        }
    }
}

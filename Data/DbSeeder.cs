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

            // Create Example Dietitian 1 (Ayşe Yılmaz)
            var dietitianUser = await userManager.FindByEmailAsync("uzman@diyetisyenapp.com");
            if (dietitianUser == null)
            {
                var dietitian = new ApplicationUser
                {
                    UserName = "uzman@diyetisyenapp.com",
                    Email = "uzman@diyetisyenapp.com",
                    Name = "Ayşe",
                    Surname = "Yılmaz",
                    ProfileImageUrl = "/images/profiles/ayse.png",
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

            // Create Example Dietitian 2 (Ahmet Demir)
            var dietitian2User = await userManager.FindByEmailAsync("uzman2@diyetisyenapp.com");
            if (dietitian2User == null)
            {
                var dietitian = new ApplicationUser
                {
                    UserName = "uzman2@diyetisyenapp.com",
                    Email = "uzman2@diyetisyenapp.com",
                    Name = "Ahmet",
                    Surname = "Demir",
                    ProfileImageUrl = "/images/profiles/ahmet.png",
                    EmailConfirmed = true,
                    DietitianProfile = new DietitianProfile
                    {
                        IsApproved = true,
                        ConsultationFee = 600,
                        Biography = "Ege Üniversitesi Beslenme ve Diyetetik mezunuyum. Sporcu beslenmesi ve fonksiyonel tıp alanlarında 8 yıllık tecrübeye sahibim.",
                        Specializations = "Sporcu Beslenmesi, Kilo Alma, Fonksiyonel Tıp",
                        AverageRating = 4.9,
                    }
                };
                await userManager.CreateAsync(dietitian, "Uzman123!");
                await userManager.AddToRoleAsync(dietitian, "Dietitian");
            }

            // Create Example Dietitian 3 (Elif Çelik)
            var dietitian3User = await userManager.FindByEmailAsync("uzman3@diyetisyenapp.com");
            if (dietitian3User == null)
            {
                var dietitian = new ApplicationUser
                {
                    UserName = "uzman3@diyetisyenapp.com",
                    Email = "uzman3@diyetisyenapp.com",
                    Name = "Elif",
                    Surname = "Çelik",
                    ProfileImageUrl = "/images/profiles/elif.png",
                    EmailConfirmed = true,
                    DietitianProfile = new DietitianProfile
                    {
                        IsApproved = true,
                        ConsultationFee = 450,
                        Biography = "Ankara Üniversitesi Beslenme ve Diyetetik bölümünden mezun oldum. Anne-çocuk beslenmesi ve yeme bozuklukları üzerine odaklanıyorum.",
                        Specializations = "Kilo Verme, Anne-Çocuk Beslenmesi, Yeme Bozuklukları",
                        AverageRating = 4.7,
                    }
                };
                await userManager.CreateAsync(dietitian, "Uzman123!");
                await userManager.AddToRoleAsync(dietitian, "Dietitian");
            }

            // Create Example Client 1
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

            // Create Example Client 2
            var clientUser2 = await userManager.FindByEmailAsync("danisan2@diyetisyenapp.com");
            if (clientUser2 == null)
            {
                var client = new ApplicationUser
                {
                    UserName = "danisan2@diyetisyenapp.com",
                    Email = "danisan2@diyetisyenapp.com",
                    Name = "Can",
                    Surname = "Demir",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(client, "Danisan123!");
                await userManager.AddToRoleAsync(client, "Client");
            }

            // Create Example Client 3
            var clientUser3 = await userManager.FindByEmailAsync("danisan3@diyetisyenapp.com");
            if (clientUser3 == null)
            {
                var client = new ApplicationUser
                {
                    UserName = "danisan3@diyetisyenapp.com",
                    Email = "danisan3@diyetisyenapp.com",
                    Name = "Zeynep",
                    Surname = "Arslan",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(client, "Danisan123!");
                await userManager.AddToRoleAsync(client, "Client");
            }

            // Create Recipe Articles
            if (!await dbContext.Articles.AnyAsync())
            {
                var ayseDyt = await userManager.FindByEmailAsync("uzman@diyetisyenapp.com");
                var ahmetDyt = await userManager.FindByEmailAsync("uzman2@diyetisyenapp.com");
                var elifDyt = await userManager.FindByEmailAsync("uzman3@diyetisyenapp.com");

                if (ayseDyt != null)
                {
                    dbContext.Articles.Add(new Article
                    {
                        Title = "Çilekli Chia Puding Tarifi",
                        Content = "Chia tohumları lif, omega-3 ve protein bakımından mükemmel bir kaynaktır.\n\nMalzemeler:\n- 3 yemek kaşığı chia tohumu\n- 1 su bardağı badem sütü (veya normal süt)\n- 1 tatlı kaşığı bal veya akçaağaç şurubu\n- 5-6 adet taze çilek\n\nHazırlanışı:\nBir kavanozda chia tohumlarını, sütü ve balı karıştırın. Karışımı 30 dakika buzdolabında bekletip tekrar karıştırın (topaklanmaması için). Ardından en az 4 saat veya bir gece buzdolabında bekletin. Jöle kıvamına gelen pudinginizi çilek dilimleri ile süsleyerek servis edin. Afiyet olsun!",
                        ImageUrl = "/images/recipes/chia_pudding.png",
                        DietitianId = ayseDyt.Id,
                        CreatedAt = DateTime.Now
                    });
                }

                if (ahmetDyt != null)
                {
                    dbContext.Articles.Add(new Article
                    {
                        Title = "Fırında Kuşkonmazlı Somon",
                        Content = "Somon, omega-3 yağ asitleri açısından çok zengindir ve protein ihtiyacınızı en sağlıklı şekilde karşılar.\n\nMalzemeler:\n- 1 adet somon fileto\n- 6-7 adet kuşkonmaz\n- 1 yemek kaşığı zeytinyağı\n- 1/2 limon\n- Tuz, karabiber, kekik\n\nHazırlanışı:\nFırın tepsisine yağlı kağıt serin. Somon filotoyu ve yıkanıp temizlenmiş kuşkonmazları yerleştirin. Üzerlerine zeytinyağı gezdirip tuz, karabiber ve kekik serpiştirin. Limon dilimlerini somonun üzerine koyun. Önceden ısıtılmış 200 derece fırında yaklaşık 15-20 dakika pişirin. Sıcak servis yapın. Afiyet olsun!",
                        ImageUrl = "/images/recipes/salmon.png",
                        DietitianId = ahmetDyt.Id,
                        CreatedAt = DateTime.Now
                    });
                }

                if (elifDyt != null)
                {
                    dbContext.Articles.Add(new Article
                    {
                        Title = "Detoks Yeşil Smoothie",
                        Content = "Güne enerjik başlamak ve vücudunuzdaki toksinlerden arınmak için harika bir içecek.\n\nMalzemeler:\n- 1 avuç taze ıspanak yaprağı\n- 1/2 yeşil elma\n- 1 adet salatalık\n- 1/2 limonun suyu\n- 1 su bardağı su veya hindistan cevizi suyu\n- 1 ince dilim taze zencefil\n\nHazırlanışı:\nTüm malzemeleri iyice yıkayın. Elma ve salatalığı dilimleyin. Bütün malzemeleri yüksek hızlı bir blender içerisine koyup pürüzsüz bir kıvam alana kadar çekin. İsteğe göre buz ekleyerek soğuk tüketin. Afiyet olsun!",
                        ImageUrl = "/images/recipes/green_smoothie.png",
                        DietitianId = elifDyt.Id,
                        CreatedAt = DateTime.Now
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}

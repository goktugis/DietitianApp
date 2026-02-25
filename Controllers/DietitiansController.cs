using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace DietitianApp.Controllers
{
    [Authorize(Roles = "Dietitian")]
    public class DietitiansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DietitiansController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var profile = await _context.DietitianProfiles.Include(dp => dp.User).FirstOrDefaultAsync(dp => dp.UserId == userId);
            
            if (profile == null)
            {
                // Identity ile oluşturulurken hata olduysa veya profil eksikse telafi et
                profile = new DietitianProfile
                {
                    UserId = userId,
                    IsApproved = false,
                    ConsultationFee = 0,
                    AverageRating = 0
                };
                _context.DietitianProfiles.Add(profile);
                await _context.SaveChangesAsync();
                
                // User objesini de yükle
                profile.User = await _context.Users.FindAsync(userId);
            }

            if (!profile.IsApproved)
            {
                return View("NotApproved", profile);
            }

            // Gelen Randevu Talepleri
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Client)
                .Where(a => a.DietitianId == userId && a.AppointmentDate >= DateTime.UtcNow.Date)
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.UpcomingAppointments = upcomingAppointments;

            // Danışanlarım (En az 1 randevusu olan unqiue kullanıcılar)
            var clients = upcomingAppointments
                .Where(a => a.Status == "Approved" || a.Status == "Completed")
                .Select(a => a.Client)
                .Distinct()
                .ToList();

            ViewBag.Clients = clients;

            return View(profile);
        }

        public async Task<IActionResult> Calendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.DietitianProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
            
            if (profile != null && !profile.IsApproved) return View("NotApproved");

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentsForCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var appointments = await _context.Appointments
                .Include(a => a.Client)
                .Where(a => a.DietitianId == userId && a.Status != "Cancelled")
                .Select(a => new {
                    id = a.Id,
                    title = a.Client != null ? a.Client.Name + " " + a.Client.Surname + (a.Status == "Pending" ? " (Bekliyor)" : "") : "Bilinmeyen Danışan",
                    start = a.AppointmentDate.ToString("yyyy-MM-dd") + "T" + a.StartTime.ToString(@"hh\:mm"),
                    end = a.AppointmentDate.ToString("yyyy-MM-dd") + "T" + a.EndTime.ToString(@"hh\:mm"),
                    color = a.Status == "Pending" ? "#ffc107" : "#198754", // Yellow for pending, Green for approved
                    url = Url.Action("Dashboard", "Dietitians")
                })
                .ToListAsync();

            return Json(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.DietitianProfiles.Include(p => p.User).FirstOrDefaultAsync(dp => dp.UserId == userId);
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(DietitianProfile model, string name, string surname, IFormFile? profileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.DietitianProfiles.Include(p => p.User).FirstOrDefaultAsync(dp => dp.UserId == userId);
            
            if (profile != null)
            {
                profile.Specializations = model.Specializations;
                profile.Biography = model.Biography;
                profile.ConsultationFee = model.ConsultationFee;
                
                if (profile.User != null)
                {
                    profile.User.Name = name;
                    profile.User.Surname = surname;

                    if (profileImage != null && profileImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "profiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "-" + profileImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImage.CopyToAsync(fileStream);
                        }

                        profile.User.ProfileImageUrl = "/images/profiles/" + uniqueFileName;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }
    }
}

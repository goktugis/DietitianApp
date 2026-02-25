using DietitianApp.Data;
using DietitianApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietitianApp.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Book(string dietitianId, DateTime appointmentDate, TimeSpan startTime)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return RedirectToAction("Login", "Account");
            
            // Çakışma Kontrolü (Aynı Diyetisyen'in aynı gün/saatte onaylı/bekleyen randevusu var mı?)
            bool isConflict = await _context.Appointments
                .AnyAsync(a => a.DietitianId == dietitianId 
                            && a.AppointmentDate.Date == appointmentDate.Date 
                            && a.StartTime == startTime
                            && a.Status != "Cancelled");

            if (isConflict)
            {
                TempData["ErrorMessage"] = "Seçtiğiniz saat dilimi doludur. Lütfen farklı bir saat seçiniz.";
                // View'e dönebilmek için dietisyen ID bulmak zorundaydık. Profil listesine atıyoruz.
                return RedirectToAction("DietitianDetails", "Clients", new { id = _context.DietitianProfiles.FirstOrDefault(dp=>dp.UserId == dietitianId)?.Id });
            }

            var endTime = startTime.Add(TimeSpan.FromMinutes(45));
            if (endTime.Days > 0)
            {
                endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
            }

            var appointment = new Appointment
            {
                ClientId = clientId,
                DietitianId = dietitianId,
                AppointmentDate = appointmentDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            
            var dietitianUser = await _context.Users.FindAsync(dietitianId);
            if (dietitianUser != null)
            {
                var notif = new Notification
                {
                    UserId = dietitianId,
                    Title = "Yeni Randevu Talebi",
                    Message = $"{appointmentDate.ToString("dd MMM yyyy")} {startTime.ToString(@"hh\:mm")} için yeni bir randevu talebiniz var.",
                    ActionUrl = "/Dietitians/Dashboard"
                };
                _context.Notifications.Add(notif);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevu talebiniz başarıyla oluşturuldu.";
            return RedirectToAction("Dashboard", "Clients");
        }

        [HttpPost]
        [Authorize(Roles = "Dietitian")]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var dietitianId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           var appointment = await _context.Appointments.Include(a => a.Dietitian).FirstOrDefaultAsync(a => a.Id == appointmentId && a.DietitianId == dietitianId);
            
            if (appointment != null)
            {
                appointment.Status = status; // Approved, Cancelled

                // Danışana Bildirim Gönder
                var notifStatus = status == "Approved" ? "Onaylandı" : "İptal Edildi";
                var notifMessage = status == "Approved" 
                    ? $"{appointment.AppointmentDate.ToString("dd MMM")} tarihindeki randevunuz Dyt. {appointment.Dietitian?.Name} tarafından onaylandı." 
                    : $"{appointment.AppointmentDate.ToString("dd MMM")} tarihindeki randevu talebiniz iptal edildi.";

                var notif = new Notification
                {
                    UserId = appointment.ClientId,
                    Title = $"Randevu {notifStatus}",
                    Message = notifMessage,
                    ActionUrl = "/Clients/Dashboard"
                };
                _context.Notifications.Add(notif);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "Dietitians");
        }
    }
}

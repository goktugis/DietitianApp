using DietitianApp.Models;
using DietitianApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DietitianApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult SelectRole(string actionType)
        {
            ViewBag.ActionType = actionType; // "Login" veya "Register"
            return View();
        }

        [HttpGet]
        public IActionResult Register(string role = "Client")
        {
            var model = new RegisterViewModel { Role = role };
            ViewBag.Role = role;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    Name = model.Name,
                    Surname = model.Surname
                };

                // Eğer Diyetisyen ise profili "Onay bekliyor" olarak oluştur
                if (model.Role == "Dietitian")
                {
                    user.DietitianProfile = new DietitianProfile
                    {
                        IsApproved = false,
                        ConsultationFee = 0,
                        AverageRating = 0
                    };
                }

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Rollerin veritabanında var olduğundan emin ol ve ata
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Başarılı kayıt sonrası direkt login yap (Admin onaylı değilse diyetisyen girişi kısıtlanabilir ama şimdilik içeri alıp sayfasında uyarı gösterelim)
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string role = "Client", string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Role = role;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin")) return RedirectToAction("Index", "Home", new { area = "Admin" });
                        if (roles.Contains("Dietitian")) return RedirectToAction("Dashboard", "Dietitians");
                        return RedirectToAction("Dashboard", "Clients"); // Danışan anasayfası
                    }
                    return LocalRedirect(returnUrl ?? "/");
                }
                
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}

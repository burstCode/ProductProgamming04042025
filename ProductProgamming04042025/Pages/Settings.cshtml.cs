using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;

namespace ProductProgamming04042025.Pages
{
    [Authorize]
    public class Settings : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserProfile UserProfile { get; set; }

        [BindProperty]
        public SettingsInputModel Input { get; set; }

        public Settings(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;

            Input = new();
        }

        public async Task OnGetAsync()
        {
            // Устанавливаем в поля текущие значения
            var user = await _userManager.GetUserAsync(User);

            UserProfile = await _context.UserProfiles.
                    FirstOrDefaultAsync(up => up.UserId == user.Id);

            Input.Weight = UserProfile.Weight;
            Input.Height = UserProfile.Height;
            Input.Age = UserProfile.Age;
        }

        // Сохранение новых значений веса, роста и возраста
        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            UserProfile = await _context.UserProfiles.
                    FirstOrDefaultAsync(up => up.UserId == user.Id);

            UserProfile.Weight = Input.Weight;
            UserProfile.Height = Input.Height;
            UserProfile.Age = Input.Age;

            _context.UserProfiles.Update(UserProfile);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }

    public class SettingsInputModel
    {
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public int Age { get; set; }
    }
}
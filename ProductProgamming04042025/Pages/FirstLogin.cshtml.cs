using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Pages.Models;
using ProductProgamming04042025.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ProductProgamming04042025.Pages
{
    [Authorize]
    public class FirstLoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public string FitnessGoal { get; set; }

        [BindProperty]
        public NewFields NewFields { get; set; }

        public FirstLoginModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            // Загружаем существующий профиль
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile != null)
            {
                ViewData["uesr_name"] = profile.FirstName + " " + profile.LastName;
                ViewData["is_male"] = profile.Sex;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            // Загружаем существующий профиль
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (NewFields.Age <= 0)
            {
                ModelState.AddModelError("UserProfile.Age", "Возраст должен быть положительным числом");
            }

            if (NewFields.Height <= 0)
            {
                ModelState.AddModelError("UserProfile.Height", "Рост должен быть положительным числом");
            }


            if (NewFields.Weight <= 0)
            {
                ModelState.AddModelError("UserProfile.Weight", "Вес должен быть положительным числом");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Обновляем только изменяемые поля
            profile.Age = NewFields.Age;
            profile.Height = NewFields.Height;
            profile.Weight = NewFields.Weight;
            profile.Sex = NewFields.Sex;
            profile.IsConfiguredFirstTime = true;

            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard");
        }
    }

    public class NewFields
    {
        public int Age { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public bool Sex { get; set; }
    }
}

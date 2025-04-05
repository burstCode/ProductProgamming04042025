using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace ProductProgamming04042025.Pages
{
    public class PlanModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserProfile UserProfile { get; set; }
        public FitnessPlan Plan { get; set; }

        public PlanModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Получаем последний примененный план
            var lastAppliedPlan = await _context.ChatRecords
                .Where(c => c.UserId == user.Id && c.IsApplied)
                .OrderByDescending(c => c.AppliedDate)
                .FirstOrDefaultAsync();

            if (lastAppliedPlan != null)
            {
                Plan = JsonConvert.DeserializeObject<FitnessPlan>(lastAppliedPlan.ModelResponse);
            }
        }
    }
}

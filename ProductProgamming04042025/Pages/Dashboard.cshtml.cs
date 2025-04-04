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
    public class DashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserProfile UserProfile { get; set; }


        [BindProperty(SupportsGet = true)]
        public string FitnessGoal { get; set; }

        public DashboardModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            UserProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if ( !UserProfile.IsConfiguredFirstTime )
            {
                return RedirectToPage("/FirstLogin");
            }

            return Page();
        }
    }
}

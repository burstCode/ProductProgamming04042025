using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;

namespace ProductProgamming04042025.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserProfile UserProfile { get; set; }

        public FitnessPlan Plan { get; set; }

        // ����� �������� ���� ��������� ������ ������ � ��� � ��������
        [BindProperty(SupportsGet = true)]
        public string FitnessGoal { get; set; }

        public IndexModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);
            }
            catch (NullReferenceException ex)
            {
                // � ������ �����������
                return RedirectToPage("/Login");
            }

            // ���� ������ ����� � ������� - ������������ �� �������������� ���������
            if (!UserProfile.IsConfiguredFirstTime)
            {
                return RedirectToPage("/FirstLogin");
            }

            // �������� ��������� ����������� ����
            var lastAppliedPlan = await _context.ChatRecords
                .Where(c => c.UserId == user.Id && c.IsApplied)
                .OrderByDescending(c => c.AppliedDate)
                .FirstOrDefaultAsync();

            if (lastAppliedPlan == null)
            {
                return RedirectToPage("/Chat");
            }

            Plan = JsonConvert.DeserializeObject<FitnessPlan>(lastAppliedPlan.ModelResponseJson);

            return Page();
        }
    }
}

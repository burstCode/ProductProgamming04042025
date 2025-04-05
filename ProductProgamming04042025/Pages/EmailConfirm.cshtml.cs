using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProductProgamming04042025.Pages
{
    public class EmailConfirmModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        public UserManager<IdentityUser> _userManager;

        public EmailConfirmModel(UserManager<IdentityUser> userManager){
            this._userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            
            if(_userManager != null){
                var user = await _userManager.GetUserAsync(User);

                if(user != null && user.EmailConfirmed){
                    return RedirectToPage("/Index");
                }
            }
            return Page();
        }
    }
}

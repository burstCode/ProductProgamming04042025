using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

using ProductProgamming04042025.Pages.Models;
using ProductProgamming04042025.Pages.Helpers;
using ProductProgamming04042025.Data;

namespace ProductProgamming04042025.Pages
{
    public class Registration : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        private readonly ApplicationDbContext _context;

        [BindProperty]
        public RegistrationModel Input { get; set; }

        public Registration(
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // �������� ������������ ������
            var userByLogin = await _userManager.FindByNameAsync(Input.Login);
            if (userByLogin != null)
            {
                ModelState.AddModelError(string.Empty, "������������ � ����� ������� ��� ����������.");

                return Page();
            }

            // �������� ������������ �����
            var userByEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (userByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "������������ � ����� ������� ����������� ����� ��� ����������.");

                return Page();
            }

            var user = new IdentityUser
            {
                UserName = Input.Login,
                Email = Input.Email
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // ������� ������� ������������
                var userProfile = new UserProfile
                {
                    UserId = user.Id,
                    User = user,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Age = 0,
                    Sex = false,
                    Height = 0,
                    Weight = 0,
                    FitnessGoal = string.Empty
                };

                // ��������� ������� � ��
                await _context.UserProfiles.AddAsync(userProfile);
                await _context.SaveChangesAsync();

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Login",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailService.SendEmailAsync(
                    Input.Email,
                    "����������� ����� ����� ����������� �����",
                    $"����������, ����������� ��� �������, ������� �� <a href=" +
                    $"'{HtmlEncoder.Default.Encode(callbackUrl)}'> ���� ������</a>. " +
                    $"������ ����� ������������� 1 ��� � ������� ��������.");

                return RedirectToPage("EmailConfirm", new { email = Input.Email });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}

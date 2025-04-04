using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

using ProductProgamming04042025.Pages.Models;
using ProductProgamming04042025.Pages.Helpers;

namespace ProductProgamming04042025.Pages
{
    public class Registration : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        [BindProperty]
        public RegistrationModel Input { get; set; }

        public Registration(
            UserManager<IdentityUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
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
                switch (error.Code)
                {
                    case nameof(IdentityErrorDescriber.DuplicateEmail):
                    case nameof(IdentityErrorDescriber.DuplicateUserName):
                        ModelState.AddModelError("Input.Email", error.Description);
                        break;
                    case nameof(IdentityErrorDescriber.PasswordTooShort):
                    case nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric):
                    case nameof(IdentityErrorDescriber.PasswordRequiresDigit):
                    case nameof(IdentityErrorDescriber.PasswordRequiresLower):
                    case nameof(IdentityErrorDescriber.PasswordRequiresUpper):
                        ModelState.AddModelError("Input.Password", error.Description);
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, error.Description);
                        break;
                }
            }

            return Page();
        }
    }
}

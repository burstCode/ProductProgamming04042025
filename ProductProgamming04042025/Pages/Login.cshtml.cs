using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

using ProductProgamming04042025.Pages.Models;

namespace ProductProgamming04042025.Pages
{
    public class Login : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        [BindProperty]
        public LoginModel Input {  get; set; }

        public Login(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Активация аккаунта
            if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Code))
            {
                var user = await _userManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    return NotFound("Пользователь не найден.");
                }

                // Декодируем токен
                var decodedCode = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(Code));

                // Подтверждаем email
                var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("/FirstLogin");
                }
                // TODO: Можнон добавить переход на страницу, где сказано, что ссылка просрочена
                // еще и аккаунт снести наху из БД
          
            }
            Response.Headers.Append(
                "Content-Security-Policy",
                "frame-ancestors http://localhost https://oauth.yandex.ru https://yandex.ru 'self'"
            );
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("UserId");
            ModelState.Remove("Code");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Login,
                    Input.Password,
                    false,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToPage("/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль");

                    return Page();
                }
            }

            return Page();
        }
    }
}

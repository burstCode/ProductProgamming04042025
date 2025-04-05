using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

using ProductProgamming04042025.Pages.Models;
using ProductProgamming04042025.Pages.Helpers;
using ProductProgamming04042025.Data;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ProductProgamming04042025.Pages
{
    public class YandexID : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly ApplicationDbContext _context;

        [BindProperty]
        public RegistrationModel Input { get; set; }

        public YandexID(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public void OnGet(){
            Response.Headers.Append(
                "Content-Security-Policy",
                "frame-ancestors https://localhost https://oauth.yandex.ru https://yandex.ru https://mc.yandex.ru 'self'"
            );
        }

        public async Task<IActionResult> OnGetToken(string t)
        {

            if (!string.IsNullOrEmpty(t)) {

                // ��������� ������
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("https://login.yandex.ru/info?format=json"),
                    Method = HttpMethod.Get,
                };

                // ������ �� ������
                request.Headers.Add("Authorization", "OAuth " + t);
                using var response = await httpClient.SendAsync(request);
                // ������ �����
                var jsonObject = await response.Content.ReadFromJsonAsync<YandexUserInfo>();

                // ���� �������
                if (jsonObject != null)
                {
                    // ���� ������������
                    var user = await _userManager.FindByLoginAsync("Yandex", jsonObject.Psuid);
                    if (user != null)
                    {
                        // ���� ���� YandexID
                        var login_result = await _signInManager.ExternalLoginSignInAsync("Yandex", jsonObject.Psuid, isPersistent: false);
                        if (login_result.Succeeded)
                        {
                            return RedirectToPage("Index");
                        }
                    }
                    else
                    {
                        // ���� ���� ��� YandexID
                        // ������ ������ ������������
                        var new_user = new IdentityUser()
                        {
                            UserName = jsonObject.Login,
                            Email = jsonObject.DefaultEmail,
                            EmailConfirmed = true
                        };

                        // ��������� ������ ������������
                        var result = await _userManager.CreateAsync(new_user);
                        if (result.Errors.Count() == 0)
                        {

                            // ��������� ���� ����������� ������������ ����� YANDEX ID � ��
                            var loginInfo = new UserLoginInfo("Yandex", jsonObject.Psuid, "Yandex ID");
                            await _userManager.AddLoginAsync(new_user, loginInfo);

                            var userProfile = new UserProfile()
                            {
                                UserId = new_user.Id,
                                FirstName = jsonObject.FirstName,
                                LastName = jsonObject.LastName,
                                User = new_user,
                                Age = 0,
                                Sex = jsonObject.Sex == "male" ? true : false,
                                Height = 0,
                                Weight = 0,
                                IsConfiguredFirstTime = false
                            };
                            await _context.UserProfiles.AddAsync(userProfile);
                            await _context.SaveChangesAsync();


                            // ��������� ��� �������������
                            var login_result = await _signInManager.ExternalLoginSignInAsync("Yandex", jsonObject.Psuid, isPersistent: false);
                            if (login_result.Succeeded)
                            {
                                return RedirectToPage("Index");
                            }
                        }

                    }
                    
                }
            }
            return Page();
        }


        public class YandexUserInfo
        {
            // �������� ����
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("login")]
            public string Login { get; set; }

            [JsonPropertyName("client_id")]
            public string ClientId { get; set; }

            [JsonPropertyName("display_name")]
            public string DisplayName { get; set; }

            // ��� � ������� (Unicode-��������������)
            [JsonPropertyName("real_name")]
            public string RealName { get; set; }

            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string LastName { get; set; }

            // ��� ("male", "female" ��� null)
            [JsonPropertyName("sex")]
            public string Sex { get; set; }

            // �������� email
            [JsonPropertyName("default_email")]
            public string DefaultEmail { get; set; }

            // ������ ���� email (����� ���� ������)
            [JsonPropertyName("emails")]
            public List<string> Emails { get; set; }

            // ���������� ������������� PSUID (Passport Social UID)
            [JsonPropertyName("psuid")]
            public string Psuid { get; set; }
        }

    }
}
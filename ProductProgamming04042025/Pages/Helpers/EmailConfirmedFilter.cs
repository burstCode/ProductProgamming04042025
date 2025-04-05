using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductProgamming04042025.Pages.Helpers
{
    public class EmailConfirmedFilter : IAsyncAuthorizationFilter
    {
        // ѕуть, дл€ которого не об€зательно, чтобы пользователь был авторизован
        private readonly List<string> _unProtectedPaths = new(){
            "/Login",
            "/Registration",
            "/Confidence",
            "/EmailConfirm",
        };

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            Console.WriteLine("ROUTE DATA");
            Console.WriteLine(context.RouteData.Values);
            
            // ≈сли путь защищЄн, а пользователь не подтвердил почту
            if(!_unProtectedPaths.Contains(context.HttpContext.Request.Path.Value, StringComparer.OrdinalIgnoreCase)){
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<IdentityUser>>();

                var user = await userManager.GetUserAsync(context.HttpContext.User);

                if (user == null)
                {
                    context.Result = new RedirectToPageResult("/Login");
                }else if(!user.EmailConfirmed){
                    context.Result = new RedirectToPageResult("/EmailConfirm");
                }
            }

        }
    }
}
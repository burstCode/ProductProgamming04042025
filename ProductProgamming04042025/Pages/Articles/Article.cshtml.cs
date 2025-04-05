
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;

namespace ProductProgamming04042025.Pages{
    [Authorize]
    public class ArticleModel : PageModel{

        ApplicationDbContext dbContext;
        public ArticleModel(ApplicationDbContext _dbContext){
            this.dbContext = _dbContext;
        }

        public async Task<IActionResult> OnGetAsync(int n){
            Article article = this.dbContext.Articles.Find(n);
            if(article != null){
                ViewData["Title"] = article.Title;
                ViewData["Content"] = article.Content;
                return Page();
            }

            return RedirectToPage("/404");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Data;

namespace ProductProgamming04042025.Pages{
    [Authorize]
    public class FoodModel : PageModel{

        ApplicationDbContext dbContext;
        public FoodModel(ApplicationDbContext _dbContext){
            this.dbContext = _dbContext;
        }

        public async Task<IActionResult> OnGetAsync(int n){
            ViewData["Title"] = "упчвебн мнлеп " + n;
            return Page();
        }
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductProgamming04042025.Pages
{
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserProfile UserProfile { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();

        [BindProperty]
        public ChatInputModel Input { get; set; }

        public ChatModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Загрузка истории сообщений
            Messages = await _context.ChatRecords
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new ChatMessage
                {
                    Text = c.IsAnswerReady ? c.ModelResponse : c.UserRequest,
                    IsBot = c.IsAnswerReady,
                    Plan = c.IsAnswerReady ? new PlanPreview { Id = c.Id } : null
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            // Сохраняем запрос пользователя
            var record = new ChatRecord
            {
                UserId = user.Id,
                UserRequest = Input.Message,
                IsAnswerReady = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatRecords.Add(record);
            await _context.SaveChangesAsync();

            // Здесь будет логика отправки запроса к ИИ
            // и обработка ответа (можно вынести в фоновую службу)

            return RedirectToPage();
        }
    }

    public class ChatInputModel
    {
        [Required]
        public string Message { get; set; }
    }

    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsBot { get; set; }
        public PlanPreview Plan { get; set; }
    }

    public class PlanPreview
    {
        public int Id { get; set; }
    }
}

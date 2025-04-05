using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductProgamming04042025.Data;
using ProductProgamming04042025.Pages.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductProgamming04042025.AI;
using ProductProgamming04042025.Pages.Helpers;
using Newtonsoft.Json;

namespace ProductProgamming04042025.Pages
{
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IBackgroundTaskQueue _taskQueue;

        public UserProfile UserProfile { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();

        [BindProperty]
        public ChatInputModel Input { get; set; }

        public ChatModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IBackgroundTaskQueue taskQueue)
        {
            _context = context;
            _userManager = userManager;
            _taskQueue = taskQueue;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Загрузка истории сообщений
            List<ChatRecord> records = await _context.ChatRecords
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => c)
                .ToListAsync();

            foreach (var record in records)
            {
                Messages.Add(new ChatMessage
                {
                    Text = record.UserRequest,
                    IsBot = false,
                    Plan = new PlanPreview
                    {
                        Id = record.Id
                    }
                });
                Messages.Add(new ChatMessage
                {
                    Text = record.ModelResponseText,
                    IsBot = true,
                    Plan = new PlanPreview
                    {
                        Id = record.Id,
                        IsApplied = record.IsApplied
                    }
                });
            }
        }

        public async Task<IActionResult> OnPostSendMessageAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Сохраняем запрос пользователя
            var record = new ChatRecord
            {
                UserId = user.Id,
                UserRequest = Input.Message,
                IsAnswerReady = false,
                ModelResponseText = string.Empty,
                ModelResponseJson = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatRecords.Add(record);
            await _context.SaveChangesAsync();

            // Добавляем задачу в фоновую очередь
            _taskQueue.Enqueue(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bot = scope.ServiceProvider.GetRequiredService<Bot>();

                try
                {
                    // Получаем свежую запись из БД
                    var freshRecord = await dbContext.ChatRecords.FindAsync(record.Id);
                    if (freshRecord == null) return;

                    // Получаем ответ от ИИ
                    var botAnswer = await bot.SendRequest(freshRecord.UserRequest, UserProfile);

                    string textResponse = (string)botAnswer[0];
                    var plan = botAnswer[1];

                    // Обновляем запись
                    freshRecord.ModelResponseText = textResponse;
                    freshRecord.ModelResponseJson = JsonConvert.SerializeObject(plan);
                    freshRecord.IsAnswerReady = true;
                    freshRecord.AppliedDate = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChatModel>>();
                    logger.LogError(ex, "Ошибка при обработке запроса ИИ");
                }
            });

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApplyPlanAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Деактивация старого плана
            var previousAppliedPlans = await _context.ChatRecords
                .Where(p => p.UserId == user.Id && p.IsApplied)
                .ToListAsync();

            foreach (var plan in previousAppliedPlans)
            {
                plan.IsApplied = false;
            }

            // Установка нового плана
            var newPlan = await _context.ChatRecords
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            newPlan.IsApplied = true;
            newPlan.AppliedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Plan");
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
        public bool IsApplied { get; set; } = false;
    }
}

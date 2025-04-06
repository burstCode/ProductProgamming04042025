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

        [BindProperty(SupportsGet = true)]
        public string FitnessGoal { get; set; }

        public ChatModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IBackgroundTaskQueue taskQueue)
        {
            _context = context;
            _userManager = userManager;
            _taskQueue = taskQueue;
        }

        public async Task<IActionResult> OnGetChatRecordWidthTimeAsync(DateTime time)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                var e = new
                {
                    Error = "Uncnown user"
                };
                return new JsonResult(e);
            }
            // �������� ������� ���������
            var records = await _context.ChatRecords
                .Where(c => c.UserId == user.Id && c.CreatedAt < time) // ������ �� ������������ � ����
                .OrderByDescending(c => c.CreatedAt) // ��������� �� ����� � ������
                .Take(1) // ����� 10 ���������
                .OrderBy(c => c.CreatedAt) // ���. ���������� (���� ����� �������� �������)
                .ToListAsync();


            return new JsonResult(records);
        }
        public async Task<IActionResult> OnGetIsReadyAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var record = await _context.ChatRecords.FindAsync(id);


            if (user == null || record == null || record.UserId != user.Id)
            {
                var er = new
                {
                    IsReady = false,
                    Error = "1"
                };
                return new JsonResult(er);
            }
            if (record.CreatedAt.Subtract(DateTime.Now) > new TimeSpan(0, 1, 0))
            {
                Console.WriteLine("record");
                var freshRecord = await _context.ChatRecords.FindAsync(record.Id);

                if (freshRecord == null)
                {
                    var err = new
                    {
                        Error = "Unknown record"
                    };
                    return new JsonResult(err);
                }


                freshRecord.IsAnswerReady = true;
                freshRecord.ModelResponseText = "� ���������, �� ������� ������������� ����, ��������� ������";
                await _context.SaveChangesAsync();
            }
            var e = new
            {
                IsReady = record.IsAnswerReady,
                Response = record.ModelResponseText
            };
            return new JsonResult(e);
        }
        public async Task<IActionResult> OnGetChatRecordAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                var e = new
                {
                    Error = "Unknown user"
                };
                return new JsonResult(e);
            }
            // �������� ������� ���������
            var records = await _context.ChatRecords
                .Where(c => c.UserId == user.Id) // ������ �� ������������
                .OrderByDescending(c => c.CreatedAt) // ��������� �� ����� � ������
                .Take(1) // ����� 10 ���������
                .OrderBy(c => c.CreatedAt) // ���. ���������� (���� ����� �������� �������)
                .ToListAsync();

            

            return new JsonResult(records);
        }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (FitnessGoal != null)
            {
                // ���� ������� � ������-�����, ������������� ���������� ������
                Input = new ChatInputModel { Message = FitnessGoal };
                await OnPostSendMessageAsync();
                Response.Redirect("/Chat");
                return;
            }

            // �������� ������� ���������
            var records = await _context.ChatRecords
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.CreatedAt)
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
                    },
                    IsAnswerReady = record.IsAnswerReady
                });
                Messages.Add(new ChatMessage
                {
                    Text = record.ModelResponseText,
                    IsBot = true,
                    Plan = new PlanPreview
                    {
                        Id = record.Id,
                        IsApplied = record.IsApplied
                    },
                    IsAnswerReady = record.IsAnswerReady
                });
            }
        }

        public async Task<IActionResult> OnPostSendMessageAAsync(
    [FromForm] string message)
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            var latestRecord = await _context.ChatRecords
                .Where(cr => cr.UserId == user.Id)  // ��������� �� UserId
                .OrderByDescending(cr => cr.CreatedAt)  // ��������� �� ���� �������� (����� �������)
                .FirstOrDefaultAsync();  // ����� ������ (����� �����)

            if (latestRecord == null || !latestRecord.IsAnswerReady)
            {
                return new JsonResult(new
                {
                    Error = true
                });
            }

            // ��������� ������ ������������
            var record = new ChatRecord
            {
                UserId = user.Id,
                UserRequest = message,
                IsAnswerReady = false,
                ModelResponseText = string.Empty,
                ModelResponseJson = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatRecords.Add(record);
            await _context.SaveChangesAsync();

            // ��������� ������ � ������� �������
            _taskQueue.Enqueue(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bot = scope.ServiceProvider.GetRequiredService<Bot>();

                try
                {
                    // �������� ������ ������ �� ��
                    var freshRecord = await dbContext.ChatRecords.FindAsync(record.Id);
                    if (freshRecord == null) return;

                    // �������� ����� �� ��
                    var botAnswer = await bot.SendRequest(freshRecord.UserRequest, UserProfile);

                    string textResponse = (string)botAnswer[0];
                    var plan = botAnswer[1];

                    // ��������� ������
                    freshRecord.ModelResponseText = textResponse;
                    freshRecord.ModelResponseJson = JsonConvert.SerializeObject(plan);
                    freshRecord.IsAnswerReady = true;
                    freshRecord.AppliedDate = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // ����������� ������
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChatModel>>();
                    logger.LogError(ex, "������ ��� ��������� ������� ��");
                    // �������� ������ ������ �� ��
                    var freshRecord = await dbContext.ChatRecords.FindAsync(record.Id);
                    if (freshRecord == null) return;


                    freshRecord.IsAnswerReady = true;
                    freshRecord.ModelResponseText = "� ���������, �� ����� ��������� �������� ������";
                    await dbContext.SaveChangesAsync();

                }
            });

            return new JsonResult(record);
        }
        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserProfile = await _context.UserProfiles.
                FirstOrDefaultAsync(p => p.UserId == user.Id);

            // ��������� ������ ������������
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

            // ��������� ������ � ������� �������
            _taskQueue.Enqueue(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bot = scope.ServiceProvider.GetRequiredService<Bot>();

                try
                {
                    // �������� ������ ������ �� ��
                    var freshRecord = await dbContext.ChatRecords.FindAsync(record.Id);
                    if (freshRecord == null) return;

                    // �������� ����� �� ��
                    var botAnswer = await bot.SendRequest(freshRecord.UserRequest, UserProfile);

                    string textResponse = (string)botAnswer[0];
                    var plan = botAnswer[1];

                    // ��������� ������
                    freshRecord.ModelResponseText = textResponse;
                    freshRecord.ModelResponseJson = JsonConvert.SerializeObject(plan);
                    freshRecord.IsAnswerReady = true;
                    freshRecord.AppliedDate = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // ����������� ������
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChatModel>>();
                    logger.LogError(ex, "������ ��� ��������� ������� ��");
                }
            });

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetApplyPlanAAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // ����������� ������� �����
            var previousAppliedPlans = await _context.ChatRecords
                .Where(p => p.UserId == user.Id && p.IsApplied)
                .ToListAsync();

            foreach (var plan in previousAppliedPlans)
            {
                plan.IsApplied = false;
            }

            // ��������� ������ �����
            var newPlan = await _context.ChatRecords
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            newPlan.IsApplied = true;
            newPlan.AppliedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                Ok = 1
            });
        }

        public async Task<IActionResult> OnPostApplyPlanAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // ����������� ������� �����
            var previousAppliedPlans = await _context.ChatRecords
                .Where(p => p.UserId == user.Id && p.IsApplied)
                .ToListAsync();

            foreach (var plan in previousAppliedPlans)
            {
                plan.IsApplied = false;
            }

            // ��������� ������ �����
            var newPlan = await _context.ChatRecords
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            newPlan.IsApplied = true;
            newPlan.AppliedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
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
        public bool IsAnswerReady { get; set; }
        public PlanPreview Plan { get; set; }
    }

    public class PlanPreview
    {
        public int Id { get; set; }
        public bool IsApplied { get; set; } = false;
    }
}

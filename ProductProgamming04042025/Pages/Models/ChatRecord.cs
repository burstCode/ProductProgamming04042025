using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProductProgamming04042025.Pages.Models
{

    public class ChatRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Required]
        public string UserRequest { get; set; }

        public bool IsAnswerReady { get; set; } = false;
        public string ModelResponseText { get; set; } = string.Empty; // Строка с планом в виде текта
        public string ModelResponseJson { get; set; } = string.Empty; // JSON строка с планом
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Для отслеживания применения плана
        public bool IsApplied { get; set; } = false;
        public DateTime? AppliedDate { get; set; }
    }
}

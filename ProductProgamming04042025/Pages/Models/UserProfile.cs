using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ProductProgamming04042025.Pages.Models
{
    public class UserProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }  // Связь с IdentityUser
        public IdentityUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public int Age { get; set; }

        public bool Sex { get; set; }

        public decimal Height { get; set; }  // Рост в см

        public decimal Weight { get; set; }  // Вес в кг

        public bool IsConfiguredFirstTime { get; set; }     // Была ли настройка при первом логине
    }
}

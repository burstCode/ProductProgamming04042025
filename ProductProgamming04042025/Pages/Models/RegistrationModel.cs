using System.ComponentModel.DataAnnotations;

namespace ProductProgamming04042025.Pages.Models
{
    public class RegistrationModel
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool PrivacyAgreement { get; set; }
    }
}

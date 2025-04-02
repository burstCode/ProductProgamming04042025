namespace ProductProgamming04042025.Pages.Helpers
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string body);
    }
}

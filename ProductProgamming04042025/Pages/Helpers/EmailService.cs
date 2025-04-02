using MimeKit;
using MailKit.Net.Smtp;

namespace ProductProgamming04042025.Pages.Helpers
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(
                new MailboxAddress(
                    "Администрация сайта",
                    "best.product.mail@yandex.ru")
                );
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 465, true);
                await client.AuthenticateAsync("best.product.mail@yandex.ru", "jjdaphzxntijlocs");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}

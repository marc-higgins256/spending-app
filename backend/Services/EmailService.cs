using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace SpendingApp.Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendConfirmationEmail(string email, string token)
        {
            var smtpHost = _config["Mailtrap:Host"];
            var portString = _config["Mailtrap:Port"];
            int smtpPort = 2525;
            if (!int.TryParse(portString, out smtpPort))
                smtpPort = 2525;
            var smtpUser = _config["Mailtrap:User"];
            var smtpPass = _config["Mailtrap:Pass"];
            var from = _config["Mailtrap:From"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Spending App", from));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Confirm your email";
            var confirmationLink = $"http://localhost:5173/confirm-email?token={token}";
            message.Body = new TextPart("plain")
            {
                Text = $"Click the link to confirm your email: {confirmationLink}"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

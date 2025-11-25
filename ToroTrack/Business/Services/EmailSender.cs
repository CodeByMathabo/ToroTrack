using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ToroTrack.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailMessage = new MimeMessage();
                // Sender Name & Address from Config
                emailMessage.From.Add(new MailboxAddress("Toro Track Auth", _config["EmailSettings:SenderEmail"]));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Connect to SMTP Server (e.g., smtp.gmail.com, port 587)
                await client.ConnectAsync(_config["EmailSettings:Server"], int.Parse(_config["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);

                // Authenticate
                await client.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw; // Rethrow to let the UI know it failed
            }
        }
    }
}
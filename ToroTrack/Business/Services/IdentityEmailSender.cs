using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ToroTrack.Data;

namespace ToroTrack.Services
{
    // Why: This class fixes the "InvalidOperationException" by implementing the generic interface 
    // requested by the ForgotPassword page (IEmailSender<ApplicationUser>).
    // It acts as a bridge to your actual EmailSender logic.
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSender _emailSender;

        public IdentityEmailSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            _emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            _emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            _emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
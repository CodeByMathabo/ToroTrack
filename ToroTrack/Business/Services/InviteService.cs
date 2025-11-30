using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using ToroTrack.Business.Helpers; // Added for PasswordHelper
using ToroTrack.Data;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;

namespace ToroTrack.Business.Services
{
    public interface IInviteService
    {
        Task<List<PendingInvite>> GetPendingInvitesAsync();
        Task SendInviteAsync(PendingInvite inviteModel);
        Task RevokeInviteAsync(int inviteId);
        Task CompleteInviteAsync(string email);
    }

    public class InviteService : IInviteService
    {
        private readonly IInviteRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<InviteService> _logger;

        public InviteService(
            IInviteRepository repository,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<InviteService> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<List<PendingInvite>> GetPendingInvitesAsync()
        {
            return await _repository.GetAllInvitesAsync();
        }

        public async Task SendInviteAsync(PendingInvite inviteModel)
        {
            _logger.LogInformation("Starting invite process for {Email}", inviteModel.Email);

            // Validation: Ensure user doesn't already exist in Identity
            var existingUser = await _userManager.FindByEmailAsync(inviteModel.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {inviteModel.Email} already exists.");
            }

            // Validation: Ensure invite isn't already pending (Business Rule)
            var existingInvite = await _repository.GetInviteByEmailAsync(inviteModel.Email);
            if (existingInvite != null)
            {
                throw new InvalidOperationException("An invitation is already pending for this email.");
            }

            try
            {
                // Why: Reuse the centralized password generation logic
                string temporaryPassword = PasswordHelper.GenerateRandomPassword();

                // Create Identity User
                var newUser = new ApplicationUser
                {
                    UserName = inviteModel.Email,
                    Email = inviteModel.Email,
                    EmailConfirmed = true, // Admin-verified, so we auto-confirm
                    FirstName = ParseFirstName(inviteModel.FullName),
                    LastName = ParseLastName(inviteModel.FullName),
                    CompanyName = inviteModel.CompanyName,
                    JobRole = inviteModel.ClientJobRole
                };

                var createResult = await _userManager.CreateAsync(newUser, temporaryPassword);
                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create user: {errors}");
                }

                // Assign Role
                var roleResult = await _userManager.AddToRoleAsync(newUser, inviteModel.Role);
                if (!roleResult.Succeeded)
                {
                    // Cleanup: Delete user if role assignment fails to prevent "ghost" users
                    await _userManager.DeleteAsync(newUser);
                    throw new Exception("Failed to assign role to user.");
                }

                // Persist Invite Record (For Admin Dashboard tracking)
                inviteModel.DateInvited = DateTime.UtcNow;
                inviteModel.InviteToken = Guid.NewGuid().ToString();
                await _repository.AddInviteAsync(inviteModel);

                // Send Email
                await SendWelcomeEmail(inviteModel.Email, inviteModel.Role, temporaryPassword);

                _logger.LogInformation("Invite sent successfully to {Email}", inviteModel.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invite user {Email}", inviteModel.Email);
                throw; // Propagate to UI
            }
        }

        public async Task RevokeInviteAsync(int inviteId)
        {
            _logger.LogInformation("Revoking invite ID: {Id}", inviteId);
            await _repository.DeleteInviteAsync(inviteId);
        }

        public async Task CompleteInviteAsync(string email)
        {
            // Logic: Find if there is a pending invite for this email
            var invite = await _repository.GetInviteByEmailAsync(email);

            if (invite != null)
            {
                _logger.LogInformation("User {Email} has logged in. Completing invitation.", email);
                // Logic: We delete the "Pending" record because they are now an "Active" user.
                await _repository.DeleteInviteAsync(invite.Id);
            }
        }

        // --- Helper Methods ---
        private async Task SendWelcomeEmail(string email, string role, string password)
        {
            var subject = "Welcome to Toro Track";
            var htmlMessage = $@"
                <div style='font-family: sans-serif; padding: 20px;'>
                    <h2>Welcome to Toro Track!</h2>
                    <p>You have been invited to join the system as a <strong>{role}</strong>.</p>
                    <p>Your account has been created. Use the following login credentials to access the system:</p>
                    <div style='background-color: #f4f4f4; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Username:</strong> {email}</p>
                        <p><strong>Password:</strong> {password}</p>
                    </div>
                </div>";

            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }

        private string ParseFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "User";
            return fullName.Split(' ')[0];
        }

        private string ParseLastName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName) || !fullName.Contains(' ')) return "";
            var parts = fullName.Split(' ');
            return parts.Length > 1 ? parts[1] : "";
        }
    }
}
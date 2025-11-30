using Microsoft.AspNetCore.Identity;
using ToroTrack.Data;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface IClientSettingsService
    {
        Task<ClientSettingsViewModel> GetSettingsForUserAsync(string userId);
        Task UpdateSettingsAsync(string userId, ClientSettingsViewModel model);
    }

    public class ClientSettingsService : IClientSettingsService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientSettingsRepository _repository;

        public ClientSettingsService(UserManager<ApplicationUser> userManager, IClientSettingsRepository repository)
        {
            _userManager = userManager;
            _repository = repository;
        }

        // Combines User data and Preference data into one View Model
        public async Task<ClientSettingsViewModel> GetSettingsForUserAsync(string userId)
        {
            var vm = new ClientSettingsViewModel();

            try
            {
                // Get User Details
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    vm.Account.UserId = user.Id;
                    vm.Account.Email = user.Email ?? "";
                    vm.Account.CompanyName = user.CompanyName ?? "";
                    vm.Account.ContactNumber = user.PhoneNumber ?? "";
                }

                // Get Preferences
                var existingPrefs = await _repository.GetPreferencesByUserIdAsync(userId);
                if (existingPrefs != null)
                {
                    vm.Preferences.NotifyOnMilestone = existingPrefs.NotifyOnMilestone;
                    vm.Preferences.NotifyOnQueryReply = existingPrefs.NotifyOnQueryReply;
                    vm.Preferences.NotifyOnMeeting = existingPrefs.NotifyOnMeeting;
                }
                else
                {
                    // Defaults if no record exists yet
                    vm.Preferences.NotifyOnMilestone = true;
                    vm.Preferences.NotifyOnQueryReply = true;
                    vm.Preferences.NotifyOnMeeting = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service Error getting settings: {ex.Message}");
            }

            return vm;
        }

        // Saves both sections
        public async Task UpdateSettingsAsync(string userId, ClientSettingsViewModel model)
        {
            try
            {
                // Update Core User Account
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.CompanyName = model.Account.CompanyName;
                    user.PhoneNumber = model.Account.ContactNumber;

                    if (user.Email != model.Account.Email)
                    {
                        await _userManager.SetEmailAsync(user, model.Account.Email);
                        user.UserName = model.Account.Email; // Keep username synced if email is username
                    }

                    await _userManager.UpdateAsync(user);
                }

                // Update Preferences
                var existingPrefs = await _repository.GetPreferencesByUserIdAsync(userId);
                if (existingPrefs == null)
                {
                    existingPrefs = new ClientPreference { UserId = userId };
                }

                existingPrefs.NotifyOnMilestone = model.Preferences.NotifyOnMilestone;
                existingPrefs.NotifyOnQueryReply = model.Preferences.NotifyOnQueryReply;
                existingPrefs.NotifyOnMeeting = model.Preferences.NotifyOnMeeting;

                await _repository.SavePreferencesAsync(existingPrefs);

                Console.WriteLine($"Settings updated for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service Error saving settings: {ex.Message}");
                throw;
            }
        }
    }
}
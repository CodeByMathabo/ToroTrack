using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface ISystemSettingsService
    {
        Task<SystemSettingsViewModel> GetSettingsAsync();
        Task SaveSettingsAsync(SystemSettingsViewModel model);
        Task ClearSystemCacheAsync();
    }

    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemSettingsRepository _repository;

        public SystemSettingsService(ISystemSettingsRepository repository)
        {
            _repository = repository;
        }

        // Why: Maps Entity to ViewModel for the UI
        public async Task<SystemSettingsViewModel> GetSettingsAsync()
        {
            var entity = await _repository.GetGlobalSettingsAsync();

            return new SystemSettingsViewModel
            {
                Id = entity.Id,
                SystemName = entity.SystemName,
                SupportEmail = entity.SupportEmail,
                Timezone = entity.Timezone,
                AutoSwitchTeams = entity.AutoSwitchTeams,
                AutoArchive = entity.AutoArchive,
                NotifyOnStageChange = entity.NotifyOnStageChange,
                EnforceProof = entity.EnforceProof,
                AuditRetentionDays = entity.AuditRetentionDays
            };
        }

        // Why: Maps ViewModel back to Entity for saving
        public async Task SaveSettingsAsync(SystemSettingsViewModel model)
        {
            // Fail fast validation
            if (model == null) return;

            try
            {
                var entity = new SystemSetting
                {
                    Id = model.Id,
                    SystemName = "Toro Track",
                    SupportEmail = model.SupportEmail,
                    Timezone = model.Timezone,
                    AutoSwitchTeams = model.AutoSwitchTeams,
                    AutoArchive = model.AutoArchive,
                    NotifyOnStageChange = model.NotifyOnStageChange,
                    EnforceProof = model.EnforceProof,
                    AuditRetentionDays = model.AuditRetentionDays
                };

                await _repository.UpdateSettingsAsync(entity);
                Console.WriteLine("System settings updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service Error saving settings: {ex.Message}");
                throw;
            }
        }

        // Why: Placeholder for clearing server-side caching mechanisms (e.g., IMemoryCache)
        public async Task ClearSystemCacheAsync()
        {
            Console.WriteLine("System cache clearing requested...");
            await Task.CompletedTask;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface ISystemSettingsRepository
    {
        Task<SystemSetting> GetGlobalSettingsAsync();
        Task UpdateSettingsAsync(SystemSetting settings);
    }

    public class SystemSettingsRepository : ISystemSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Why: Ensures a settings record always exists. 
        // We use AsNoTracking() so this entity doesn't stay stuck in the context, preventing update conflicts later.
        public async Task<SystemSetting> GetGlobalSettingsAsync()
        {
            try
            {
                var settings = await _context.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (settings == null)
                {
                    // Create defaults if no record exists yet (First run)
                    settings = new SystemSetting
                    {
                        SystemName = "Toro Track",
                        SupportEmail = "support@toroinformatics.com",
                        Timezone = "South Africa Standard Time (SAST)",
                        AutoSwitchTeams = true,
                        NotifyOnStageChange = true,
                        EnforceProof = true,
                        AuditRetentionDays = 90
                    };

                    _context.SystemSettings.Add(settings);
                    await _context.SaveChangesAsync();

                    // Why: Detach it immediately so subsequent updates don't fail
                    _context.Entry(settings).State = EntityState.Detached;
                }

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository Error fetching system settings: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateSettingsAsync(SystemSetting settings)
        {
            try
            {
                // Why: Fail-safe. Check if the context is already tracking an entity with this ID.
                // If it is, detach it so we can attach the new incoming object without error.
                var local = _context.Set<SystemSetting>()
                    .Local
                    .FirstOrDefault(entry => entry.Id == settings.Id);

                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                _context.SystemSettings.Update(settings);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository Error updating system settings: {ex.Message}");
                throw;
            }
        }
    }
}
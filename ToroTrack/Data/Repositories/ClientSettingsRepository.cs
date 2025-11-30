using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface IClientSettingsRepository
    {
        Task<ClientPreference?> GetPreferencesByUserIdAsync(string userId);
        Task SavePreferencesAsync(ClientPreference preference);
    }

    public class ClientSettingsRepository : IClientSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Fetch preferences. Returns null if not found (first time user)
        public async Task<ClientPreference?> GetPreferencesByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Set<ClientPreference>()
                    .FirstOrDefaultAsync(p => p.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching preferences for {userId}: {ex.Message}");
                throw;
            }
        }

        // Creates or Updates the preference record
        public async Task SavePreferencesAsync(ClientPreference preference)
        {
            try
            {
                if (preference.Id == 0)
                {
                    _context.Set<ClientPreference>().Add(preference);
                }
                else
                {
                    _context.Set<ClientPreference>().Update(preference);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving preferences: {ex.Message}");
                throw;
            }
        }
    }
}
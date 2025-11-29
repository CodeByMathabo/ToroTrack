using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface ILicenseRepository
    {
        Task<List<License>> GetAllLicensesAsync();
        Task<List<License>> GetExpiringLicensesAsync(int daysThreshold);
        Task<License?> GetLicenseByIdAsync(int id);
        Task AddLicenseAsync(License license);
        Task UpdateLicenseAsync(License license);
        Task<List<ApplicationUser>> GetClientsAsync();
    }

    public class LicenseRepository : ILicenseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public LicenseRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<License>> GetAllLicensesAsync()
        {
            // Why: Create a short-lived context to fetch data without long-lived tracking overhead
            using var context = _contextFactory.CreateDbContext();
            return await context.Licenses
                .Include(l => l.Client)
                .OrderByDescending(l => l.Id)
                .ToListAsync();
        }

        public async Task<List<License>> GetExpiringLicensesAsync(int daysThreshold)
        {
            using var context = _contextFactory.CreateDbContext();
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);

            return await context.Licenses
                .Include(l => l.Client)
                .Where(l => l.Status == "Active" && l.ExpiryDate <= thresholdDate)
                .ToListAsync();
        }

        public async Task<License?> GetLicenseByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Licenses
                .Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddLicenseAsync(License license)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Licenses.Add(license);
            await context.SaveChangesAsync();
        }

        public async Task UpdateLicenseAsync(License license)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Licenses.Update(license);
            await context.SaveChangesAsync();
        }

        public async Task<List<ApplicationUser>> GetClientsAsync()
        {
            // Why: Fetch users to populate the client dropdown in the modal
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.ToListAsync();
        }
    }
}
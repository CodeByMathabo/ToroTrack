using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface ILicenseRepository
    {
        Task<List<License>> GetExpiringLicensesAsync(int daysThreshold);
    }

    public class LicenseRepository : ILicenseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public LicenseRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<License>> GetExpiringLicensesAsync(int daysThreshold)
        {
            // Why: Create a short-lived context to fetch data without tracking overhead
            using var context = _contextFactory.CreateDbContext();
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);

            return await context.Licenses
                .Include(l => l.Client)
                .Where(l => l.Status == "Active" && l.ExpiryDate <= thresholdDate)
                .ToListAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetRecentSecurityAnomaliesAsync(DateTime since);
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AuditLogRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<AuditLog>> GetRecentSecurityAnomaliesAsync(DateTime since)
        {
            using var context = _contextFactory.CreateDbContext();

            // Why: Fetch failed logins or suspicious actions
            return await context.AuditLogs
                .Where(l => l.Timestamp >= since &&
                           (l.Action == "FailedLogin" || l.Category == "Security"))
                .ToListAsync();
        }
    }
}
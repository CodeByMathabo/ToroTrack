using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;
using ToroTrack.Models;

namespace ToroTrack.Data.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetRecentSecurityAnomaliesAsync(DateTime since);
        Task<List<AuditLogEntry>> GetAllAuditLogsAsync();
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
            return await context.AuditLogs
                .Where(l => l.Timestamp >= since &&
                           (l.Action == "FailedLogin" || l.Category == "Security"))
                .ToListAsync();
        }

        // Why: Fetches logs joined with User and Role data for the UI
        public async Task<List<AuditLogEntry>> GetAllAuditLogsAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            // Perform a Left Join on Users and then Roles to populate the View Model
            var query = from log in context.AuditLogs
                        join user in context.Users on log.UserId equals user.Id into userJoin
                        from u in userJoin.DefaultIfEmpty()

                            // Join UserRoles to get RoleId
                        join ur in context.UserRoles on u.Id equals ur.UserId into urJoin
                        from userRole in urJoin.DefaultIfEmpty()

                            // Join Roles to get Role Name
                        join r in context.Roles on userRole.RoleId equals r.Id into rJoin
                        from role in rJoin.DefaultIfEmpty()

                        orderby log.Timestamp descending
                        select new AuditLogEntry
                        {
                            Id = log.Id,
                            Timestamp = log.Timestamp,
                            Category = log.Category,
                            ActionType = log.Action,
                            Details = log.Details,
                            IpAddress = log.IpAddress ?? "N/A",
                            UserName = u != null ? u.UserName : "System",
                            UserRole = role != null ? role.Name : (u != null ? "User" : "System")
                        };

            return await query.ToListAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface IReportRepository
    {
        Task<List<ReportLog>> GetRecentReportsAsync(int count);
        Task<ReportLog> LogReportGenerationAsync(ReportLog report);
    }

    public class ReportRepository : IReportRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReportRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ReportLog>> GetRecentReportsAsync(int count)
        {
            using var context = _contextFactory.CreateDbContext();

            // Why: Order by most recent first for the UI history table
            return await context.ReportLogs
                .OrderByDescending(r => r.DateGenerated)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ReportLog> LogReportGenerationAsync(ReportLog report)
        {
            using var context = _contextFactory.CreateDbContext();

            try
            {
                context.ReportLogs.Add(report);
                await context.SaveChangesAsync();
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReportRepository] Error logging report: {ex.Message}");
                throw;
            }
        }
    }
}
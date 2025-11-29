using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public class AuditService
    {
        private readonly IAuditLogRepository _repository;

        public AuditService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AuditLogEntry>> GetFullAuditTrailAsync()
        {
            try
            {
                return await _repository.GetAllAuditLogsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditService] Error fetching logs: {ex.Message}");
                return new List<AuditLogEntry>();
            }
        }
    }
}
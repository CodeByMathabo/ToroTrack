using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface IComplianceService
    {
        Task<(int Score, List<AuditFlag> Flags)> RunComplianceScanAsync();
    }

    public class ComplianceService : IComplianceService
    {
        private readonly ILicenseRepository _licenseRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IAuditLogRepository _auditRepo;
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(
            ILicenseRepository licenseRepo,
            ITaskRepository taskRepo,
            IAuditLogRepository auditRepo,
            ILogger<ComplianceService> logger)
        {
            _licenseRepo = licenseRepo;
            _taskRepo = taskRepo;
            _auditRepo = auditRepo;
            _logger = logger;
        }

        public async Task<(int Score, List<AuditFlag> Flags)> RunComplianceScanAsync()
        {
            var flags = new List<AuditFlag>();

            try
            {
                _logger.LogInformation("Starting Compliance Scan...");

                // 1. CHECK LICENSES (Risk Management)
                var expiringLicenses = await _licenseRepo.GetExpiringLicensesAsync(30);
                foreach (var license in expiringLicenses)
                {
                    var daysLeft = (license.ExpiryDate - DateTime.UtcNow).TotalDays;
                    var severity = daysLeft <= 7 ? "Critical" : "Warning";

                    flags.Add(new AuditFlag
                    {
                        Severity = severity,
                        Category = "Licensing",
                        Message = $"{license.SoftwareName} expires in {Math.Ceiling(daysLeft)} days.",
                        EntityName = license.Client?.CompanyName ?? "Unknown Client",
                        ActionUrl = $"/auditor/license/edit/{license.Id}"
                    });
                }

                // CHECK PROCESS GAPS (Proof of Work)
                var invalidTasks = await _taskRepo.GetVerifiedTasksWithoutProofAsync();
                foreach (var task in invalidTasks)
                {
                    flags.Add(new AuditFlag
                    {
                        Severity = "Warning", // Process failure is usually a warning unless systemic
                        Category = "Process",
                        Message = $"Task '{task.Title}' marked Verified without proof.",
                        EntityName = task.Project?.Client?.CompanyName ?? "Internal Team",
                        ActionUrl = $"/admin/verify-tasks"
                    });
                }

                // CHECK SECURITY (Access Control)
                var anomalies = await _auditRepo.GetRecentSecurityAnomaliesAsync(DateTime.UtcNow.AddHours(-24));
                var failedLogins = anomalies.GroupBy(x => x.UserId).Where(g => g.Count() > 3);

                foreach (var group in failedLogins)
                {
                    flags.Add(new AuditFlag
                    {
                        Severity = group.Count() > 5 ? "Critical" : "Low",
                        Category = "Security",
                        Message = $"{group.Count()} failed login attempts detected.",
                        EntityName = group.Key ?? "Unknown User"
                    });
                }

                // CALCULATE SCORE
                int deduction = (flags.Count(f => f.Severity == "Critical") * 15)
                              + (flags.Count(f => f.Severity == "Warning") * 5)
                              + (flags.Count(f => f.Severity == "Low") * 2);

                int score = Math.Max(0, 100 - deduction);

                return (score, flags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compliance Scan Failed");
                // Return a system error flag so the UI knows something went wrong
                flags.Add(new AuditFlag
                {
                    Severity = "Critical",
                    Category = "System",
                    Message = "Audit Engine Failure. Check logs.",
                    EntityName = "System Internal"
                });
                return (0, flags);
            }
        }
    }
}
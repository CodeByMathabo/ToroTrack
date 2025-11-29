using Microsoft.EntityFrameworkCore;
using ToroTrack.Data;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface IComplianceService
    {
        Task<(int Score, List<AuditFlag> Flags)> RunComplianceScanAsync();
        Task ResolveRiskAsync(AuditFlag flag, string actionType);
    }

    public class ComplianceService : IComplianceService
    {
        private readonly ILicenseRepository _licenseRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IAuditLogRepository _auditRepo;
        private readonly INotificationService _notificationService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(
            ILicenseRepository licenseRepo,
            ITaskRepository taskRepo,
            IAuditLogRepository auditRepo,
            INotificationService notificationService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ComplianceService> logger)
        {
            _licenseRepo = licenseRepo;
            _taskRepo = taskRepo;
            _auditRepo = auditRepo;
            _notificationService = notificationService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<(int Score, List<AuditFlag> Flags)> RunComplianceScanAsync()
        {
            // Note: Keeping existing scan logic as is, focusing refactor on ResolveRiskAsync
            // for the notification requirement.
            var flags = new List<AuditFlag>();

            try
            {
                _logger.LogInformation("Starting Compliance Scan...");

                // 1. Check Licenses
                var expiringLicenses = await _licenseRepo.GetExpiringLicensesAsync(30);
                flags.AddRange(expiringLicenses.Select(MapLicenseToFlag));

                // 2. Check Process Gaps
                var invalidTasks = await _taskRepo.GetVerifiedTasksWithoutProofAsync();
                flags.AddRange(invalidTasks.Select(MapTaskToFlag));

                // 3. Check Security
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

                return (CalculateScore(flags), flags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compliance Scan Failed");
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

        // --- Resolution Logic ---
         public async Task ResolveRiskAsync(AuditFlag flag, string actionType)
        {
            try
            {
                _logger.LogInformation($"Attempting to resolve risk. Type: {flag.EntityType}, Action: {actionType}");

                // Why: Handle failure/ignore cases first to keep main logic flat 
                if (flag.EntityType != "Task" || actionType != "Ping")
                {
                    _logger.LogWarning($"Skipping resolution. Unsupported Type/Action combination: {flag.EntityType}/{actionType}");
                    return;
                }

                await ProcessTaskPingAsync(flag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving risk for {flag.EntityType} {flag.EntityId}");
                throw;
            }
        }

        private async Task ProcessTaskPingAsync(AuditFlag flag)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var task = await context.ProjectTasks.FindAsync(flag.EntityId);

            // Why: Avoid nesting by returning early if data is missing [cite: 99]
            if (task == null)
            {
                _logger.LogError($"Task not found for ID: {flag.EntityId}");
                return;
            }

            // 1. Revert Task Status
            task.Status = "Changes Requested";
            context.ProjectTasks.Update(task);
            await context.SaveChangesAsync();

            _logger.LogInformation($"Task {task.Id} status reverted to 'Changes Requested'.");

            // 2. Send Notification
            await NotifyTeamMemberAsync(flag, task.Title);
        }

        private async Task NotifyTeamMemberAsync(AuditFlag flag, string taskTitle)
        {
            // Why: Separate notification logic to ensure single responsibility per function
            if (string.IsNullOrEmpty(flag.TargetUserId))
            {
                _logger.LogWarning($"No TargetUserId found for Flag {flag.Id}. Notification skipped.");
                return;
            }

            _logger.LogInformation($"Sending notification to User: {flag.TargetUserId}");

            // Why: We use "Alert" as the type so it shows up Red in the Notification UI
            await _notificationService.SendNotificationAsync(
                flag.TargetUserId,
                $"AUDIT ACTION: Proof missing for '{taskTitle}'. Status reverted. Please fix immediately.",
                "Alert",
                "/team/kanban"
            );
        }

        // --- Helper Methods to Reduce Scan Complexity ---

        private AuditFlag MapLicenseToFlag(License license)
        {
            var daysLeft = (license.ExpiryDate - DateTime.UtcNow).TotalDays;
            return new AuditFlag
            {
                Severity = daysLeft <= 7 ? "Critical" : "Warning",
                Category = "Licensing",
                Message = $"{license.SoftwareName} expires in {Math.Ceiling(daysLeft)} days.",
                EntityName = license.Client?.CompanyName ?? "Unknown Client",
                ActionUrl = $"/auditor/license/edit/{license.Id}",
                EntityId = license.Id,
                EntityType = "License"
            };
        }

        private AuditFlag MapTaskToFlag(ProjectTask task)
        {
            // Why: Identify target user safely to prevent null reference issues later
            string targetUser = !string.IsNullOrEmpty(task.AssignedToId)
                ? task.AssignedToId
                : task.Project?.ClientId;

            return new AuditFlag
            {
                Severity = "Warning",
                Category = "Process",
                Message = $"Task '{task.Title}' marked Verified without proof.",
                EntityName = task.Project?.Client?.CompanyName ?? "Internal Team",
                ActionUrl = $"/admin/verify-tasks",
                EntityId = task.Id,
                EntityType = "Task",
                TargetUserId = targetUser
            };
        }

        private int CalculateScore(List<AuditFlag> flags)
        {
            int deduction = (flags.Count(f => f.Severity == "Critical") * 15)
                          + (flags.Count(f => f.Severity == "Warning") * 5)
                          + (flags.Count(f => f.Severity == "Low") * 2);

            return Math.Max(0, 100 - deduction);
        }
    }
}
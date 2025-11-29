using Microsoft.JSInterop;
using System.Text;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;

namespace ToroTrack.Business.Services
{
    public class ReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IAuditLogRepository _auditRepository; // Reusing existing repo
        private readonly IJSRuntime _jsRuntime;

        public ReportService(
            IReportRepository reportRepository,
            IAuditLogRepository auditRepository,
            IJSRuntime jsRuntime)
        {
            _reportRepository = reportRepository;
            _auditRepository = auditRepository;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<ReportLog>> GetReportHistoryAsync()
        {
            return await _reportRepository.GetRecentReportsAsync(10);
        }

        public async Task<ReportLog> GenerateReportAsync(string type, string format, DateTime start, DateTime end, string userName)
        {
            Console.WriteLine($"[ReportService] Generating {type} report ({format}) for {userName}");

            // 1. Fetch relevant data based on report type
            // Note: In a real scenario, this would call specific services to build the file content
            var dataCount = await FetchDataCountForReport(type, start, end);

            // 2. Create the log entry
            var report = new ReportLog
            {
                Name = $"{FormatReportName(type)} ({start:MMM dd}-{end:MMM dd})",
                Type = type,
                Format = format,
                DateGenerated = DateTime.UtcNow,
                GeneratedBy = userName,
                PeriodStart = start,
                PeriodEnd = end
            };

            // 3. Persist the log
            return await _reportRepository.LogReportGenerationAsync(report);
        }

        public async Task DownloadReportAsync(ReportLog report)
        {
            try
            {
                // Why: Simulating file content generation. In production, use a library like iTextSharp or CsvHelper.
                string content = $"Report: {report.Name}\nGenerated: {report.DateGenerated}\nType: {report.Type}\n\n[Data Content Would Go Here]";
                byte[] fileBytes = Encoding.UTF8.GetBytes(content);
                string fileName = $"{report.Name.Replace(" ", "_")}.txt"; // Using .txt for demo, replace with .pdf/.csv

                // Trigger browser download
                await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, Convert.ToBase64String(fileBytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReportService] Error downloading report: {ex.Message}");
            }
        }

        // Helper to simulate data fetching logic
        private async Task<int> FetchDataCountForReport(string type, DateTime start, DateTime end)
        {
            if (type == "SecurityAudit")
            {
                var logs = await _auditRepository.GetRecentSecurityAnomaliesAsync(start);
                return logs.Count;
            }
            return 0; // Default for others for now
        }

        private string FormatReportName(string type)
        {
            return type switch
            {
                "ComplianceSummary" => "Compliance Risk Summary",
                "AssetInventory" => "Full Asset Inventory",
                "SecurityAudit" => "Security Audit Log",
                "LicensingCosts" => "Licensing & Cost Projection",
                _ => "General Report"
            };
        }
    }
}
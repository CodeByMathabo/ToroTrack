namespace ToroTrack.Models
{
    // Why: Separating the UI model from the Razor page for better reusability and testing.
    public class AuditFlag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Severity { get; set; } = "Low"; // Critical, Warning, Low
        public string Category { get; set; } = "System"; // Licensing, Process, Security
        public string Message { get; set; } = "";
        public string EntityName { get; set; } = ""; // Who is affected
        public string? ActionUrl { get; set; }
    }
}
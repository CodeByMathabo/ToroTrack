using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Data.Entities
{
    // Why: Stores global configuration for the application.
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SystemName { get; set; } = "Toro Track";

        public string SupportEmail { get; set; } = string.Empty;

        public string Timezone { get; set; } = "South Africa Standard Time (SAST)";

        // Automation Toggles
        public bool AutoSwitchTeams { get; set; }
        public bool AutoArchive { get; set; }
        public bool NotifyOnStageChange { get; set; }

        // Compliance Toggles
        public bool EnforceProof { get; set; }
        public int AuditRetentionDays { get; set; } = 90;
    }
}
using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Models
{
    // Why: Defines the shape of data used in the Admin Settings UI.
    public class SystemSettingsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "System Name")]
        public string SystemName { get; set; } = "Toro Track";

        [Required]
        [EmailAddress]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; } = string.Empty;

        public string Timezone { get; set; } = "South Africa Standard Time (SAST)";

        // Automation
        public bool AutoSwitchTeams { get; set; }
        public bool AutoArchive { get; set; }
        public bool NotifyOnStageChange { get; set; }

        // Compliance
        public bool EnforceProof { get; set; }

        [Range(1, 365, ErrorMessage = "Retention must be between 1 and 365 days")]
        public int AuditRetentionDays { get; set; }
    }
}
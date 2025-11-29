using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    public class License
    {
        public int Id { get; set; }

        [Required]
        public string SoftwareName { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        // Why: Links the license to a specific client (ApplicationUser)
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public string Status { get; set; } = "Active"; // Active, Expired, Renewed

        // Why: Required for Auditor Portal UI to distinguish asset types
        public string Type { get; set; } = "Software"; // e.g., Software, Hardware

        // Why: Tracks if this was automated or manually overridden
        public string Source { get; set; } = "Manual Entry"; // e.g., System Order, Manual Entry

        // Why: Financial tracking per seat/unit
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
    }
}
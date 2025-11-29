using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    // Why: Defines the structure for storing generated report metadata in the database
    public class ReportLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "General"; // Compliance, Assets, Security, Licensing

        [Required]
        public string Format { get; set; } = "PDF"; // PDF, CSV

        public DateTime DateGenerated { get; set; } = DateTime.UtcNow;

        public string GeneratedBy { get; set; } = "System";

        // Why: Storing parameters allows us to re-generate the report if needed without storing large blobs
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
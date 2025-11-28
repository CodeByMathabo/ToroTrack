using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    public class Meeting
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int DurationMinutes { get; set; } = 30;

        public string? MeetingLink { get; set; }

        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        // Link meeting to a specific project
        public int? ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
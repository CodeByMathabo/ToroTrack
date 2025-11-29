using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Who receives the notification

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Message { get; set; } = "";

        public string Type { get; set; } = "Info"; // Alert, Info, Success
        public string? ActionUrl { get; set; } // Link to the task/issue

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    // Stores specific notification settings for a client
    public class ClientPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // Preference Toggles
        public bool NotifyOnMilestone { get; set; } = true;
        public bool NotifyOnQueryReply { get; set; } = true;
        public bool NotifyOnMeeting { get; set; } = false;
    }
}
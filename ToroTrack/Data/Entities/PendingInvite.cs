using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Data.Entities
{
    public class PendingInvite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        // Client-specific details [cite: 1471]
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactNumber { get; set; }
        public string? ClientJobRole { get; set; }

        public DateTime DateInvited { get; set; } = DateTime.UtcNow;

        // Token could be used for the registration link later
        public string InviteToken { get; set; } = Guid.NewGuid().ToString();
    }
}
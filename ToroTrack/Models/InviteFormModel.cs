using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Models
{
    // Why: Separated from the UI file to keep the component "dumb" and focused only on rendering.
    public class InviteFormModel
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Client";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        // Client-specific optional fields
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactNumber { get; set; }
        public string? ClientJobRole { get; set; }
    }
}
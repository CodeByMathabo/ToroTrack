using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Data
{
    // This class represents the "AspNetUsers" table in the database. Added 'FirstName', 'LastName', etc., so they are stored for every user.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        // '?' means these are OPTIONAL. Admins/Auditors will have null here. Clients will have data.
        public string? CompanyName { get; set; }
        public string? JobRole { get; set; }

        // A helper to easily display the full name in the UI
        public string FullName => $"{FirstName} {LastName}";
    }
}
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

        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public string Status { get; set; } = "Active"; // Active, Expired, Renewed
    }
}
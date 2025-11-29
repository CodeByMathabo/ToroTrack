using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string AssignedTeam { get; set; } = "Platform Engineer";

        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public List<ProjectTask> Tasks { get; set; } = new();
    }

    public class ProjectTask
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [Required]
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Backlog";
        public string Priority { get; set; } = "Medium";

        // Stores the role for this specific task
        public string AssignedToId { get; set; } = "";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? ProofUrl { get; set; }
    }

    public class CatalogItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Hardware";
        public string Description { get; set; } = "";
        public decimal Cost { get; set; }
        public int StockQuantity { get; set; }
    }

    public class ClientAsset
    {
        public int Id { get; set; }

        // Made nullable because a custom project delivery might not match a catalog item
        public int? CatalogItemId { get; set; }

        public string ClientId { get; set; } = "";
        public string? SerialNumber { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // Added these fields to support the "Delivered Assets" feature
        public string Name { get; set; } = "Unnamed Asset";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string? ImageUrl { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = "";
        public string Category { get; set; } = "System";
        public string Details { get; set; } = "";
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
    }
}
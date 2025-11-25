using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    // PROJECTS (Managed by Admin, Viewed by Client)
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Active"; // Active, Suspended, Completed

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        // Link to the Client (The User who owns this project)
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        // A project contains many tasks
        public List<ProjectTask> Tasks { get; set; } = new();
    }

    // TASKS (The work items on the Kanban board)
    public class ProjectTask
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Backlog"; // Backlog, Doing, Done, Verified

        // Which Project does this belong to?
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // Who is working on this? (Team Member)
        public string? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public ApplicationUser? AssignedTo { get; set; }

        // Evidence link (required to mark as Done)
        public string? ProofUrl { get; set; }
    }

    // CATALOG (Items available for order)
    public class CatalogItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Hardware";
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int StockQuantity { get; set; }
    }

    // CLIENT ASSETS (Items the client actually owns)
    public class ClientAsset
    {
        public int Id { get; set; }

        // What item is it?
        public int CatalogItemId { get; set; }
        public CatalogItem? CatalogItem { get; set; }

        // Who owns it?
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public string? SerialNumber { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }

    // AUDIT LOGS (Security Trail)
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = string.Empty;
        public string Category { get; set; } = "System";
        public string Details { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
}
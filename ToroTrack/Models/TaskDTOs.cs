using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Backlog";
        public string AssignedToName { get; set; } = "Unassigned";
        public string ProjectName { get; set; } = "";
        public string? ProofUrl { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class CreateTaskModel
    {
        [Required(ErrorMessage = "Task Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        // REMOVED: [Required] StartDate. 
        // Logic change: System sets this automatically.
        public string? AssignedToId { get; set; }
    }

    public class UpdateTaskStatusModel
    {
        public int TaskId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? ProofUrl { get; set; }
    }
}
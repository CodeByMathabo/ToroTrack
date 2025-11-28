using System.ComponentModel.DataAnnotations;

namespace ToroTrack.Models
{
    public class ProjectInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please select a client.")]
        public string SelectedClientId { get; set; } = "";

        // Admin selects the starting team here
        [Required(ErrorMessage = "Please select the starting team.")]
        public string AssignedTeam { get; set; } = "Platform Engineer";

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now.AddMonths(1);
    }

    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string ClientEmail { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string CurrentTeam { get; set; } = "";
        public int ProgressPercent { get; set; }
        public bool IsActive { get; set; }
    }
    public class ClientProjectViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClientStageViewModel
    {
        public int Id { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string AssignedTeam { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public int ProgressPercent { get; set; }
    }
}
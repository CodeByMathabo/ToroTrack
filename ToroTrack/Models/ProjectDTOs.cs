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
}
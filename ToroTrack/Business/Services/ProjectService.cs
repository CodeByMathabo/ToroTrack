using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface IProjectService
    {
        Task<List<ProjectViewModel>> GetAllProjectsAsync();
        Task<List<ApplicationUser>> GetActiveClientsAsync();
        Task CreateProjectAsync(ProjectInputModel model);
        Task UpdateProjectAsync(ProjectInputModel model);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProjectService> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ProjectService(
            IProjectRepository repository,
            UserManager<ApplicationUser> userManager,
            ILogger<ProjectService> logger,
            IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _repository = repository;
            _userManager = userManager;
            _logger = logger;
            _dbFactory = dbFactory;
        }

        public async Task<List<ProjectViewModel>> GetAllProjectsAsync()
        {
            var entities = await _repository.GetAllProjectsAsync();

            return entities.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ClientName = p.Client != null ? $"{p.Client.FirstName} {p.Client.LastName}" : "Unknown",
                ClientEmail = p.Client?.Email ?? "N/A",
                StartDate = p.StartDate.ToLocalTime(),
                DueDate = (p.EndDate ?? DateTime.Now).ToLocalTime(),

                // READ: Directly from the new database column
                CurrentTeam = p.AssignedTeam,

                ProgressPercent = p.Tasks.Any()
                    ? (int)((double)p.Tasks.Count(t => t.Status == "Verified") / p.Tasks.Count * 100)
                    : 0,
                IsActive = p.Status == "Active"
            }).ToList();
        }

        public async Task<List<ApplicationUser>> GetActiveClientsAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("Client")).ToList();
        }

        public async Task CreateProjectAsync(ProjectInputModel model)
        {
            _logger.LogInformation("Creating new project: {Name} for Team: {Team}", model.Name, model.AssignedTeam);

            var entity = new Project
            {
                Name = model.Name,
                Description = "Initialized via Admin Portal",
                ClientId = model.SelectedClientId,

                // SAVE: Persist the Admin's choice
                AssignedTeam = model.AssignedTeam,

                StartDate = model.StartDate.ToUniversalTime(),
                EndDate = model.DueDate.ToUniversalTime(),
                Status = "Active"
            };

            await _repository.AddProjectAsync(entity);

            // AUTOMATION: Create Stage 1 Task assigned to the CHOSEN team
            using var context = _dbFactory.CreateDbContext();

            var initialTask = new ProjectTask
            {
                ProjectId = entity.Id,
                Title = "Stage 1: Initial Setup & Planning",
                Description = $"Initial phase execution for {model.AssignedTeam}. Please review project scope.",
                Status = "Backlog",
                Priority = "High",

                // Task is assigned to the selected team
                AssignedToId = model.AssignedTeam,

                DueDate = model.StartDate.AddDays(5).ToUniversalTime()
            };

            context.ProjectTasks.Add(initialTask);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(ProjectInputModel model)
        {
            var entity = await _repository.GetProjectByIdAsync(model.Id);
            if (entity == null) throw new Exception("Project not found");

            entity.Name = model.Name;
            entity.AssignedTeam = model.AssignedTeam; // Allow updates
            entity.StartDate = model.StartDate.ToUniversalTime();
            entity.EndDate = model.DueDate.ToUniversalTime();

            await _repository.UpdateProjectAsync(entity);
        }
    }
}
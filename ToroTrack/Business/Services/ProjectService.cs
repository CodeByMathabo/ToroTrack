using Microsoft.AspNetCore.Identity;
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

        public ProjectService(
            IProjectRepository repository,
            UserManager<ApplicationUser> userManager,
            ILogger<ProjectService> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _logger = logger;
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
                StartDate = p.StartDate,
                DueDate = p.EndDate ?? p.StartDate.AddMonths(1),
                IsActive = p.Status == "Active",
                ProgressPercent = p.Tasks.Any()
                    ? (int)((double)p.Tasks.Count(t => t.Status == "Verified") / p.Tasks.Count * 100)
                    : 0,
                CurrentTeam = "Infrastructure"
            }).ToList();
        }

        public async Task<List<ApplicationUser>> GetActiveClientsAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("Client")).ToList();
        }

        public async Task CreateProjectAsync(ProjectInputModel model)
        {
            _logger.LogInformation("Creating new project: {Name}", model.Name);

            var entity = new Project
            {
                Name = model.Name, // Verify this line maps the name
                Description = "Initialized via Admin Portal",
                ClientId = model.SelectedClientId,
                StartDate = model.StartDate.ToUniversalTime(),
                EndDate = model.DueDate.ToUniversalTime(),
                Status = "Active"
            };

            await _repository.AddProjectAsync(entity);
        }

        public async Task UpdateProjectAsync(ProjectInputModel model)
        {
            var entity = await _repository.GetProjectByIdAsync(model.Id);
            if (entity == null) throw new Exception("Project not found");

            entity.Name = model.Name; 
            entity.StartDate = model.StartDate;
            entity.EndDate = model.DueDate;

            await _repository.UpdateProjectAsync(entity);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task AddProjectAsync(Project project);
        Task UpdateProjectAsync(Project project);
    }

    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ProjectRepository> _logger;

        public ProjectRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<ProjectRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Why: Include Client for names and Tasks for progress calculation
            return await context.Projects
                .Include(p => p.Client)
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Projects.FindAsync(id);
        }

        public async Task AddProjectAsync(Project project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Projects.Add(project);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Projects.Update(project);
            await context.SaveChangesAsync();
        }
    }
}
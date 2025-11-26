using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface ITaskRepository
    {
        Task<List<ProjectTask>> GetTasksByProjectAsync(int projectId);
        Task<List<ProjectTask>> GetAllTasksAsync();
        Task<ProjectTask?> GetTaskByIdAsync(int id);
        Task AddTaskAsync(ProjectTask task);
        Task UpdateTaskAsync(ProjectTask task);
    }

    public class TaskRepository : ITaskRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public TaskRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ProjectTask>> GetTasksByProjectAsync(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectTasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAllTasksAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectTasks
                .Include(t => t.Project)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectTasks.FindAsync(id);
        }

        public async Task AddTaskAsync(ProjectTask task)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.ProjectTasks.Add(task);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(ProjectTask task)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.ProjectTasks.Update(task);
            await context.SaveChangesAsync();
        }
    }
}
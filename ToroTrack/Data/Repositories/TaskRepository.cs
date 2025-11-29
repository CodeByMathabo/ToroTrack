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
        Task<List<ProjectTask>> GetVerifiedTasksWithoutProofAsync();
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

        // Why: Identifies tasks marked 'Verified' that lack a ProofUrl, indicating a process gap.
        public async Task<List<ProjectTask>> GetVerifiedTasksWithoutProofAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Why: Logging to trace compliance check execution
                Console.WriteLine("TaskRepository: Querying verified tasks without proof...");

                var invalidTasks = await context.ProjectTasks
                    .Include(t => t.Project)
                        .ThenInclude(p => p.Client)
                    .Where(t => t.Status == "Verified" && string.IsNullOrEmpty(t.ProofUrl))
                    .ToListAsync();

                Console.WriteLine($"TaskRepository: Found {invalidTasks.Count} tasks violating verification rules.");

                return invalidTasks;
            }
            catch (Exception ex)
            {
                // Why: Log error but return empty list to prevent crashing the entire compliance dashboard
                Console.WriteLine($"TaskRepository Error in GetVerifiedTasksWithoutProofAsync: {ex.Message}");
                return new List<ProjectTask>();
            }
        }
    }
}
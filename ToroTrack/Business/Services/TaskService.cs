using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;
using ToroTrack.Models;

namespace ToroTrack.Business.Services
{
    public interface ITaskService
    {
        Task<List<TaskViewModel>> GetKanbanTasksAsync(int? projectId = null);
        Task CreateTaskAsync(CreateTaskModel model);
        Task UpdateTaskStatusAsync(UpdateTaskStatusModel model);
    }

    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<TaskViewModel>> GetKanbanTasksAsync(int? projectId = null)
        {
            var tasks = projectId.HasValue
                ? await _repository.GetTasksByProjectAsync(projectId.Value)
                : await _repository.GetAllTasksAsync();

            return tasks.Select(t => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                ProofUrl = t.ProofUrl,
                ProjectName = t.Project?.Name ?? "Unknown Project",

                // Use the ID directly as it now stores the Team Name
                AssignedToName = t.AssignedToId,

                DueDate = t.DueDate,
                StartDate = t.StartDate
            }).ToList();
        }

        public async Task CreateTaskAsync(CreateTaskModel model)
        {
            var utcStart = DateTime.UtcNow;

            var entity = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                ProjectId = model.ProjectId,
                AssignedToId = model.AssignedToId ?? "Unassigned",
                Status = "Backlog",
                StartDate = utcStart,
                DueDate = utcStart.AddDays(5)
            };

            await _repository.AddTaskAsync(entity);
        }

        public async Task UpdateTaskStatusAsync(UpdateTaskStatusModel model)
        {
            var task = await _repository.GetTaskByIdAsync(model.TaskId);
            if (task == null) throw new Exception("Task not found");

            if (model.NewStatus == "Done" && string.IsNullOrWhiteSpace(model.ProofUrl))
            {
                if (string.IsNullOrWhiteSpace(task.ProofUrl))
                {
                    throw new InvalidOperationException("Proof of work is required to complete a task.");
                }
            }

            if (!string.IsNullOrWhiteSpace(model.ProofUrl))
            {
                task.ProofUrl = model.ProofUrl;
            }

            task.Status = model.NewStatus;
            await _repository.UpdateTaskAsync(task);
        }
    }
}
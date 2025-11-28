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
        Task<List<ClientProjectViewModel>> GetProjectsForClientAsync(string clientId);
        Task<List<ClientStageViewModel>> GetProjectStagesAsync(int projectId);
        Task SignOffStageAsync(int taskId, string signerName);
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
            try
            {
                var entities = await _repository.GetAllProjectsAsync();

                return entities.Select(p =>
                {
                    int totalTasks = p.Tasks.Count;
                    int completedTasks = p.Tasks.Count(t => t.Status == "Verified" || t.Status == "Done");

                    int progress = totalTasks > 0 ? (int)((double)completedTasks / totalTasks * 100) : 0;

                    return new ProjectViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ClientName = p.Client != null ? $"{p.Client.FirstName} {p.Client.LastName}" : "Unknown",
                        ClientEmail = p.Client?.Email ?? "N/A",
                        StartDate = p.StartDate.ToLocalTime(),
                        DueDate = (p.EndDate ?? DateTime.Now).ToLocalTime(),
                        CurrentTeam = p.AssignedTeam,
                        ProgressPercent = progress,
                        IsActive = p.Status == "Active"
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all projects.");
                return new List<ProjectViewModel>();
            }
        }

        public async Task<List<ApplicationUser>> GetActiveClientsAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("Client")).ToList();
        }

        public async Task CreateProjectAsync(ProjectInputModel model)
        {
            try
            {
                _logger.LogInformation("Creating new project: {Name} for Team: {Team}", model.Name, model.AssignedTeam);

                var entity = new Project
                {
                    Name = model.Name,
                    Description = "Initialized via Admin Portal",
                    ClientId = model.SelectedClientId,
                    AssignedTeam = model.AssignedTeam,
                    StartDate = model.StartDate.ToUniversalTime(),
                    EndDate = model.DueDate.ToUniversalTime(),
                    Status = "Active"
                };

                await _repository.AddProjectAsync(entity);

                await SeedProjectTasksAsync(entity.Id, model.AssignedTeam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project.");
                throw;
            }
        }

        public async Task UpdateProjectAsync(ProjectInputModel model)
        {
            try
            {
                var entity = await _repository.GetProjectByIdAsync(model.Id);
                if (entity == null) throw new Exception("Project not found");

                entity.Name = model.Name;
                entity.AssignedTeam = model.AssignedTeam;
                entity.StartDate = model.StartDate.ToUniversalTime();
                entity.EndDate = model.DueDate.ToUniversalTime();

                await _repository.UpdateProjectAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {Id}", model.Id);
                throw;
            }
        }

        // --- CLIENT METHODS IMPLEMENTATION ---

        public async Task<List<ClientProjectViewModel>> GetProjectsForClientAsync(string clientId)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();

                var projects = await context.Projects
                    .Include(p => p.Client)
                    .Where(p => p.ClientId == clientId)
                    .OrderByDescending(p => p.Status == "Active")
                    .ThenByDescending(p => p.StartDate)
                    .ToListAsync();

                return projects.Select(p => new ClientProjectViewModel
                {
                    Id = p.Id,
                    ProjectName = p.Name,
                    ClientName = p.Client != null ? $"{p.Client.FirstName} {p.Client.LastName}" : "Unknown",
                    StartDate = p.StartDate,
                    DueDate = p.EndDate ?? DateTime.UtcNow.AddDays(30),
                    IsActive = p.Status == "Active"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client projects for {ClientId}", clientId);
                return new List<ClientProjectViewModel>();
            }
        }

        public async Task<List<ClientStageViewModel>> GetProjectStagesAsync(int projectId)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();

                var tasks = await context.ProjectTasks
                    .Where(t => t.ProjectId == projectId)
                    .OrderBy(t => t.Id)
                    .ToListAsync();

                return tasks.Select(t => new ClientStageViewModel
                {
                    Id = t.Id,
                    StageName = t.Title,
                    AssignedTeam = string.IsNullOrEmpty(t.AssignedToId) ? "Project Team" : t.AssignedToId,
                    Status = MapDbStatusToUiStatus(t.Status),
                    ProgressPercent = MapStatusToProgress(t.Status)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stages for project {ProjectId}", projectId);
                return new List<ClientStageViewModel>();
            }
        }

        public async Task SignOffStageAsync(int taskId, string signerName)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();

                var task = await context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task != null)
                {
                    task.Status = "Verified";
                    task.Description += $" | Signed off by {signerName} on {DateTime.UtcNow}";
                    task.CompletedDate = DateTime.UtcNow;

                    // Create Asset if it's the final stage
                    if (IsFinalLogisticsStage(task.Title))
                    {
                        var newAsset = new ClientAsset
                        {
                            Name = $"Delivered System - {task.Project.Name}",
                            Description = "Asset automatically registered upon client sign-off.",
                            ClientId = task.Project.ClientId,
                            AssignedDate = DateTime.UtcNow,
                            SerialNumber = $"SN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            Status = "Delivered",
                            ImageUrl = "/appImages/server-icon.png",
                            CatalogItemId = null 
                        };

                        context.ClientAssets.Add(newAsset);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Task {TaskId} signed off by {Signer}", taskId, signerName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing off task {TaskId}", taskId);
                throw;
            }
        }

        // --- HELPER METHODS ---

        private bool IsFinalLogisticsStage(string title)
        {
            return title.Contains("Stage 5") || title.Contains("Final Dispatch");
        }

        private async Task SeedProjectTasksAsync(int projectId, string teamName)
        {
            using var context = _dbFactory.CreateDbContext();
            var workflowSteps = GetWorkflowForTeam(teamName);

            foreach (var step in workflowSteps)
            {
                context.ProjectTasks.Add(new ProjectTask
                {
                    ProjectId = projectId,
                    Title = step.Title,
                    Description = step.Description,
                    Status = "Backlog",
                    AssignedToId = teamName,
                    StartDate = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        private string MapDbStatusToUiStatus(string dbStatus)
        {
            return dbStatus switch
            {
                "Verified" => "Completed",
                "Done" => "Pending Sign-off",
                "In Progress" => "Active",
                _ => "Pending"
            };
        }

        private int MapStatusToProgress(string dbStatus)
        {
            return dbStatus switch
            {
                "Verified" => 100,
                "Done" => 100,
                "In Progress" => 50,
                _ => 0
            };
        }

        private List<(string Title, string Description)> GetWorkflowForTeam(string teamName)
        {
            return teamName switch
            {
                "Logistics Coordinator" => new()
                {
                    ("Stage 1: Catalog Verification", "Review the 'Catalog' page."),
                    ("Stage 2: Order Monitoring", "Monitor incoming requests."),
                    ("Stage 3: Stock Allocation", "Physically set aside hardware."),
                    ("Stage 4: Asset Registration", "Register serial numbers."),
                    ("Stage 5: Final Dispatch", "Coordinate courier pickup.")
                },
                "Platform Engineer" => new()
                {
                    ("Stage 1: Infrastructure Audit", "Assess current environment."),
                    ("Stage 2: Virtualization Setup", "Configure Hyper-V/VMware."),
                    ("Stage 3: Server Provisioning", "Deploy Windows Server."),
                    ("Stage 4: Backup Configuration", "Setup backup routines."),
                    ("Stage 5: Disaster Recovery Test", "Simulate failure.")
                },
                "Network Security Engineer" => new()
                {
                    ("Stage 1: Firewall Rule Analysis", "Audit existing firewall rules."),
                    ("Stage 2: VLAN Segmentation", "Configure network segments."),
                    ("Stage 3: VPN Configuration", "Setup secure remote access."),
                    ("Stage 4: IPS/IDS Tuning", "Calibrate intrusion detection."),
                    ("Stage 5: Penetration Test", "Run internal vulnerability scan.")
                },
                "Cloud Engineer" => new()
                {
                    ("Stage 1: Tenant Initialization", "Setup Azure/AWS tenant."),
                    ("Stage 2: Identity Sync", "Configure Azure AD Connect."),
                    ("Stage 3: Data Migration", "Execute lift-and-shift."),
                    ("Stage 4: Cutover", "Switch DNS records."),
                    ("Stage 5: Post-Migration Support", "Monitor logs.")
                },
                "Modern Workplace Engineer" => new()
                {
                    ("Stage 1: M365 Licensing", "Assign Business Premium licenses."),
                    ("Stage 2: Email Migration", "Migrate Exchange mailboxes."),
                    ("Stage 3: Endpoint Manager", "Enroll devices in Intune."),
                    ("Stage 4: Policy Deployment", "Push security policies."),
                    ("Stage 5: User Training", "Conduct Teams training.")
                },
                _ => new()
                {
                    ("Stage 1: Initiation", "Define project scope."),
                    ("Stage 2: Execution", "Perform core project tasks."),
                    ("Stage 3: Closure", "Finalize project.")
                }
            };
        }
    }
}
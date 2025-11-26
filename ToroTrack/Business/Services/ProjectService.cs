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

            return entities.Select(p => {
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

        public async Task<List<ApplicationUser>> GetActiveClientsAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("Client")).ToList();
        }

        public async Task CreateProjectAsync(ProjectInputModel model)
        {
            _logger.LogInformation("Creating new project: {Name} for Team: {Team}", model.Name, model.AssignedTeam);

            // Create the Project Record ONLY
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

        }

        public async Task UpdateProjectAsync(ProjectInputModel model)
        {
            var entity = await _repository.GetProjectByIdAsync(model.Id);
            if (entity == null) throw new Exception("Project not found");

            entity.Name = model.Name;
            entity.AssignedTeam = model.AssignedTeam;
            entity.StartDate = model.StartDate.ToUniversalTime();
            entity.EndDate = model.DueDate.ToUniversalTime();

            await _repository.UpdateProjectAsync(entity);
        }

        // --- WORKFLOW DEFINITIONS ---
        private List<(string Title, string Description)> GetWorkflowForTeam(string teamName)
        {
            return teamName switch
            {
                "Logistics Coordinator" => new()
                {
                    // Keyword: "Catalog" -> Links to /admin/catalog
                    ("Stage 1: Catalog Verification", "Review the 'Catalog' page. Ensure stock levels are accurate before client ordering begins."),
                    
                    // Keyword: "Order" -> Links to /admin/verify (or Orders page)
                    ("Stage 2: Order Monitoring", "Monitor incoming requests on the 'Verify Tasks' or Orders page."),
                    
                    // Generic task (No link)
                    ("Stage 3: Stock Allocation", "Physically set aside hardware in the warehouse."),
                    
                    // Keyword: "Assets" -> Links to /client/assets (or Admin view of assets)
                    ("Stage 4: Asset Registration", "Register serial numbers in the 'Assets' system."),

                    ("Stage 5: Final Dispatch", "Coordinate courier pickup and mark project as deployed.")
                },

                "Platform Engineer" => new()
                {
                    ("Stage 1: Infrastructure Audit", "Assess current on-premise server environment."),
                    ("Stage 2: Virtualization Setup", "Configure Hyper-V/VMware host environment."),
                    ("Stage 3: Server Provisioning", "Deploy Windows Server instances and configure AD."),
                    ("Stage 4: Backup Configuration", "Setup local and offsite backup routines."),
                    ("Stage 5: Disaster Recovery Test", "Simulate failure and verify recovery time objectives.")
                },
                "Network Security Engineer" => new()
                {
                    ("Stage 1: Network Mapping", "Document existing IP schema and topology."),
                    ("Stage 2: Firewall Configuration", "Configure VLANs and inbound/outbound rules."),
                    ("Stage 3: VPN Tunneling", "Setup secure Site-to-Site or Client VPNs."),
                    ("Stage 4: Penetration Testing", "Run vulnerability scan and patch critical risks."),
                    ("Stage 5: Security Sign-off", "Generate security report and obtain client approval.")
                },
                "Cloud Engineer" => new()
                {
                    ("Stage 1: Tenant Initialization", "Setup Azure/AWS tenant and billing alerts."),
                    ("Stage 2: Identity Sync", "Configure Azure AD Connect for user synchronization."),
                    ("Stage 3: Data Migration", "Execute lift-and-shift of file server data to cloud."),
                    ("Stage 4: Cutover", "Switch DNS records and go live."),
                    ("Stage 5: Post-Migration Support", "Monitor logs for 48 hours and resolve sync errors.")
                },
                "Modern Workplace Engineer" => new()
                {
                    ("Stage 1: M365 Licensing", "Assign Business Premium licenses to users."),
                    ("Stage 2: Email Migration", "Migrate Exchange mailboxes to Exchange Online."),
                    ("Stage 3: Endpoint Manager", "Enroll devices in Intune/Autopilot."),
                    ("Stage 4: Policy Deployment", "Push security policies (BitLocker, MFA)."),
                    ("Stage 5: User Training", "Conduct Teams/OneDrive training session.")
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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;
namespace ToroTrack.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // Creates the tables in the DB.
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<ClientAsset> ClientAssets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        // Table for tracking invites
        public DbSet<PendingInvite> PendingInvites { get; set; }
        public DbSet<AssetOrder> AssetOrders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
    }
}
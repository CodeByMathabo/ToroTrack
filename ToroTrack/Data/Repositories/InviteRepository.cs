using Microsoft.EntityFrameworkCore;
using ToroTrack.Data.Entities;

namespace ToroTrack.Data.Repositories
{
    public interface IInviteRepository
    {
        Task<List<PendingInvite>> GetAllInvitesAsync();
        Task<PendingInvite?> GetInviteByEmailAsync(string email);
        Task AddInviteAsync(PendingInvite invite);
        Task DeleteInviteAsync(int id);
    }

    public class InviteRepository : IInviteRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<InviteRepository> _logger;

        public InviteRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<InviteRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<PendingInvite>> GetAllInvitesAsync()
        {
            // Why: Create a short-lived context for this operation to prevent tracking issues in Blazor
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.PendingInvites.OrderByDescending(i => i.DateInvited).ToListAsync();
        }

        public async Task<PendingInvite?> GetInviteByEmailAsync(string email)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.PendingInvites.FirstOrDefaultAsync(i => i.Email == email);
        }

        public async Task AddInviteAsync(PendingInvite invite)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.PendingInvites.Add(invite);
                await context.SaveChangesAsync();
                _logger.LogInformation("Invite persisted for {Email}", invite.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB Error persisting invite for {Email}", invite.Email);
                throw; // Propagate to Service to handle
            }
        }

        public async Task DeleteInviteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var invite = await context.PendingInvites.FindAsync(id);
            if (invite != null)
            {
                context.PendingInvites.Remove(invite);
                await context.SaveChangesAsync();
            }
        }
    }
}
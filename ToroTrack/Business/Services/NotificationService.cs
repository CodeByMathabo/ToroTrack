using Microsoft.EntityFrameworkCore;
using ToroTrack.Data;
using ToroTrack.Data.Entities;

namespace ToroTrack.Business.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, string type = "Info", string? url = null);
        Task<List<Notification>> GetMyNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public NotificationService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SendNotificationAsync(string userId, string message, string type = "Info", string? url = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var notif = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                ActionUrl = url,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notif);
            await context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetMyNotificationsAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Limit to last 50
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var notif = await context.Notifications.FindAsync(notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
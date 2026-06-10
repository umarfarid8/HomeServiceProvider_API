using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific;

public class ChatThreadRepository : Repository<ChatThread>, IChatThreadRepository
{
    public ChatThreadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ChatThread>> GetThreadsWithDetailsForUserAsync(
        Guid userId, UserRole role)
    {
        // Build the base query with all navigation properties we need
        IQueryable<ChatThread> query = _dbSet
            .Include(t => t.Booking)
                .ThenInclude(b => b.CustomerProfile).ThenInclude(c => c.User)
            .Include(t => t.Booking)
                .ThenInclude(b => b.ProviderProfile).ThenInclude(p => p.User)
            .Include(t => t.Booking)
                .ThenInclude(b => b.ServiceCategory)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Sender);

        // Filter to only threads this user is part of
        query = role == UserRole.Customer
            ? query.Where(t => t.Booking.CustomerProfile.UserId == userId)
            : query.Where(t => t.Booking.ProviderProfile.UserId == userId);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatThread?> GetThreadWithMessagesAsync(Guid threadId)
        => await _dbSet
            .Include(t => t.Booking)
                .ThenInclude(b => b.CustomerProfile).ThenInclude(c => c.User)
            .Include(t => t.Booking)
                .ThenInclude(b => b.ProviderProfile).ThenInclude(p => p.User)
            .Include(t => t.Booking)
                .ThenInclude(b => b.ServiceCategory)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(t => t.Id == threadId);

    public async Task<int> GetUnreadCountForUserAsync(Guid userId, UserRole role)
    {
        // Single efficient DB query — joins through ChatThread → Booking → Profile
        // Counts messages sent by the OTHER party that this user hasn't read yet
        if (role == UserRole.Customer)
        {
            return await _context.Set<Message>()
                .CountAsync(m =>
                    !m.IsRead &&
                    m.SenderId != userId &&
                    m.ChatThread.Booking.CustomerProfile.UserId == userId);
        }

        return await _context.Set<Message>()
            .CountAsync(m =>
                !m.IsRead &&
                m.SenderId != userId &&
                m.ChatThread.Booking.ProviderProfile.UserId == userId);
    }
}
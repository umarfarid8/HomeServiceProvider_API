using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific;

public interface IChatThreadRepository : IRepository<ChatThread>
{
    // Used by the thread list view — loads booking + participants + all messages
    Task<IEnumerable<ChatThread>> GetThreadsWithDetailsForUserAsync(Guid userId, UserRole role);

    // Used when opening a single thread — loads messages in order
    Task<ChatThread?> GetThreadWithMessagesAsync(Guid threadId);

    // Used for the unread badge (notification count in the header)
    Task<int> GetUnreadCountForUserAsync(Guid userId, UserRole role);
}
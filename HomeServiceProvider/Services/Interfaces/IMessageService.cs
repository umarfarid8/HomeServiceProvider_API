using HomeServiceProvider.Dtos.Messaging;

namespace HomeServiceProvider.Services.Interfaces;

public interface IMessageService
{
    // Returns all chat threads for the logged-in user (inbox/sidebar list)
    Task<List<ChatThreadSummaryDto>> GetMyThreadsAsync(Guid userId);

    // Opens a thread, returns all messages, and marks incoming messages as read
    Task<ChatThreadDto> GetThreadMessagesAsync(Guid threadId, Guid userId);

    // Sends a new message in the thread
    Task<MessageDto> SendMessageAsync(Guid threadId, Guid userId, SendMessageDto dto);

    // Returns total unread count across all threads (for notification bell/badge)
    Task<int> GetUnreadCountAsync(Guid userId);
}
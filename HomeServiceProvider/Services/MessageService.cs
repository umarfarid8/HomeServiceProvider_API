using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Messaging;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _uow;

    public MessageService(IUnitOfWork uow) => _uow = uow;

    // ─── Get All Threads (Inbox) ──────────────────────────────────────────────

    public async Task<List<ChatThreadSummaryDto>> GetMyThreadsAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var threads = await _uow.ChatThreads
            .GetThreadsWithDetailsForUserAsync(userId, user.Role);

        return threads.Select(t =>
        {
            // Determine who the "other party" is based on the requesting user's role
            bool iAmCustomer = t.Booking.CustomerProfile.UserId == userId;
            string otherName = iAmCustomer
                ? t.Booking.ProviderProfile.User.FullName
                : t.Booking.CustomerProfile.User.FullName;

            var lastMsg = t.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            int unread = t.Messages.Count(m => m.SenderId != userId && !m.IsRead);

            return new ChatThreadSummaryDto
            {
                ThreadId = t.Id,
                BookingId = t.BookingId,
                BookingStatus = t.Booking.Status.ToString(),
                OtherPartyName = otherName,
                ServiceCategory = t.Booking.ServiceCategory.Name,
                ScheduledDate = t.Booking.ScheduledDate.ToString("yyyy-MM-dd"),
                LastMessageContent = lastMsg?.Content,
                LastMessageAt = lastMsg?.CreatedAt,
                UnreadCount = unread
            };
        })
        .OrderByDescending(t => t.LastMessageAt ?? DateTime.MinValue)
        .ToList();
    }

    // ─── Open a Thread (Read Messages) ───────────────────────────────────────

    public async Task<ChatThreadDto> GetThreadMessagesAsync(Guid threadId, Guid userId)
    {
        var thread = await _uow.ChatThreads.GetThreadWithMessagesAsync(threadId)
            ?? throw new KeyNotFoundException("Chat thread not found.");

        // Security check — only the customer or provider on this booking can read it
        VerifyParticipant(thread, userId);

        // Auto-mark messages from the other party as read
        // EF Core is already tracking these entities (loaded via Include)
        // so we just update the property and call SaveChanges — no .Update() needed
        var unread = thread.Messages
            .Where(m => m.SenderId != userId && !m.IsRead)
            .ToList();

        if (unread.Any())
        {
            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }

        bool iAmCustomer = thread.Booking.CustomerProfile.UserId == userId;
        string otherName = iAmCustomer
            ? thread.Booking.ProviderProfile.User.FullName
            : thread.Booking.CustomerProfile.User.FullName;

        return new ChatThreadDto
        {
            ThreadId = thread.Id,
            BookingId = thread.BookingId,
            BookingStatus = thread.Booking.Status.ToString(),
            OtherPartyName = otherName,
            ServiceCategory = thread.Booking.ServiceCategory.Name,
            ProblemDescription = thread.Booking.ProblemDescription,
            ScheduledDate = thread.Booking.ScheduledDate.ToString("yyyy-MM-dd"),
            Messages = thread.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapToMessageDto(m, userId))
                .ToList()
        };
    }

    // ─── Send a Message ───────────────────────────────────────────────────────

    public async Task<MessageDto> SendMessageAsync(
        Guid threadId, Guid userId, SendMessageDto dto)
    {
        var thread = await _uow.ChatThreads.GetThreadWithMessagesAsync(threadId)
            ?? throw new KeyNotFoundException("Chat thread not found.");

        VerifyParticipant(thread, userId);

        // Do not allow messaging on a cancelled booking
        if (thread.Booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException(
                "Messaging is disabled for cancelled bookings.");

        // Load sender info for the response DTO (we already know the userId)
        var sender = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var message = new Message
        {
            ChatThreadId = threadId,
            SenderId = userId,
            Content = dto.Content.Trim(),
            IsRead = false
        };

        await _uow.Messages.AddAsync(message);
        await _uow.SaveChangesAsync();

        // Build the response without reloading from DB
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SenderName = sender.FullName,
            IsMine = true,             // sender just sent it — always theirs
            IsRead = false,
            SentAt = message.CreatedAt
        };
    }

    // ─── Unread Count (Notification Badge) ───────────────────────────────────

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return await _uow.ChatThreads.GetUnreadCountForUserAsync(userId, user.Role);
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private static void VerifyParticipant(ChatThread thread, Guid userId)
    {
        bool isCustomer = thread.Booking.CustomerProfile.UserId == userId;
        bool isProvider = thread.Booking.ProviderProfile.UserId == userId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException(
                "You don't have access to this chat thread.");
    }

    private static MessageDto MapToMessageDto(Message message, Guid requestingUserId)
        => new()
        {
            Id = message.Id,
            Content = message.Content,
            SenderName = message.Sender.FullName,
            IsMine = message.SenderId == requestingUserId,
            IsRead = message.IsRead,
            SentAt = message.CreatedAt
        };
}
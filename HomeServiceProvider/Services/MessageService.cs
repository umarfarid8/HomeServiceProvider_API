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
            bool iAmCustomer = t.Booking?.CustomerProfile?.UserId == userId;
            string otherName = iAmCustomer
                ? t.Booking?.ProviderProfile?.User?.FullName ?? t.Booking?.ProviderProfile?.BusinessName ?? "Provider"
                : t.Booking?.CustomerProfile?.User?.FullName ?? "Customer";

            var lastMsg = t.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            int unread = t.Messages?.Count(m => m.SenderId != userId && !m.IsRead) ?? 0;

            return new ChatThreadSummaryDto
            {
                ThreadId = t.Id,
                BookingId = t.BookingId,
                BookingStatus = t.Booking?.Status.ToString() ?? "Pending",
                OtherPartyName = otherName,
                ServiceCategory = t.Booking?.ServiceCategory?.Name ?? "General",
                ScheduledDate = t.Booking?.ScheduledDate.ToString("yyyy-MM-dd") ?? string.Empty,
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

        VerifyParticipant(thread, userId);

        var unread = thread.Messages?
            .Where(m => m.SenderId != userId && !m.IsRead)
            .ToList() ?? new();

        if (unread.Any())
        {
            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }

        bool iAmCustomer = thread.Booking?.CustomerProfile?.UserId == userId;
        string otherName = iAmCustomer
            ? thread.Booking?.ProviderProfile?.User?.FullName ?? thread.Booking?.ProviderProfile?.BusinessName ?? "Provider"
            : thread.Booking?.CustomerProfile?.User?.FullName ?? "Customer";

        return new ChatThreadDto
        {
            ThreadId = thread.Id,
            BookingId = thread.BookingId,
            BookingStatus = thread.Booking?.Status.ToString() ?? "Pending",
            OtherPartyName = otherName,
            ServiceCategory = thread.Booking?.ServiceCategory?.Name ?? "General",
            ProblemDescription = thread.Booking?.ProblemDescription ?? string.Empty,
            ScheduledDate = thread.Booking?.ScheduledDate.ToString("yyyy-MM-dd") ?? string.Empty,
            Messages = thread.Messages?
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapToMessageDto(m, userId))
                .ToList() ?? new()
        };
    }

    // ─── Send a Message ───────────────────────────────────────────────────────

    public async Task<MessageDto> SendMessageAsync(
        Guid threadId, Guid userId, SendMessageDto dto)
    {
        var thread = await _uow.ChatThreads.GetThreadWithMessagesAsync(threadId)
            ?? throw new KeyNotFoundException("Chat thread not found.");

        VerifyParticipant(thread, userId);

        if (thread.Booking?.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Messaging is disabled for cancelled bookings.");

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

        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SenderName = sender.FullName,
            IsMine = true,
            IsRead = false,
            SentAt = message.CreatedAt
        };
    }

    // ─── Unread Count ────────────────────────────────────────────────────────

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return await _uow.ChatThreads.GetUnreadCountForUserAsync(userId, user.Role);
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private static void VerifyParticipant(ChatThread thread, Guid userId)
    {
        if (thread.Booking == null) return;

        bool isCustomer = thread.Booking.CustomerProfile?.UserId == userId;
        bool isProvider = thread.Booking.ProviderProfile?.UserId == userId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException("You don't have access to this chat thread.");
    }

    private static MessageDto MapToMessageDto(Message message, Guid requestingUserId)
        => new()
        {
            Id = message.Id,
            Content = message.Content,
            SenderName = message.Sender?.FullName ?? "User",
            IsMine = message.SenderId == requestingUserId,
            IsRead = message.IsRead,
            SentAt = message.CreatedAt
        };
}
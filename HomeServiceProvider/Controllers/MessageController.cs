using HomeServiceProvider.Dtos.Messaging;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize(Roles = "Customer,Provider")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
        => _messageService = messageService;

    // GET api/messages/threads
    // Returns inbox — all threads for the logged-in user
    [HttpGet("threads")]
    public async Task<IActionResult> GetMyThreads()
    {
        var userId = User.GetUserId();
        var threads = await _messageService.GetMyThreadsAsync(userId);
        return Ok(threads);
    }

    // GET api/messages/threads/{threadId}
    // Opens a thread — returns all messages and marks incoming ones as read
    [HttpGet("threads/{threadId:guid}")]
    public async Task<IActionResult> GetThreadMessages(Guid threadId)
    {
        var userId = User.GetUserId();
        var thread = await _messageService.GetThreadMessagesAsync(threadId, userId);
        return Ok(thread);
    }

    // POST api/messages/threads/{threadId}
    // Sends a new message in an existing thread
    [HttpPost("threads/{threadId:guid}")]
    public async Task<IActionResult> SendMessage(
        Guid threadId, [FromBody] SendMessageDto dto)
    {
        var userId = User.GetUserId();
        var message = await _messageService.SendMessageAsync(threadId, userId, dto);
        return Ok(message);
    }

    // GET api/messages/unread-count
    // Returns a single number for the notification badge in the header
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(new { unreadCount = count });
    }
}
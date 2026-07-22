using HomeServiceProvider.Dtos.Chat;
using HomeServiceProvider.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public ChatbotController(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    // POST api/chatbot/message
    // No auth required — chatbot is available to all visitors
    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto dto)
    {
        if (dto.Messages.Count == 0)
            return BadRequest(new { message = "Please send at least one message." });

        // Load system prompt from DB
        var template = await _uow.PromptTemplates.FirstOrDefaultAsync(
            t => t.TemplateKey == "platform_chatbot" && t.IsActive);

        var systemPromptText = template?.Content
            ?? "You are a helpful assistant for Home Service Provider (HSP) platform.";

        var apiKey = _config["OpenAI:ApiKey"]!;
        var model = _config["OpenAI:Model"] ?? "gpt-4o-mini";

        try
        {
            var client = new OpenAI.OpenAIClient(apiKey);
            var chatClient = client.GetChatClient(model);

            // Build the message list for OpenAI
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPromptText)
            };

            foreach (var turn in dto.Messages.TakeLast(10))  // max 10 turns of history
            {
                if (turn.Role == "user")
                    messages.Add(new UserChatMessage(turn.Content));
                else if (turn.Role == "assistant")
                    messages.Add(new AssistantChatMessage(turn.Content));
            }

            var response = await chatClient.CompleteChatAsync(messages);
            var reply = response.Value.Content[0].Text;

            return Ok(new ChatResponseDto { Reply = reply });
        }
        catch
        {
            return Ok(new ChatResponseDto
            {
                Reply = "I'm having trouble connecting right now. " +
                        "Please try again in a moment or contact support at support@hsp.com."
            });
        }
    }
}
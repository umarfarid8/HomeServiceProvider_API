namespace HomeServiceProvider.Dtos.Chat;

public class ChatRequestDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public List<ChatTurnDto> Messages { get; set; } = new();
}

public class ChatTurnDto
{
    public string Role { get; set; } = string.Empty;    // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}

public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
}
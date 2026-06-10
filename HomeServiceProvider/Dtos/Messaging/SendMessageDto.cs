using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Messaging;

public class SendMessageDto
{
    [Required, MinLength(1), MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}
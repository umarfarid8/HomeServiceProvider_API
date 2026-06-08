using System.ComponentModel.DataAnnotations;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.Dtos.Provider;

public class AddVerificationDocumentDto
{
    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public string DocumentUrl { get; set; } = string.Empty;
}
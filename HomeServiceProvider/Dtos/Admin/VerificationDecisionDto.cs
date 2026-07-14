using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Admin;

public class VerificationDecisionDto
{
    // Admin must explicitly pass a rejection reason when rejecting
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
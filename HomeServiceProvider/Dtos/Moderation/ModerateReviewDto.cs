using System.ComponentModel.DataAnnotations;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.Dtos.Moderation;

public class ModerateReviewDto
{
    // Admin must pick Approved or Rejected — Pending is not a valid decision
    [Required]
    public ModerationStatus Decision { get; set; }

    public string? AdminNotes { get; set; }
}
using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class VerificationDocument : BaseEntity
    {
        public Guid ProviderProfileId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? AdminNotes { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}

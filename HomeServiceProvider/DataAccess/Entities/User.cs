using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public CustomerProfile? CustomerProfile { get; set; }
        public ProviderProfile? ProviderProfile { get; set; }
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    }
}

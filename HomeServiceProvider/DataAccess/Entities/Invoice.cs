using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid BookingId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;  // e.g. HSP-2025-00001
        public decimal SubTotal { get; set; }
        public decimal PlatformCommissionRate { get; set; } = 0.15m;
        public decimal PlatformCommissionAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCashCollected { get; set; } = false;
        public DateTime? CashCollectedAt { get; set; }
        public string? PdfUrl { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }
}

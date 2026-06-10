namespace HomeServiceProvider.Dtos.Invoice;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;  // e.g. HSP-2025-00001
    public Guid BookingId { get; set; }

    // Parties
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;

    // Job details
    public string ServiceCategory { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    // Financials
    public decimal SubTotal { get; set; }
    public decimal PlatformCommissionRate { get; set; }   // 0.15
    public decimal PlatformCommissionAmount { get; set; }
    public decimal TotalAmount { get; set; }              // what customer pays

    // COD Status
    public bool IsCashCollected { get; set; }
    public DateTime? CashCollectedAt { get; set; }

    // PDF
    public string? PdfUrl { get; set; }
    public DateTime GeneratedAt { get; set; }
}
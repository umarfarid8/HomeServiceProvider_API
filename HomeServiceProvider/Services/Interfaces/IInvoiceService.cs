using HomeServiceProvider.Dtos.Invoice;

namespace HomeServiceProvider.Services.Interfaces;

public interface IInvoiceService
{
    // Called automatically when booking status → Completed
    Task<InvoiceDto> GenerateInvoiceAsync(Guid bookingId);

    // Get invoice for a specific booking
    Task<InvoiceDto> GetInvoiceByBookingIdAsync(Guid bookingId, Guid userId);

    // Provider confirms they collected cash from the customer
    Task<InvoiceDto> ConfirmCashReceivedAsync(Guid invoiceId, Guid providerUserId);

    // Download the PDF file (returns raw bytes)
    Task<byte[]> DownloadInvoicePdfAsync(Guid invoiceId, Guid userId);
}
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Invoice;
using HomeServiceProvider.Helpers;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _uow;
    private readonly InvoicePdfGenerator _pdfGenerator;

    // PDF files are saved here so ASP.NET Core's static files middleware can serve them
    private readonly string _invoiceStoragePath = Path.Combine(
        Directory.GetCurrentDirectory(), "wwwroot", "invoices");

    public InvoiceService(IUnitOfWork uow, InvoicePdfGenerator pdfGenerator)
    {
        _uow = uow;
        _pdfGenerator = pdfGenerator;
        Directory.CreateDirectory(_invoiceStoragePath);
    }

    // ─── Generate Invoice ─────────────────────────────────────────────────────

    public async Task<InvoiceDto> GenerateInvoiceAsync(Guid bookingId)
    {
        // Guard: don't create duplicate invoices
        var existing = await _uow.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
        if (existing is not null)
            throw new InvalidOperationException("An invoice already exists for this booking.");

        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        // The amount the customer pays the provider in cash
        decimal subTotal = booking.FinalAmount ?? booking.EstimatedAmount;

        decimal commissionRate = 0.15m;
        decimal commissionAmount = Math.Round(subTotal * commissionRate, 2);

        // Invoice number: HSP-2025-00001 format
        int count = await _uow.Invoices.CountAsync();
        string invoiceNum = $"HSP-{DateTime.UtcNow.Year}-{(count + 1):D5}";

        var invoice = new Invoice
        {
            BookingId = bookingId,
            InvoiceNumber = invoiceNum,
            SubTotal = subTotal,
            PlatformCommissionRate = commissionRate,
            PlatformCommissionAmount = commissionAmount,
            TotalAmount = subTotal,       // Customer pays full amount in cash to provider
            IsCashCollected = false
        };

        await _uow.Invoices.AddAsync(invoice);
        await _uow.SaveChangesAsync();

        // Generate the PDF and save to disk
        invoice.PdfUrl = await SavePdfAsync(invoice, booking);
        _uow.Invoices.Update(invoice);
        await _uow.SaveChangesAsync();

        return MapToDto(invoice, booking);
    }

    // ─── Get Invoice ──────────────────────────────────────────────────────────

    public async Task<InvoiceDto> GetInvoiceByBookingIdAsync(Guid bookingId, Guid userId)
    {
        var invoice = await _uow.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId)
            ?? throw new KeyNotFoundException(
                "Invoice not found. The booking may not be completed yet.");

        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        VerifyParticipant(booking, userId);

        return MapToDto(invoice, booking);
    }

    // ─── Confirm Cash Received ────────────────────────────────────────────────

    public async Task<InvoiceDto> ConfirmCashReceivedAsync(Guid invoiceId, Guid providerUserId)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var booking = await _uow.Bookings.GetWithFullDetailsAsync(invoice.BookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        // Only the provider on this booking can confirm cash receipt
        if (booking.ProviderProfile.UserId != providerUserId)
            throw new UnauthorizedAccessException(
                "Only the assigned provider can confirm cash receipt.");

        if (invoice.IsCashCollected)
            throw new InvalidOperationException(
                "Cash has already been confirmed for this invoice.");

        invoice.IsCashCollected = true;
        invoice.CashCollectedAt = DateTime.UtcNow;

        _uow.Invoices.Update(invoice);
        await _uow.SaveChangesAsync();

        // Regenerate PDF to reflect the updated COD status
        invoice.PdfUrl = await SavePdfAsync(invoice, booking);
        _uow.Invoices.Update(invoice);
        await _uow.SaveChangesAsync();

        return MapToDto(invoice, booking);
    }

    // ─── Download PDF ─────────────────────────────────────────────────────────

    public async Task<byte[]> DownloadInvoicePdfAsync(Guid invoiceId, Guid userId)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var booking = await _uow.Bookings.GetWithFullDetailsAsync(invoice.BookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        VerifyParticipant(booking, userId);

        // Try to read from disk first
        if (invoice.PdfUrl is not null)
        {
            var savedPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot",
                invoice.PdfUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(savedPath))
                return await File.ReadAllBytesAsync(savedPath);
        }

        // File missing — regenerate on the fly (always works as a fallback)
        return _pdfGenerator.Generate(invoice, booking);
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private async Task<string> SavePdfAsync(Invoice invoice, Booking booking)
    {
        var pdfBytes = _pdfGenerator.Generate(invoice, booking);
        var fileName = $"{invoice.Id}.pdf";
        var filePath = Path.Combine(_invoiceStoragePath, fileName);

        await File.WriteAllBytesAsync(filePath, pdfBytes);

        return $"/invoices/{fileName}";
    }

    private static void VerifyParticipant(Booking booking, Guid userId)
    {
        bool isCustomer = booking.CustomerProfile.UserId == userId;
        bool isProvider = booking.ProviderProfile.UserId == userId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException(
                "You don't have access to this invoice.");
    }

    private static InvoiceDto MapToDto(Invoice invoice, Booking booking)
        => new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            BookingId = invoice.BookingId,
            CustomerName = booking.CustomerProfile.User.FullName,
            CustomerCity = booking.CustomerProfile.City,
            ProviderName = booking.ProviderProfile.User.FullName,
            BusinessName = booking.ProviderProfile.BusinessName,
            ServiceCategory = booking.ServiceCategory.Name,
            ScheduledDate = booking.ScheduledDate.ToString("yyyy-MM-dd"),
            StartTime = booking.ScheduledStartTime.ToString("HH:mm"),
            EndTime = booking.ScheduledEndTime.ToString("HH:mm"),
            SubTotal = invoice.SubTotal,
            PlatformCommissionRate = invoice.PlatformCommissionRate,
            PlatformCommissionAmount = invoice.PlatformCommissionAmount,
            TotalAmount = invoice.TotalAmount,
            IsCashCollected = invoice.IsCashCollected,
            CashCollectedAt = invoice.CashCollectedAt,
            PdfUrl = invoice.PdfUrl,
            GeneratedAt = invoice.CreatedAt
        };
}
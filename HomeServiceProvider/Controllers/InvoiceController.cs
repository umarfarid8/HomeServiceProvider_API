using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = "Customer,Provider")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
        => _invoiceService = invoiceService;

    // GET api/invoices/booking/{bookingId}
    // Get the invoice for a specific booking
    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetInvoice(Guid bookingId)
    {
        var userId = User.GetUserId();
        var invoice = await _invoiceService.GetInvoiceByBookingIdAsync(bookingId, userId);
        return Ok(invoice);
    }

    // POST api/invoices/{invoiceId}/confirm-cash
    // Provider confirms they received cash from the customer
    [HttpPost("{invoiceId:guid}/confirm-cash")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> ConfirmCash(Guid invoiceId)
    {
        var userId = User.GetUserId();
        var invoice = await _invoiceService.ConfirmCashReceivedAsync(invoiceId, userId);
        return Ok(invoice);
    }

    // GET api/invoices/{invoiceId}/download
    // Download the PDF invoice as a file
    [HttpGet("{invoiceId:guid}/download")]
    public async Task<IActionResult> DownloadPdf(Guid invoiceId)
    {
        var userId = User.GetUserId();
        var pdfBytes = await _invoiceService.DownloadInvoicePdfAsync(invoiceId, userId);

        // Returns the PDF as a downloadable file
        return File(pdfBytes, "application/pdf", $"invoice-{invoiceId}.pdf");
    }
}
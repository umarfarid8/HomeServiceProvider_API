using HomeServiceProvider.DataAccess.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HomeServiceProvider.Helpers;

public class InvoicePdfGenerator
{
    public byte[] Generate(Invoice invoice, Booking booking)
    {
        var duration = (booking.ScheduledEndTime.ToTimeSpan()
                      - booking.ScheduledStartTime.ToTimeSpan()).TotalHours;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily(Fonts.Arial));

                // ── Header ────────────────────────────────────────────────────
                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .PaddingBottom(10).Row(row =>
                    {
                        // Left: platform name
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Home Service Provider")
                               .SemiBold().FontSize(18).FontColor("#1a73e8");
                            col.Item().Text("Professional Home Services Platform")
                               .FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        // Right: invoice meta
                        row.ConstantItem(160).Column(col =>
                        {
                            col.Item().AlignRight().Text("INVOICE")
                               .SemiBold().FontSize(14).FontColor(Colors.Grey.Darken2);
                            col.Item().AlignRight()
                               .Text($"# {invoice.InvoiceNumber}").SemiBold();
                            col.Item().AlignRight()
                               .Text($"Date: {invoice.CreatedAt:dd MMM yyyy}")
                               .FontColor(Colors.Grey.Medium);
                        });
                    });

                // ── Content ────────────────────────────────────────────────────
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(16);

                    // Parties side-by-side
                    col.Item().Row(row =>
                    {
                        // Customer
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                           .Padding(10).Column(c =>
                           {
                               c.Item().Text("BILLED TO")
                                .FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                               c.Item().PaddingTop(4)
                                .Text(booking.CustomerProfile.User.FullName).SemiBold();
                               c.Item().Text(booking.CustomerProfile.City);
                               c.Item().Text(booking.CustomerProfile.User.Email)
                                .FontColor(Colors.Grey.Medium);
                           });

                        row.ConstantItem(20); // gap

                        // Provider
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                           .Padding(10).Column(c =>
                           {
                               c.Item().Text("SERVICE PROVIDER")
                                .FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                               c.Item().PaddingTop(4)
                                .Text(booking.ProviderProfile.BusinessName).SemiBold();
                               c.Item().Text(booking.ProviderProfile.User.FullName);
                               c.Item().Text(booking.ProviderProfile.City)
                                .FontColor(Colors.Grey.Medium);
                           });
                    });

                    // Problem description
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("JOB DESCRIPTION")
                         .FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                        c.Item().PaddingTop(4).Text(booking.ProblemDescription);
                    });

                    // Service details table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);   // Service
                            cols.RelativeColumn(2);   // Date & Time
                            cols.RelativeColumn(1);   // Hours
                            cols.RelativeColumn(1.5f);// Rate (PKR/hr)
                            cols.RelativeColumn(1.5f);// Amount
                        });

                        // Table header
                        static IContainer HeaderStyle(IContainer c)
                            => c.Background("#1a73e8").Padding(6);

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderStyle)
                             .Text("Service").FontColor(Colors.White).SemiBold();
                            h.Cell().Element(HeaderStyle)
                             .Text("Date & Time").FontColor(Colors.White).SemiBold();
                            h.Cell().Element(HeaderStyle)
                             .Text("Hours").FontColor(Colors.White).SemiBold();
                            h.Cell().Element(HeaderStyle)
                             .Text("Rate (PKR/hr)").FontColor(Colors.White).SemiBold();
                            h.Cell().Element(HeaderStyle)
                             .Text("Amount").FontColor(Colors.White).SemiBold();
                        });

                        // Table row
                        static IContainer RowStyle(IContainer c)
                            => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);

                        table.Cell().Element(RowStyle)
                             .Text(booking.ServiceCategory.Name);
                        table.Cell().Element(RowStyle)
                             .Text($"{booking.ScheduledDate:dd MMM yyyy}\n" +
                                   $"{booking.ScheduledStartTime:HH:mm} – " +
                                   $"{booking.ScheduledEndTime:HH:mm}");
                        table.Cell().Element(RowStyle)
                             .Text($"{duration:F1}");
                        table.Cell().Element(RowStyle)
                             .Text($"{invoice.SubTotal / (decimal)duration:N0}");
                        table.Cell().Element(RowStyle)
                             .Text($"PKR {invoice.SubTotal:N0}");
                    });

                    // Surcharge note
                    if (booking.IsEmergency || booking.IsOffHours)
                    {
                        var notes = new List<string>();
                        if (booking.IsEmergency) notes.Add("Emergency surcharge applied");
                        if (booking.IsOffHours) notes.Add("Off-hours surcharge applied");

                        col.Item().AlignRight()
                           .Text($"* {string.Join(" | ", notes)}")
                           .FontSize(8).Italic().FontColor(Colors.Orange.Medium);
                    }

                    // Financial summary
                    col.Item().AlignRight().Width(260).Column(summary =>
                    {
                        summary.Spacing(4);

                        static IContainer SummaryRow(IContainer c)
                            => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(4);

                        summary.Item().Element(SummaryRow).Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal (Customer Pays)");
                            r.ConstantItem(100).AlignRight()
                             .Text($"PKR {invoice.SubTotal:N0}");
                        });

                        summary.Item().Element(SummaryRow).Row(r =>
                        {
                            r.RelativeItem()
                             .Text($"Platform Commission ({invoice.PlatformCommissionRate:P0})")
                             .FontColor(Colors.Grey.Medium).FontSize(9);
                            r.ConstantItem(100).AlignRight()
                             .Text($"PKR {invoice.PlatformCommissionAmount:N0}")
                             .FontColor(Colors.Grey.Medium).FontSize(9);
                        });

                        summary.Item().Element(SummaryRow).Row(r =>
                        {
                            r.RelativeItem()
                             .Text("Net to Provider")
                             .FontColor(Colors.Grey.Medium).FontSize(9);
                            r.ConstantItem(100).AlignRight()
                             .Text($"PKR {(invoice.SubTotal - invoice.PlatformCommissionAmount):N0}")
                             .FontColor(Colors.Grey.Medium).FontSize(9);
                        });

                        // Total highlighted
                        summary.Item().Background("#1a73e8").Padding(8).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL PAYABLE").SemiBold()
                             .FontColor(Colors.White);
                            r.ConstantItem(100).AlignRight()
                             .Text($"PKR {invoice.TotalAmount:N0}")
                             .SemiBold().FontColor(Colors.White).FontSize(12);
                        });
                    });

                    // COD Status badge
                    col.Item().AlignRight().Width(260).Row(r =>
                    {
                        var (label, bgColor) = invoice.IsCashCollected
                            ? ("✓ CASH RECEIVED", Colors.Green.Lighten4)
                            : ("⏳ CASH PENDING", Colors.Orange.Lighten4);

                        r.RelativeItem().Background(bgColor).Padding(8)
                         .AlignCenter().Text(label).SemiBold().FontSize(10);
                    });
                });

                // ── Footer ────────────────────────────────────────────────────
                page.Footer().BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Payment Method: Cash on Delivery (COD)")
                           .FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight()
                           .Text($"Generated: {invoice.CreatedAt:dd MMM yyyy HH:mm} UTC")
                           .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
            });
        }).GeneratePdf();
    }
}
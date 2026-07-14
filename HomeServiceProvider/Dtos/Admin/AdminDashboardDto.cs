namespace HomeServiceProvider.Dtos.Admin;

public class AdminDashboardDto
{
    // Users
    public int TotalCustomers { get; set; }
    public int TotalProviders { get; set; }
    public int TotalAdmins { get; set; }

    // Action items — these drive the notification badges in the admin sidebar
    public int PendingProviderVerifications { get; set; }
    public int PendingModerationItems { get; set; }
    public int ActiveDisputes { get; set; }

    // Bookings
    public BookingStatsDto BookingStats { get; set; } = new();

    // Money
    public FinancialSnapshotDto FinancialSnapshot { get; set; } = new();
}

public class BookingStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Disputed { get; set; }
}

public class FinancialSnapshotDto
{
    public decimal TotalPlatformRevenue { get; set; }    // all-time commissions
    public decimal ThisMonthRevenue { get; set; }        // current calendar month
    public int TotalInvoicesGenerated { get; set; }
    public int CashConfirmedInvoices { get; set; }
}
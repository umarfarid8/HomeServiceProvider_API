namespace HomeServiceProvider.Dtos.Admin;

public class FinancialAnalyticsDto
{
    public decimal TotalTransactionVolume { get; set; }   // sum of all invoice SubTotals
    public decimal TotalPlatformRevenue { get; set; }     // sum of all commissions
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
    public List<TopProviderDto> TopProviders { get; set; } = new();
    public List<TopCategoryDto> TopCategories { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;     // "Jan 2025"
    public decimal Revenue { get; set; }
    public int BookingsCount { get; set; }
}

public class TopProviderDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int CompletedJobs { get; set; }
}

public class TopCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}
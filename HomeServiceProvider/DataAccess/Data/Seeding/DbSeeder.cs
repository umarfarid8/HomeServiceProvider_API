using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedPricingRulesAsync(context);
    }

    private static async Task SeedPricingRulesAsync(AppDbContext context)
    {
        if (await context.PricingRules.AnyAsync()) return;

        context.PricingRules.AddRange(
            new PricingRule
            {
                RuleName = "Emergency Surcharge",
                RuleType = PricingRuleType.Emergency,
                Multiplier = 1.50m,
                Description = "50% surcharge applied to emergency bookings",
                IsActive = true
            },
            new PricingRule
            {
                RuleName = "Off-Hours Surcharge",
                RuleType = PricingRuleType.OffHours,
                Multiplier = 1.25m,
                Description = "25% surcharge for bookings outside 09:00–18:00",
                IsActive = true
            },
            new PricingRule
            {
                RuleName = "Weekend Surcharge",
                RuleType = PricingRuleType.Weekend,
                Multiplier = 1.20m,
                Description = "20% surcharge on Saturday and Sunday bookings",
                IsActive = true
            }
        );

        await context.SaveChangesAsync();
    }
}
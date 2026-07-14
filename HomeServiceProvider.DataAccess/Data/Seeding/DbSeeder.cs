using BCrypt.Net;
using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedPricingRulesAsync(context);       // Phase 3 (unchanged)
        await SeedAdminUserAsync(context);          // ★ Robust Admin Seeding
        await SeedServiceCategoriesAsync(context);  // ★ Fixed: Added missing category seeding call!
    }

    private static async Task SeedAdminUserAsync(AppDbContext context)
    {
        // Search specifically for the target admin account by email
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hsp.com");

        if (admin == null)
        {
            // Create a brand new admin if it doesn't exist at all
            context.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@hsp.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@12345"),
                PhoneNumber = "03000000000",
                Role = UserRole.Admin,
                IsEmailVerified = true,
                IsActive = true
            });
        }
        else
        {
            // Self-Healing Check: Overwrite the existing record to ensure 
            // the password hash and parameters are 100% correct.
            admin.FullName = "System Admin";
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@12345");
            admin.Role = UserRole.Admin;
            admin.IsEmailVerified = true;
            admin.IsActive = true;
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPricingRulesAsync(AppDbContext context)
    {
        if (await context.PricingRules.AnyAsync()) return;

        context.PricingRules.AddRange(
            new PricingRule { RuleName = "Emergency Surcharge", RuleType = PricingRuleType.Emergency, Multiplier = 1.50m, IsActive = true },
            new PricingRule { RuleName = "Off-Hours Surcharge", RuleType = PricingRuleType.OffHours, Multiplier = 1.25m, IsActive = true },
            new PricingRule { RuleName = "Weekend Surcharge", RuleType = PricingRuleType.Weekend, Multiplier = 1.20m, IsActive = true }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedServiceCategoriesAsync(AppDbContext context)
    {
        if (await context.ServiceCategories.AnyAsync()) return;

        context.ServiceCategories.AddRange(
            new ServiceCategory { Name = "Plumbing", Description = "Pipes, drains, taps" },
            new ServiceCategory { Name = "Electrical", Description = "Wiring, fans, sockets" },
            new ServiceCategory { Name = "Carpentry", Description = "Doors, furniture, woodwork" },
            new ServiceCategory { Name = "AC & Cooling", Description = "Air conditioner repair and service" },
            new ServiceCategory { Name = "Painting", Description = "Interior and exterior painting" },
            new ServiceCategory { Name = "Cleaning", Description = "Deep cleaning, sofa, carpet" },
            new ServiceCategory { Name = "Generator", Description = "Generator repair and installation" }
        );

        await context.SaveChangesAsync();
    }
}
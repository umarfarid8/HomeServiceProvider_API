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
        await SeedPricingRulesAsync(context);
        await SeedAdminUserAsync(context);
        await SeedServiceCategoriesAsync(context);
        await SeedPromptTemplatesAsync(context); // ★ NEW: Prompt Templates Seeder
    }

    private static async Task SeedAdminUserAsync(AppDbContext context)
    {
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hsp.com");

        if (admin == null)
        {
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
            admin.FullName = "System Admin";
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
            new ServiceCategory { Name = "Plumbing", Description = "Pipes, drains, taps", IsActive = true },
            new ServiceCategory { Name = "Electrical", Description = "Wiring, fans, sockets", IsActive = true },
            new ServiceCategory { Name = "Carpentry", Description = "Doors, furniture, woodwork", IsActive = true },
            new ServiceCategory { Name = "AC & Cooling", Description = "Air conditioner repair and service", IsActive = true },
            new ServiceCategory { Name = "Painting", Description = "Interior and exterior painting", IsActive = true },
            new ServiceCategory { Name = "Cleaning", Description = "Deep cleaning, sofa, carpet", IsActive = true },
            new ServiceCategory { Name = "Generator", Description = "Generator repair and installation", IsActive = true }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedPromptTemplatesAsync(AppDbContext context)
    {
        if (await context.PromptTemplates.AnyAsync()) return;

        context.PromptTemplates.Add(new PromptTemplate
        {
            TemplateKey = "intent_classifier",
            DisplayName = "AI Intent Classifier",
            Description = "Classifies a customer's free-text query into a service category.",
            PlaceholderKeys = "CATEGORIES,QUERY",
            IsActive = true,
            Content = @"## ROLE
You are a strict intent classification engine for a home services marketplace in Pakistan.

## TASK
Read the customer's problem description and classify it into exactly ONE service category
from the list below. Return ONLY a JSON object — no explanation, no markdown.

## AVAILABLE CATEGORIES
{{CATEGORIES}}

## OUTPUT FORMAT
{
  ""category"": ""<exact category name from the list above, or 'Unknown'>"",
  ""confidence"": <decimal between 0.0 and 1.0>,
  ""reasoning"": ""<one sentence explaining your classification>""
}

## RULES
- If the query is clearly home-service related but no category fits, use the closest one
- If the query is completely unrelated (e.g. 'my cat is sad', 'I need a lawyer'),
  return 'Unknown' with confidence 0.0
- confidence must reflect your true certainty: 0.9+ very clear, 0.6–0.9 reasonable
- Never return a category not in the list above

## CUSTOMER QUERY
{{QUERY}}"
        });

        context.PromptTemplates.Add(new PromptTemplate
        {
            TemplateKey = "platform_chatbot",
            DisplayName = "Platform Assistant Chatbot",
            Description = "System prompt for the HSP platform assistant chatbot.",
            IsActive = true,
            Content = @"You are a friendly and helpful assistant for Home Service Provider (HSP),
a Pakistani home services marketplace that connects customers with verified professionals
such as plumbers, electricians, carpenters, AC technicians, painters, and cleaners.

You help users with:
- Understanding how the platform works
- Explaining how to book a service
- Answering questions about billing (Cash on Delivery, 15% platform commission)
- Helping with account or profile questions
- Describing what service categories are available
- Troubleshooting common issues (booking status, reviews, disputes)

Keep answers short, friendly, and in simple English. If the user writes in Urdu or
Roman Urdu, respond in the same language. Never make up information about specific
providers, pricing, or availability — direct users to search or contact support.
If asked about something completely unrelated to the platform, politely redirect."
        });

        await context.SaveChangesAsync();
    }
}
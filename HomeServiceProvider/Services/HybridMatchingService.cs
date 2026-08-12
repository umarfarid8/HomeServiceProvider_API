using System.Text.Json;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Matching;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using OpenAI.Chat;

namespace HomeServiceProvider.Services;

public class HybridMatchingService : IHybridMatchingService
{
    private const decimal ConfidenceThreshold = 0.60m;
    private const string CacheKeyPrefix = "intent_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    public HybridMatchingService(
        IUnitOfWork uow,
        IConfiguration config,
        IMemoryCache cache)
    {
        _uow = uow;
        _config = config;
        _cache = cache;
    }

    public async Task<HybridSearchResultDto> SearchAsync(
        Guid customerUserId, HybridSearchRequestDto dto)
    {
        // ── Step 1: Resolve customer city ─────────────────────────────────────
        var customerProfile = await _uow.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == customerUserId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        if (string.IsNullOrWhiteSpace(customerProfile.City))
            throw new InvalidOperationException(
                "Please update your city in your profile before searching.");

        // ── Step 2: Load all active service categories ─────────────────────────
        var allCategories = (await _uow.ServiceCategories
            .FindAsync(c => c.IsActive))
            .ToList();

        if (!allCategories.Any())
            throw new InvalidOperationException(
                "No service categories configured. Contact administrator.");

        // ── Step 3: Check cache before calling AI ──────────────────────────────
        var normalizedQuery = dto.Query.Trim().ToLower();
        var cacheKey = $"{CacheKeyPrefix}{normalizedQuery}";
        bool fromCache = false;
        ClassificationResult classification;

        if (_cache.TryGetValue(cacheKey, out ClassificationResult? cached) && cached is not null)
        {
            classification = cached;
            fromCache = true;
        }
        else
        {
            // ── Step 4: Phase 1 — AI Intent Classification ─────────────────────
            classification = await ClassifyIntentAsync(dto.Query, allCategories.Select(c => c.Name));

            if (classification.Confidence >= ConfidenceThreshold)
            {
                _cache.Set(cacheKey, classification, CacheDuration);
            }
        }

        // ── Step 5: Log search attempt (AWAITED to avoid DbContext concurrency issues)
        await LogSearchAsync(customerProfile.Id, dto.Query,
            classification.Category, classification.Confidence,
            classification.Confidence >= ConfidenceThreshold, fromCache);

        // ── Step 6: Confidence gate — halt if query is unclear ─────────────────
        if (classification.Confidence < ConfidenceThreshold ||
            classification.Category == "Unknown")
        {
            throw new InvalidOperationException(
                BuildLowConfidenceMessage(
                    classification.Confidence, allCategories.Select(c => c.Name)));
        }

        // ── Step 7: Find the matching ServiceCategory entity ───────────────────
        var matchedCategory = allCategories.FirstOrDefault(c =>
            string.Equals(c.Name, classification.Category,
                StringComparison.OrdinalIgnoreCase));

        if (matchedCategory is null)
            throw new InvalidOperationException(
                $"Classified category '{classification.Category}' not found in database.");

        // ── Step 8: Phase 2 — Database Query ───────────────────────────────────
        var providers = await GetSortedProvidersAsync(
            customerProfile.City, matchedCategory.Id);

        // ── Step 9: Update analytics log count (AWAITED) ───────────────────────
        await UpdateLogProviderCountAsync(customerProfile.Id, dto.Query, providers.Count);

        if (providers.Count == 0)
            throw new InvalidOperationException(
                $"No verified {matchedCategory.Name} providers found in {customerProfile.City}. " +
                "Try a nearby city or check back later.");

        // ── Step 10: Phase 3 — Split AI Top Match vs Remaining ─────────────────
        var topProvider = providers.First();
        var remainingProviders = providers.Skip(1).ToList();

        return new HybridSearchResultDto
        {
            ClassifiedCategory = matchedCategory.Name,
            ConfidenceScore = classification.Confidence,
            AiSuggestedProvider = MapToDto(topProvider),
            RemainingProviders = remainingProviders.Select(MapToDto).ToList(),
            TotalProvidersFound = providers.Count,
            ServedFromCache = fromCache
        };
    }

    // ─── Phase 1: AI Classification ───────────────────────────────────────────

    private async Task<ClassificationResult> ClassifyIntentAsync(
        string query, IEnumerable<string> categoryNames)
    {
        var template = await _uow.PromptTemplates.FirstOrDefaultAsync(
            t => t.TemplateKey == "intent_classifier" && t.IsActive)
            ?? throw new InvalidOperationException(
                "Intent classifier prompt template not found. Contact administrator.");

        var systemPrompt = template.Content
            .Replace("{{CATEGORIES}}", string.Join("\n", categoryNames.Select(c => $"- {c}")))
            .Replace("{{QUERY}}", query);

        var apiKey = _config["OpenAI:ApiKey"]!;
        var model = _config["OpenAI:Model"] ?? "llama-3.1-8b-instant";
        var baseUrl = _config["OpenAI:BaseUrl"] ?? "https://api.groq.com/openai/v1/";

        var clientOptions = new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(baseUrl)
        };

        var client = new OpenAI.OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            clientOptions
        );

        var chatClient = client.GetChatClient(model);

        try
        {
            var response = await chatClient.CompleteChatAsync(new UserChatMessage(systemPrompt));
            var raw = response.Value.Content[0].Text;
            var clean = raw.Replace("```json", "").Replace("```", "").Trim();

            var parsed = JsonSerializer.Deserialize<ClassificationJson>(
                clean, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ClassificationResult
            {
                Category = parsed?.Category ?? "Unknown",
                Confidence = parsed?.Confidence ?? 0m,
                Reasoning = parsed?.Reasoning ?? string.Empty
            };
        }
        catch
        {
            return new ClassificationResult
            {
                Category = "Unknown",
                Confidence = 0m,
                Reasoning = "Classification service temporarily unavailable."
            };
        }
    }

    public async Task<HybridSearchResultDto> ManualSearchAsync(
        Guid customerUserId, ManualSearchRequestDto dto)
    {
        var customerProfile = await _uow.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == customerUserId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        var city = !string.IsNullOrWhiteSpace(dto.City)
            ? dto.City.Trim()
            : customerProfile.City;

        if (string.IsNullOrWhiteSpace(city))
            throw new InvalidOperationException(
                "Please set your city in your profile or provide a city in the search.");

        var category = await _uow.ServiceCategories.GetByIdAsync(dto.ServiceCategoryId)
            ?? throw new KeyNotFoundException("Service category not found.");

        var providers = await GetSortedProvidersAsync(city, dto.ServiceCategoryId);

        if (!providers.Any())
            throw new InvalidOperationException(
                $"No verified {category.Name} providers found in {city}.");

        return new HybridSearchResultDto
        {
            ClassifiedCategory = category.Name,
            ConfidenceScore = 1.0m,
            AiSuggestedProvider = MapToDto(providers.First()),
            RemainingProviders = providers.Skip(1).Select(MapToDto).ToList(),
            TotalProvidersFound = providers.Count,
            ServedFromCache = false
        };
    }

    // ─── Phase 2: DB Query Sorting ────────────────────────────────────────────

    private async Task<List<DataAccess.Entities.ProviderProfile>> GetSortedProvidersAsync(
        string city, Guid serviceCategoryId)
    {
        var providers = (await _uow.ProviderProfiles
            .GetProvidersForAIMatchAsync(city, serviceCategoryId))
            .ToList();

        return providers
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.TotalJobsCompleted)
            .ToList();
    }

    // ─── Logging Helpers ──────────────────────────────────────────────────────

    private async Task LogSearchAsync(
        Guid customerProfileId, string query, string? category,
        decimal confidence, bool success, bool fromCache)
    {
        try
        {
            var log = new SearchAnalyticsLog
            {
                CustomerProfileId = customerProfileId,
                RawQuery = query.Trim(),
                ClassifiedCategory = category,
                ConfidenceScore = confidence,
                WasSuccessful = success,
                FailureReason = !success
                    ? $"Confidence {confidence:F2} below threshold {ConfidenceThreshold:F2}"
                    : null,
                ServedFromCache = fromCache
            };

            await _uow.SearchAnalyticsLogs.AddAsync(log);
            await _uow.SaveChangesAsync();
        }
        catch { /* ignore log failures */ }
    }

    private async Task UpdateLogProviderCountAsync(
        Guid customerProfileId, string query, int count)
    {
        try
        {
            var log = (await _uow.SearchAnalyticsLogs.FindAsync(l =>
                l.CustomerProfileId == customerProfileId &&
                l.RawQuery == query.Trim()))
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefault();

            if (log is not null)
            {
                log.ProvidersReturned = count;
                _uow.SearchAnalyticsLogs.Update(log);
                await _uow.SaveChangesAsync();
            }
        }
        catch { /* ignore log failures */ }
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static HybridProviderDto MapToDto(DataAccess.Entities.ProviderProfile p)
    {
        var maxExp = p.Services.Any()
            ? p.Services.Max(s => s.YearsOfExperience)
            : 0;

        return new HybridProviderDto
        {
            ProviderProfileId = p.Id,
            BusinessName = p.BusinessName,
            ProviderName = p.User.FullName,
            City = p.City,
            AverageRating = p.AverageRating,
            TotalJobsCompleted = p.TotalJobsCompleted,
            BaseHourlyRate = p.BaseHourlyRate,
            ProfileImageUrl = p.ProfileImageUrl,
            ServiceNames = p.Services.Select(s => s.ServiceCategory.Name).ToList(),
            ExperienceYears = maxExp
        };
    }

    // ─── Utility ──────────────────────────────────────────────────────────────

    private static string BuildLowConfidenceMessage(
        decimal confidence, IEnumerable<string> categories)
    {
        var examples = string.Join(", ", categories.Take(4));
        return $"We couldn't understand what service you need " +
               $"(confidence: {confidence:P0}). " +
               $"Try describing the problem differently, or pick a category like: {examples}.";
    }
}

internal record ClassificationResult
{
    public string Category { get; init; } = "Unknown";
    public decimal Confidence { get; init; } = 0m;
    public string Reasoning { get; init; } = string.Empty;
}

internal record ClassificationJson
{
    public string Category { get; init; } = "Unknown";
    public decimal Confidence { get; init; } = 0m;
    public string Reasoning { get; init; } = string.Empty;
}
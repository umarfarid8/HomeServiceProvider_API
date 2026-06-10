using System.Text;
using System.Text.Json;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Matching;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;
using OpenAI.Chat;

namespace HomeServiceProvider.Services;

public class MatchingService : IMatchingService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public MatchingService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    public async Task<MatchResultDto> FindBestProvidersAsync(
        Guid customerUserId, SubmitMatchRequestDto dto)
    {
        // ── Step 1: Get customer's city ───────────────────────────────────────
        var customerProfile = await _uow.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == customerUserId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        if (string.IsNullOrWhiteSpace(customerProfile.City))
            throw new InvalidOperationException(
                "Please update your city in your profile before using the matching service.");

        // ── Step 2: Load candidate providers from DB ──────────────────────────
        var candidates = await _uow.ProviderProfiles
            .GetProvidersForAIMatchAsync(customerProfile.City, dto.ServiceCategoryId);

        if (!candidates.Any())
            throw new InvalidOperationException(
                $"No verified providers found in {customerProfile.City}. " +
                "Try a different city or service category.");

        // ── Step 3: Enrich each provider with their recent reviews ────────────
        // Build a list of (provider, reviews) pairs
        var enrichedProviders = new List<(ProviderProfile Profile, IEnumerable<Review> Reviews)>();

        foreach (var provider in candidates)
        {
            var reviews = await _uow.Reviews
                .GetRecentReviewsForProviderAsync(provider.UserId, count: 10);
            enrichedProviders.Add((provider, reviews));
        }

        // ── Step 4: Save the MatchRequest to DB ───────────────────────────────
        var matchRequest = new MatchRequest
        {
            CustomerProfileId = customerProfile.Id,
            ProblemDescription = dto.ProblemDescription.Trim(),
            ServiceCategoryId = dto.ServiceCategoryId,
            City = customerProfile.City
        };
        await _uow.MatchRequests.AddAsync(matchRequest);
        await _uow.SaveChangesAsync();

        // ── Step 5: Build the prompt for OpenAI ───────────────────────────────
        var prompt = BuildPrompt(dto.ProblemDescription, enrichedProviders);

        // ── Step 6: Call OpenAI ───────────────────────────────────────────────
        string apiKey = _config["OpenAI:ApiKey"]!;
        string model = _config["OpenAI:Model"] ?? "gpt-4o-mini";

        var rankedResults = await CallOpenAIAsync(apiKey, model, prompt, matchRequest.Id);

        // ── Step 7: Save results to DB ────────────────────────────────────────
        foreach (var result in rankedResults)
        {
            await _uow.MatchResults.AddAsync(result);
        }
        await _uow.SaveChangesAsync();

        // ── Step 8: Build response ────────────────────────────────────────────
        var ranked = rankedResults
            .OrderBy(r => r.Rank)
            .Select(r =>
            {
                var provider = enrichedProviders
                    .First(e => e.Profile.Id == r.ProviderProfileId).Profile;

                return new ProviderMatchDto
                {
                    ProviderProfileId = provider.Id,
                    BusinessName = provider.BusinessName,
                    ProviderName = provider.User.FullName,
                    City = provider.City,
                    AverageRating = provider.AverageRating,
                    TotalJobsCompleted = provider.TotalJobsCompleted,
                    BaseHourlyRate = provider.BaseHourlyRate,
                    ProfileImageUrl = provider.ProfileImageUrl,
                    Rank = r.Rank,
                    AIScore = r.AIScore,
                    ExplanationTag = r.ExplanationTag
                };
            }).ToList();

        return new MatchResultDto
        {
            MatchRequestId = matchRequest.Id,
            ProblemDescription = dto.ProblemDescription,
            TotalProvidersEvaluated = enrichedProviders.Count,
            RankedProviders = ranked
        };
    }

    // ─── Private: Build the text prompt sent to OpenAI ────────────────────────

    private static string BuildPrompt(
        string customerProblem,
        List<(ProviderProfile Profile, IEnumerable<Review> Reviews)> providers)
    {
        var sb = new StringBuilder();

        sb.AppendLine("CUSTOMER PROBLEM:");
        sb.AppendLine(customerProblem);
        sb.AppendLine();
        sb.AppendLine("AVAILABLE PROVIDERS:");
        sb.AppendLine();

        foreach (var (profile, reviews) in providers)
        {
            sb.AppendLine($"Provider ID: {profile.Id}");
            sb.AppendLine($"Business: {profile.BusinessName}");
            sb.AppendLine($"Bio: {profile.Bio}");
            sb.AppendLine($"Services: {string.Join(", ", profile.Services.Select(s => s.ServiceCategory.Name))}");
            sb.AppendLine($"Rating: {profile.AverageRating}/5 ({profile.TotalJobsCompleted} jobs completed)");

            var reviewTexts = reviews.Select(r => $"- \"{r.Comment}\"").ToList();
            if (reviewTexts.Any())
            {
                sb.AppendLine("Recent customer reviews:");
                reviewTexts.ForEach(r => sb.AppendLine(r));
            }
            else
            {
                sb.AppendLine("Recent customer reviews: No reviews yet.");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ─── Private: Call OpenAI and parse the JSON response ─────────────────────

    private async Task<List<MatchResult>> CallOpenAIAsync(
        string apiKey, string model, string prompt, Guid matchRequestId)
    {
        // System instruction — tells OpenAI exactly what to do and how to respond
        const string systemInstruction = """
            You are an AI matching engine for a home services platform in Pakistan.
            
            Your job: read the customer's problem and the list of providers,
            then rank ALL providers from best to worst match.
            
            Evaluation criteria (in order of importance):
            1. Relevance — does their bio/services match the customer's problem?
            2. Review quality — do their reviews mention solving similar problems?
            3. Rating and experience — higher rating and more jobs = more reliable
            
            You MUST respond with valid JSON only. No explanation text outside the JSON.
            The JSON must be an array with one object per provider, like this:
            
            [
              {
                "providerId": "the-exact-guid-from-the-input",
                "rank": 1,
                "score": 0.95,
                "explanation": "Short human-readable tag, max 10 words"
              }
            ]
            
            Rules:
            - Include ALL providers from the input, ranked 1 to N
            - score must be between 0.0 and 1.0
            - explanation must be short and specific (e.g. "Top-rated specialist for drain issues")
            - Do not invent providers. Use only the Provider IDs given to you.
            """;

        var openAIClient = new OpenAI.OpenAIClient(apiKey);
        var chatClient = openAIClient.GetChatClient(model);

        int promptTokens = 0;
        int completionTokens = 0;
        bool success = false;
        string? errorMessage = null;

        try
        {
            var chatResponse = await chatClient.CompleteChatAsync(
                new SystemChatMessage(systemInstruction),
                new UserChatMessage(prompt));

            promptTokens = chatResponse.Value.Usage.InputTokenCount;
            completionTokens = chatResponse.Value.Usage.OutputTokenCount;
            success = true;

            // Parse the JSON response from OpenAI
            var responseText = chatResponse.Value.Content[0].Text;
            var aiItems = ParseAIResponse(responseText);

            // Map to MatchResult entities
            var results = aiItems.Select((item, index) => new MatchResult
            {
                MatchRequestId = matchRequestId,
                ProviderProfileId = item.ProviderId,
                Rank = item.Rank,
                AIScore = item.Score,
                ExplanationTag = item.Explanation
            }).ToList();

            // Log token usage for cost tracking
            await LogAICallAsync(matchRequestId, model,
                promptTokens, completionTokens, success, null);

            return results;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            await LogAICallAsync(matchRequestId, model,
                promptTokens, completionTokens, false, errorMessage);
            throw new InvalidOperationException(
                "AI matching service is temporarily unavailable. Please try again.");
        }
    }

    // ─── Private: Parse OpenAI JSON output ────────────────────────────────────

    private static List<AIResponseItem> ParseAIResponse(string responseText)
    {
        // Strip any markdown code fences OpenAI sometimes adds
        // e.g. ```json [...] ``` → [...]
        var clean = responseText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            var items = JsonSerializer.Deserialize<List<AIResponseItem>>(
                clean,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return items ?? new List<AIResponseItem>();
        }
        catch
        {
            // If OpenAI response is malformed, return empty rather than crash
            return new List<AIResponseItem>();
        }
    }

    // ─── Private: Log the AI call for cost tracking ────────────────────────────

    private async Task LogAICallAsync(
        Guid matchRequestId, string model,
        int promptTokens, int completionTokens,
        bool success, string? errorMessage)
    {
        // Approximate cost for gpt-4o-mini (update rates if needed)
        const decimal inputCostPer1k = 0.000150m;
        const decimal outputCostPer1k = 0.000600m;

        decimal estimatedCost =
            (promptTokens / 1000m * inputCostPer1k) +
            (completionTokens / 1000m * outputCostPer1k);

        var log = new AIEvaluationLog
        {
            MatchRequestId = matchRequestId,
            ModelUsed = model,
            PromptTokensUsed = promptTokens,
            CompletionTokensUsed = completionTokens,
            TotalTokensUsed = promptTokens + completionTokens,
            EstimatedCostUsd = Math.Round(estimatedCost, 6),
            IsSuccessful = success,
            ErrorMessage = errorMessage
        };

        await _uow.AIEvaluationLogs.AddAsync(log);
        // Note: caller is responsible for SaveChangesAsync
    }
}

// ─── Internal model for deserializing OpenAI JSON response ────────────────────

internal class AIResponseItem
{
    public Guid ProviderId { get; set; }
    public int Rank { get; set; }
    public decimal Score { get; set; }
    public string Explanation { get; set; } = string.Empty;
}
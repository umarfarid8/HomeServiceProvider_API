using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class AIEvaluationLog : BaseEntity
    {
        public Guid MatchRequestId { get; set; }
        public string ModelUsed { get; set; } = string.Empty;
        public int PromptTokensUsed { get; set; }
        public int CompletionTokensUsed { get; set; }
        public int TotalTokensUsed { get; set; }
        public decimal EstimatedCostUsd { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation
        public MatchRequest MatchRequest { get; set; } = null!;
    }
}

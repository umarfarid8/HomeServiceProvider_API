using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class PricingRule : BaseEntity
    {
        public string RuleName { get; set; } = string.Empty;
        public PricingRuleType RuleType { get; set; }
        public decimal Multiplier { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}

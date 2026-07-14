using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations;

public class AIEvaluationLogConfiguration : IEntityTypeConfiguration<AIEvaluationLog>
{
    public void Configure(EntityTypeBuilder<AIEvaluationLog> builder)
    {
        builder.Property(e => e.EstimatedCostUsd).HasPrecision(18, 6);
    }
}
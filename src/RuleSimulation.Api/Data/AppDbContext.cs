using Microsoft.EntityFrameworkCore;
using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RequestRow> Requests => Set<RequestRow>();
    public DbSet<RuleSet> RuleSets => Set<RuleSet>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleCondition> RuleConditions => Set<RuleCondition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var request = modelBuilder.Entity<RequestRow>();
        request.ToTable("Request");
        request.HasKey(x => x.RequestId);
        request.Property(x => x.RequestId).HasColumnName("request_id").HasMaxLength(50);
        request.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
        request.Property(x => x.Channel).HasColumnName("channel").HasConversion<string>();
        request.Property(x => x.Region).HasColumnName("region").HasConversion<string>();
        request.Property(x => x.AccountTier).HasColumnName("account_tier").HasConversion<string>();
        request.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        request.Property(x => x.ItemClass).HasColumnName("item_class").HasConversion<string>();
        request.Property(x => x.Quantity).HasColumnName("quantity");
        request.Property(x => x.DeclaredValue).HasColumnName("declared_value").HasPrecision(18, 2);
        request.Property(x => x.ItemAgeDays).HasColumnName("item_age_days");
        request.Property(x => x.HasDocumentation).HasColumnName("has_documentation");
        request.Property(x => x.PriorRequests90d).HasColumnName("prior_requests_90d");
        request.Property(x => x.FlaggedDuplicate).HasColumnName("flagged_duplicate");
        request.Property(x => x.RecordedOutcome).HasColumnName("recorded_outcome").HasConversion<string>();

        var set = modelBuilder.Entity<RuleSet>();
        set.ToTable("RuleSet");
        set.HasKey(x => x.Id);
        set.Property(x => x.Name).HasMaxLength(200).IsRequired();
        set.HasMany(x => x.Rules).WithOne(x => x.RuleSet).HasForeignKey(x => x.RuleSetId).OnDelete(DeleteBehavior.Cascade);

        var rule = modelBuilder.Entity<Rule>();
        rule.ToTable("Rule");
        rule.HasKey(x => x.Id);
        rule.Property(x => x.Name).HasMaxLength(200).IsRequired();
        rule.Property(x => x.ConditionJoin).HasConversion<string>().HasMaxLength(3);
        rule.Property(x => x.Action).HasConversion<string>().HasMaxLength(30);
        rule.HasIndex(x => new { x.RuleSetId, x.Enabled, x.Priority, x.DisplayOrder, x.Id });
        rule.HasMany(x => x.Conditions).WithOne(x => x.Rule).HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.Cascade);

        var condition = modelBuilder.Entity<RuleCondition>();
        condition.ToTable("RuleCondition");
        condition.HasKey(x => x.Id);
        condition.Property(x => x.Field).HasMaxLength(50);
        condition.Property(x => x.Operator).HasConversion<string>().HasMaxLength(30);
        condition.Property(x => x.Value).HasMaxLength(4000);
        condition.Property(x => x.ValuesJson).HasMaxLength(4000);
        condition.HasIndex(x => new { x.RuleId, x.Sequence }).IsUnique();
    }
}

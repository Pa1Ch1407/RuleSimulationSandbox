using Microsoft.EntityFrameworkCore;
using RuleSimulation.Api.Contracts;
using RuleSimulation.Api.Data;
using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Services;

public sealed class RuleSetService(AppDbContext db)
{
    public async Task<IReadOnlyList<RuleSetDto>> ListAsync(CancellationToken ct)
    {
        var sets = await db.RuleSets.Include(x => x.Rules).ThenInclude(x => x.Conditions).AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
        return sets.Select(Map).ToList();
    }

    public async Task<RuleSetDto?> GetAsync(long id, CancellationToken ct)
    {
        var set = await db.RuleSets.Include(x => x.Rules).ThenInclude(x => x.Conditions).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return set is null ? null : Map(set);
    }

    public async Task<RuleSetDto> CreateAsync(CreateRuleSetRequest input, CancellationToken ct)
    {
        Validate(input.Name, input.Rules);
        var now = DateTime.UtcNow;
        var set = new RuleSet { Name = input.Name.Trim(), Enabled = input.Enabled, CreatedAtUtc = now, UpdatedAtUtc = now };
        set.Rules = input.Rules.Select(ToEntity).ToList();
        db.RuleSets.Add(set);
        await db.SaveChangesAsync(ct);
        return Map(set);
    }

    public async Task<RuleSetDto?> UpdateAsync(long id, CreateRuleSetRequest input, CancellationToken ct)
    {
        Validate(input.Name, input.Rules);
        var set = await db.RuleSets.Include(x => x.Rules).ThenInclude(x => x.Conditions).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (set is null) return null;

        set.Name = input.Name.Trim();
        set.Enabled = input.Enabled;
        set.UpdatedAtUtc = DateTime.UtcNow;
        db.Rules.RemoveRange(set.Rules);
        set.Rules = input.Rules.Select(ToEntity).ToList();
        await db.SaveChangesAsync(ct);
        return Map(set);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var set = await db.RuleSets.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (set is null) return false;
        db.RuleSets.Remove(set);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static Rule ToEntity(CreateRuleRequest x) => new()
    {
        Name = x.Name.Trim(), Priority = x.Priority, Enabled = x.Enabled, ConditionJoin = x.ConditionJoin,
        Action = x.Action, DisplayOrder = x.DisplayOrder,
        Conditions = x.Conditions.Select((c, i) => new RuleCondition
        {
            Sequence = i + 1, Field = c.Field.Trim().ToLowerInvariant(), Operator = c.Operator,
            Value = c.Value, ValuesJson = c.Values is null ? null : System.Text.Json.JsonSerializer.Serialize(c.Values)
        }).ToList()
    };

    private static void Validate(string name, IReadOnlyList<CreateRuleRequest> rules)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Rule set name is required.");
        if (rules.Count == 0) throw new ArgumentException("At least one rule is required.");
        if (rules.Select(x => x.DisplayOrder).Distinct().Count() != rules.Count) throw new ArgumentException("DisplayOrder must be unique within a rule set.");
        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Name)) throw new ArgumentException("Rule name is required.");
            if (rule.Conditions.Count == 0) throw new ArgumentException($"Rule '{rule.Name}' must contain at least one condition.");
        }
    }

    public static RuleSetDto Map(RuleSet x) => new(x.Id, x.Name, x.Enabled, x.CreatedAtUtc, x.UpdatedAtUtc,
        x.Rules.OrderBy(r => r.DisplayOrder).Select(r => new RuleDto(r.Id, r.Name, r.Priority, r.Enabled, r.ConditionJoin, r.Action, r.DisplayOrder,
            r.Conditions.OrderBy(c => c.Sequence).Select(c => new ConditionDto(c.Field, c.Operator, c.Value,
                c.ValuesJson is null ? null : System.Text.Json.JsonSerializer.Deserialize<string[]>(c.ValuesJson))).ToList())).ToList());
}

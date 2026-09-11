namespace RuleSimulation.Api.Domain;

public sealed class RuleSet
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<Rule> Rules { get; set; } = [];
}

public sealed class Rule
{
    public long Id { get; set; }
    public long RuleSetId { get; set; }
    public required string Name { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; } = true;
    public ConditionJoin ConditionJoin { get; set; }
    public RuleAction Action { get; set; }
    public int DisplayOrder { get; set; }
    public RuleSet? RuleSet { get; set; }
    public List<RuleCondition> Conditions { get; set; } = [];
}

public sealed class RuleCondition
{
    public long Id { get; set; }
    public long RuleId { get; set; }
    public int Sequence { get; set; }
    public required string Field { get; set; }
    public ConditionOperator Operator { get; set; }
    public string? Value { get; set; }
    public string? ValuesJson { get; set; }
    public Rule? Rule { get; set; }
}

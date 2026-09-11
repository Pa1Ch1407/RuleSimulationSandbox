using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Contracts;

public sealed record ConditionDto(string Field, ConditionOperator Operator, string? Value, IReadOnlyList<string>? Values);
public sealed record RuleDto(long Id, string Name, int Priority, bool Enabled, ConditionJoin ConditionJoin, RuleAction Action, int DisplayOrder, IReadOnlyList<ConditionDto> Conditions);
public sealed record RuleSetDto(long Id, string Name, bool Enabled, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, IReadOnlyList<RuleDto> Rules);
public sealed record CreateRuleSetRequest(string Name, bool Enabled, IReadOnlyList<CreateRuleRequest> Rules);
public sealed record CreateRuleRequest(string Name, int Priority, bool Enabled, ConditionJoin ConditionJoin, RuleAction Action, int DisplayOrder, IReadOnlyList<ConditionDto> Conditions);

using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Rules;

public sealed record ConditionInput(string Field, ConditionOperator Operator, string? Value, IReadOnlyList<string>? Values);
public sealed record RuleInput(long Id, string Name, int Priority, bool Enabled, ConditionJoin ConditionJoin, RuleAction Action, int DisplayOrder, IReadOnlyList<ConditionInput> Conditions);
public sealed record RuleSetInput(string Name, bool Enabled, IReadOnlyList<RuleInput> Rules);

public sealed record EvaluationResult(bool Matched, Rule? WinningRule, RuleAction? Action);

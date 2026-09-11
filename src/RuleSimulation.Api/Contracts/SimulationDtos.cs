using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Contracts;

public sealed record SimulationRequest(long? RuleSetId, DraftRuleSetDto? Draft, int Page = 1, int PageSize = 50);
public sealed record DraftRuleSetDto(string Name, bool Enabled, IReadOnlyList<DraftRuleDto> Rules);
public sealed record DraftRuleDto(string Name, int Priority, bool Enabled, ConditionJoin ConditionJoin, RuleAction Action, int DisplayOrder, IReadOnlyList<ConditionDto> Conditions);

public sealed record SimulationResponse(
    int DatasetCount,
    IReadOnlyDictionary<string, int> Summary,
    BaselineComparisonDto BaselineComparison,
    IReadOnlyList<RuleAttributionDto> RuleAttribution,
    PagedChangedDecisionDto ChangedDecisions);

public sealed record BaselineComparisonDto(int AgreementCount, decimal AgreementRate, int ChangedDecisionCount);
public sealed record RuleAttributionDto(long RuleId, string RuleName, int FiredCount);
public sealed record ChangedDecisionDto(string RequestId, string ProjectedAction, string? WinningRule, string RecordedOutcome);
public sealed record PagedChangedDecisionDto(int Page, int PageSize, int Total, IReadOnlyList<ChangedDecisionDto> Items);

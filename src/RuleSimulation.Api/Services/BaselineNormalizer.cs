using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Services;

public static class BaselineNormalizer
{
    public static DecisionFamily? NormalizeProjected(RuleAction? action) => action switch
    {
        RuleAction.AUTO_APPROVE => DecisionFamily.AUTO_APPROVE,
        RuleAction.AUTO_DECLINE => DecisionFamily.AUTO_DECLINE,
        RuleAction.ROUTE_SPECIALIST => DecisionFamily.ROUTE_SPECIALIST,
        RuleAction.MANUAL_REVIEW => DecisionFamily.MANUAL,
        _ => null
    };

    public static DecisionFamily NormalizeRecorded(RecordedOutcome outcome) => outcome switch
    {
        RecordedOutcome.AUTO_APPROVED => DecisionFamily.AUTO_APPROVE,
        RecordedOutcome.AUTO_DECLINED => DecisionFamily.AUTO_DECLINE,
        RecordedOutcome.ROUTED_SPECIALIST => DecisionFamily.ROUTE_SPECIALIST,
        RecordedOutcome.MANUAL_APPROVED => DecisionFamily.MANUAL,
        RecordedOutcome.MANUAL_DECLINED => DecisionFamily.MANUAL,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
    };
}

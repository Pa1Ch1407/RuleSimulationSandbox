namespace RuleSimulation.Api.Domain;

public sealed class RequestRow
{
    public required string RequestId { get; init; }
    public DateTime SubmittedAt { get; init; }
    public Channel Channel { get; init; }
    public Region? Region { get; init; }
    public AccountTier AccountTier { get; init; }
    public required string ItemCode { get; init; }
    public ItemClass ItemClass { get; init; }
    public int Quantity { get; init; }
    public decimal DeclaredValue { get; init; }
    public int ItemAgeDays { get; init; }
    public bool HasDocumentation { get; init; }
    public int PriorRequests90d { get; init; }
    public bool FlaggedDuplicate { get; init; }
    public RecordedOutcome RecordedOutcome { get; init; }
}

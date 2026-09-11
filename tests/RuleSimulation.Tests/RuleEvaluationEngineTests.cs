using RuleSimulation.Api.Domain;
using RuleSimulation.Api.Rules;
using Xunit;

namespace RuleSimulation.Tests;

public sealed class RuleEvaluationEngineTests
{
    private readonly RuleEvaluationEngine _engine = new();

    [Fact]
    public void Gte_matches_boundary_and_rejects_just_below()
    {
        var ruleSet = RuleSet(new Rule
        {
            Id = 1, Name = "Value threshold", Priority = 1, Enabled = true, DisplayOrder = 1,
            ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_APPROVE,
            Conditions = [new RuleCondition { Field = "declared_value", Operator = ConditionOperator.GTE, Value = "100.00" }]
        });

        Assert.True(_engine.Evaluate(Request(100m), ruleSet).Matched);
        Assert.False(_engine.Evaluate(Request(99.99m), ruleSet).Matched);
    }

    [Fact]
    public void Numeric_all_six_operators_work()
    {
        Assert.True(Eval(10, ConditionOperator.EQ, "10"));
        Assert.True(Eval(10, ConditionOperator.NEQ, "9"));
        Assert.True(Eval(10, ConditionOperator.GT, "9"));
        Assert.True(Eval(10, ConditionOperator.GTE, "10"));
        Assert.True(Eval(10, ConditionOperator.LT, "11"));
        Assert.True(Eval(10, ConditionOperator.LTE, "10"));
    }

    [Fact]
    public void Boolean_operator_matches_expected_value()
    {
        var ruleSet = RuleSet(new Rule
        {
            Id = 1, Name = "Duplicate", Priority = 1, Enabled = true, DisplayOrder = 1,
            ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE,
            Conditions = [new RuleCondition { Field = "flagged_duplicate", Operator = ConditionOperator.IS_TRUE }]
        });

        Assert.True(_engine.Evaluate(Request(10m, duplicate: true), ruleSet).Matched);
        Assert.False(_engine.Evaluate(Request(10m, duplicate: false), ruleSet).Matched);
    }

    [Fact]
    public void In_operator_matches_member_case_insensitively()
    {
        var ruleSet = RuleSet(new Rule
        {
            Id = 1, Name = "Regions", Priority = 1, Enabled = true, DisplayOrder = 1,
            ConditionJoin = ConditionJoin.AND, Action = RuleAction.ROUTE_SPECIALIST,
            Conditions = [new RuleCondition { Field = "region", Operator = ConditionOperator.IN, ValuesJson = "[\"EMEA\",\"APAC\"]" }]
        });

        Assert.True(_engine.Evaluate(Request(10m, region: Region.APAC), ruleSet).Matched);
        Assert.False(_engine.Evaluate(Request(10m, region: Region.NA), ruleSet).Matched);
    }

    [Fact]
    public void And_requires_all_conditions()
    {
        var ruleSet = RuleSet(new Rule
        {
            Id = 1, Name = "High value duplicate", Priority = 1, Enabled = true, DisplayOrder = 1,
            ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE,
            Conditions =
            [
                new RuleCondition { Field = "declared_value", Operator = ConditionOperator.GT, Value = "100" },
                new RuleCondition { Field = "flagged_duplicate", Operator = ConditionOperator.IS_TRUE }
            ]
        });

        Assert.True(_engine.Evaluate(Request(101m, duplicate: true), ruleSet).Matched);
        Assert.False(_engine.Evaluate(Request(101m, duplicate: false), ruleSet).Matched);
    }

    [Fact]
    public void Or_requires_any_condition()
    {
        var ruleSet = RuleSet(new Rule
        {
            Id = 1, Name = "High or duplicate", Priority = 1, Enabled = true, DisplayOrder = 1,
            ConditionJoin = ConditionJoin.OR, Action = RuleAction.AUTO_DECLINE,
            Conditions =
            [
                new RuleCondition { Field = "declared_value", Operator = ConditionOperator.GT, Value = "100" },
                new RuleCondition { Field = "flagged_duplicate", Operator = ConditionOperator.IS_TRUE }
            ]
        });

        Assert.True(_engine.Evaluate(Request(101m, duplicate: false), ruleSet).Matched);
        Assert.True(_engine.Evaluate(Request(50m, duplicate: true), ruleSet).Matched);
        Assert.False(_engine.Evaluate(Request(50m, duplicate: false), ruleSet).Matched);
    }

    [Fact]
    public void First_match_wins_by_priority()
    {
        var ruleSet = RuleSet(
            new Rule { Id = 1, Name = "Low", Priority = 10, Enabled = true, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_APPROVE, Conditions = [Condition("quantity", ConditionOperator.GTE, "1")] },
            new Rule { Id = 2, Name = "High", Priority = 100, Enabled = true, DisplayOrder = 2, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE, Conditions = [Condition("quantity", ConditionOperator.GTE, "1")] });

        Assert.Equal(RuleAction.AUTO_DECLINE, _engine.Evaluate(Request(), ruleSet).Action);
    }

    [Fact]
    public void Equal_priority_uses_display_order_as_tie_breaker()
    {
        var ruleSet = RuleSet(
            new Rule { Id = 1, Name = "Later", Priority = 50, Enabled = true, DisplayOrder = 2, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE, Conditions = [Condition("quantity", ConditionOperator.GTE, "1")] },
            new Rule { Id = 2, Name = "Earlier", Priority = 50, Enabled = true, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.ROUTE_SPECIALIST, Conditions = [Condition("quantity", ConditionOperator.GTE, "1")] });

        var result = _engine.Evaluate(Request(), ruleSet);
        Assert.Equal("Earlier", result.WinningRule!.Name);
        Assert.Equal(RuleAction.ROUTE_SPECIALIST, result.Action);
    }

    [Fact]
    public void Disabled_rule_is_ignored()
    {
        var ruleSet = RuleSet(new Rule { Id = 1, Name = "Disabled", Priority = 100, Enabled = false, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE, Conditions = [Condition("quantity", ConditionOperator.GTE, "1")] });
        Assert.False(_engine.Evaluate(Request(), ruleSet).Matched);
    }

    [Fact]
    public void No_matching_rule_returns_no_match()
    {
        var ruleSet = RuleSet(new Rule { Id = 1, Name = "Impossible", Priority = 1, Enabled = true, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE, Conditions = [Condition("quantity", ConditionOperator.GT, "500")] });
        var result = _engine.Evaluate(Request(quantity: 500), ruleSet);
        Assert.False(result.Matched);
        Assert.Null(result.Action);
        Assert.Null(result.WinningRule);
    }

    [Fact]
    public void Evaluation_does_not_mutate_request()
    {
        var request = Request(100m, duplicate: true, quantity: 9);
        var ruleSet = RuleSet(new Rule { Id = 1, Name = "Rule", Priority = 1, Enabled = true, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_DECLINE, Conditions = [Condition("flagged_duplicate", ConditionOperator.IS_TRUE, "true")] });
        _ = _engine.Evaluate(request, ruleSet);
        Assert.Equal(100m, request.DeclaredValue);
        Assert.True(request.FlaggedDuplicate);
        Assert.Equal(9, request.Quantity);
        Assert.Equal(RecordedOutcome.MANUAL_APPROVED, request.RecordedOutcome);
    }

    private bool Eval(int quantity, ConditionOperator op, string value)
    {
        var rs = RuleSet(new Rule { Id = 1, Name = "n", Priority = 1, Enabled = true, DisplayOrder = 1, ConditionJoin = ConditionJoin.AND, Action = RuleAction.AUTO_APPROVE, Conditions = [Condition("quantity", op, value)] });
        return _engine.Evaluate(Request(quantity: quantity), rs).Matched;
    }

    private static RuleCondition Condition(string field, ConditionOperator op, string value) => new() { Field = field, Operator = op, Value = value };

    private static RuleSet RuleSet(params Rule[] rules) => new() { Id = 1, Name = "Test", Enabled = true, Rules = rules.ToList() };

    private static RequestRow Request(decimal declaredValue = 100m, bool duplicate = false, int quantity = 1, Region? region = Region.EMEA) => new()
    {
        RequestId = "REQ-TEST", SubmittedAt = DateTime.UtcNow, Channel = Channel.PORTAL, Region = region,
        AccountTier = AccountTier.GOLD, ItemCode = "ITM-1", ItemClass = ItemClass.CLASS_A, Quantity = quantity,
        DeclaredValue = declaredValue, ItemAgeDays = 10, HasDocumentation = true, PriorRequests90d = 1,
        FlaggedDuplicate = duplicate, RecordedOutcome = RecordedOutcome.MANUAL_APPROVED
    };
}

using System.Globalization;
using System.Text.Json;
using RuleSimulation.Api.Domain;

namespace RuleSimulation.Api.Rules;

public interface IRuleEvaluationEngine
{
    EvaluationResult Evaluate(RequestRow request, RuleSet ruleSet);
}

public sealed class RuleEvaluationEngine : IRuleEvaluationEngine
{
    private static readonly HashSet<string> NumericFields = [
        "quantity", "declared_value", "item_age_days", "prior_requests_90d"
    ];

    private static readonly HashSet<string> StringOrEnumFields = [
        "channel", "region", "account_tier", "item_code", "item_class"
    ];

    private static readonly HashSet<string> BooleanFields = [
        "has_documentation", "flagged_duplicate"
    ];

    public EvaluationResult Evaluate(RequestRow request, RuleSet ruleSet)
    {
        var orderedRules = ruleSet.Rules
            .Where(x => x.Enabled)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id);

        foreach (var rule in orderedRules)
        {
            var matched = rule.ConditionJoin == ConditionJoin.AND
                ? rule.Conditions.OrderBy(x => x.Sequence).All(c => Matches(request, c))
                : rule.Conditions.OrderBy(x => x.Sequence).Any(c => Matches(request, c));

            if (matched)
                return new EvaluationResult(true, rule, rule.Action);
        }

        return new EvaluationResult(false, null, null);
    }

    private static bool Matches(RequestRow request, RuleCondition condition)
    {
        var field = condition.Field.Trim().ToLowerInvariant();

        if (BooleanFields.Contains(field))
            return MatchBoolean(GetBoolean(request, field), condition);

        if (NumericFields.Contains(field))
            return MatchNumeric(GetDecimal(request, field), condition);

        if (StringOrEnumFields.Contains(field))
            return MatchString(GetString(request, field), condition);

        throw new InvalidOperationException($"Unsupported condition field '{condition.Field}'.");
    }

    private static bool MatchBoolean(bool actual, RuleCondition condition) => condition.Operator switch
    {
        ConditionOperator.IS_TRUE => actual,
        ConditionOperator.IS_FALSE => !actual,
        _ => throw new InvalidOperationException($"Operator '{condition.Operator}' is not valid for boolean fields.")
    };

    private static bool MatchNumeric(decimal actual, RuleCondition condition)
    {
        if (!decimal.TryParse(condition.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var expected))
            throw new InvalidOperationException($"Value '{condition.Value}' is not numeric for field '{condition.Field}'.");

        return condition.Operator switch
        {
            ConditionOperator.EQ => actual == expected,
            ConditionOperator.NEQ => actual != expected,
            ConditionOperator.GT => actual > expected,
            ConditionOperator.GTE => actual >= expected,
            ConditionOperator.LT => actual < expected,
            ConditionOperator.LTE => actual <= expected,
            _ => throw new InvalidOperationException($"Operator '{condition.Operator}' is not valid for numeric fields.")
        };
    }

    private static bool MatchString(string actual, RuleCondition condition)
    {
        var values = condition.ValuesJson is not null
            ? JsonSerializer.Deserialize<string[]>(condition.ValuesJson) ?? []
            : condition.Value is null ? [] : [condition.Value];

        return condition.Operator switch
        {
            ConditionOperator.EQUALS or ConditionOperator.EQ => values.Count() == 1 && string.Equals(actual, values[0], StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NOT_EQUALS or ConditionOperator.NEQ => values.Count() == 1 && !string.Equals(actual, values[0], StringComparison.OrdinalIgnoreCase),
            ConditionOperator.IN => values.Any(v => string.Equals(actual, v, StringComparison.OrdinalIgnoreCase)),
            ConditionOperator.NOT_IN => values.All(v => !string.Equals(actual, v, StringComparison.OrdinalIgnoreCase)),
            _ => throw new InvalidOperationException($"Operator '{condition.Operator}' is not valid for string/enum fields.")
        };
    }

    private static decimal GetDecimal(RequestRow r, string field) => field switch
    {
        "quantity" => r.Quantity,
        "declared_value" => r.DeclaredValue,
        "item_age_days" => r.ItemAgeDays,
        "prior_requests_90d" => r.PriorRequests90d,
        _ => throw new InvalidOperationException($"Unknown numeric field '{field}'.")
    };

    private static bool GetBoolean(RequestRow r, string field) => field switch
    {
        "has_documentation" => r.HasDocumentation,
        "flagged_duplicate" => r.FlaggedDuplicate,
        _ => throw new InvalidOperationException($"Unknown boolean field '{field}'.")
    };

    private static string GetString(RequestRow r, string field) => field switch
    {
        "channel" => r.Channel.ToString(),
        "region" => r.Region?.ToString() ?? string.Empty,
        "account_tier" => r.AccountTier.ToString(),
        "item_code" => r.ItemCode,
        "item_class" => r.ItemClass.ToString(),
        _ => throw new InvalidOperationException($"Unknown string/enum field '{field}'.")
    };
}

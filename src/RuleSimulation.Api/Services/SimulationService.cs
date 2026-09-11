using Microsoft.EntityFrameworkCore;
using RuleSimulation.Api.Contracts;
using RuleSimulation.Api.Data;
using RuleSimulation.Api.Domain;
using RuleSimulation.Api.Rules;

namespace RuleSimulation.Api.Services;

public sealed class SimulationService(AppDbContext db, IRuleEvaluationEngine engine)
{
    public async Task<SimulationResponse> RunAsync(SimulationRequest input, CancellationToken ct)
    {
        if (input.Page < 1) throw new ArgumentException("Page must be >= 1.");
        if (input.PageSize < 1 || input.PageSize > 500) throw new ArgumentException("PageSize must be between 1 and 500.");

        RuleSet ruleSet;
        if (input.RuleSetId.HasValue)
        {
            ruleSet = await db.RuleSets.Include(x => x.Rules).ThenInclude(x => x.Conditions).AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.RuleSetId.Value, ct)
                ?? throw new KeyNotFoundException($"Rule set {input.RuleSetId.Value} was not found.");
        }
        else if (input.Draft is not null)
        {
            ruleSet = ToDomain(input.Draft);
        }
        else
        {
            throw new ArgumentException("Provide either ruleSetId or draft.");
        }

        var requests = await db.Requests.AsNoTracking().OrderBy(x => x.RequestId).ToListAsync(ct);
        var summary = Enum.GetValues<RuleAction>().ToDictionary(x => x.ToString(), _ => 0);
        summary["NO_MATCH"] = 0;
        var attribution = ruleSet.Rules.ToDictionary(x => x.Id, _ => 0);
        var changed = new List<ChangedDecisionDto>();
        var agreement = 0;

        foreach (var request in requests)
        {
            var result = engine.Evaluate(request, ruleSet);
            if (!result.Matched)
            {
                summary["NO_MATCH"]++;
            }
            else
            {
                summary[result.Action!.Value.ToString()]++;
                attribution[result.WinningRule!.Id]++;
            }

            var projectedFamily = BaselineNormalizer.NormalizeProjected(result.Action);
            var baselineFamily = BaselineNormalizer.NormalizeRecorded(request.RecordedOutcome);
            if (projectedFamily == baselineFamily)
            {
                agreement++;
            }
            else
            {
                changed.Add(new ChangedDecisionDto(request.RequestId, result.Action?.ToString() ?? "NO_MATCH",
                    result.WinningRule?.Name, request.RecordedOutcome.ToString()));
            }
        }

        var pageItems = changed.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
        var rate = requests.Count == 0 ? 0 : Math.Round((decimal)agreement / requests.Count, 4);
        return new SimulationResponse(
            requests.Count,
            summary,
            new BaselineComparisonDto(agreement, rate, changed.Count),
            attribution.Select(x => new RuleAttributionDto(x.Key, ruleSet.Rules.Single(r => r.Id == x.Key).Name, x.Value)).ToList(),
            new PagedChangedDecisionDto(input.Page, input.PageSize, changed.Count, pageItems));
    }

    private static RuleSet ToDomain(DraftRuleSetDto input) => new()
    {
        Id = 0,
        Name = input.Name,
        Enabled = input.Enabled,
        Rules = input.Rules.Select((r, ri) => new Rule
        {
            Id = -(ri + 1), Name = r.Name, Priority = r.Priority, Enabled = r.Enabled,
            ConditionJoin = r.ConditionJoin, Action = r.Action, DisplayOrder = r.DisplayOrder,
            Conditions = r.Conditions.Select((c, ci) => new RuleCondition
            {
                Id = -(ci + 1), Sequence = ci + 1, Field = c.Field, Operator = c.Operator,
                Value = c.Value, ValuesJson = c.Values is null ? null : System.Text.Json.JsonSerializer.Serialize(c.Values)
            }).ToList()
        }).ToList()
    };
}

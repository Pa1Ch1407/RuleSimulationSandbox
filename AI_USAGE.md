AI Usage

Tools and Models

Cline (VS Code extension, open source), initially used with Qwen2.5-Coder:7B locally via Ollama.

Cline was successfully connected to Ollama, but the local setup was not practical for agentic development. Requests repeatedly timed out because the model was running on CPU and Cline was sending a large agent prompt with each request. I increased the request timeout, configured a larger Ollama context window, and pre-warmed the model, but the workflow remained too slow to be useful.

I also attempted to use OpenRouter with a paid model through Cline, but the request was blocked by insufficient account credit.

Continue (VS Code extension, open source), used with Autocomplete via OpenRouter.

After the Cline/Ollama attempt, I moved to Continue because it provided a more practical development workflow for this project. The shorter prompts, large hosted-model context, and explicit diff-based changes made it easier to review every modification before accepting it.

The configuration was kept outside the repository.

How I Used AI?

I used the AI assistant primarily for:

Reviewing implementation approaches and identifying potential issues.
Generating targeted code changes rather than allowing unrestricted repository-wide rewrites.
Generating and extending unit tests.
Reviewing API, rule-engine, database, and UI implementation decisions.
Investigating specific problems such as Swagger startup behavior and validation coverage.

AI-generated changes were not accepted blindly. I reviewed each diff and either accepted, modified, or reverted it based on the requirements and the existing code.

I also committed before significant AI-assisted changes so that an incorrect change could be reverted safely.

AI Decisions I Rejected or Substantially Rewrote

1. Swagger startup configuration

I wanted Swagger to open automatically when the API started.

The assistant initially led me toward changing the API registration and middleware configuration. I had already correctly configured `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI`.

The actual missing configuration was in Properties/launchSettings.json, where the profile needed:

{
  "launchBrowser": true,
  "launchUrl": "swagger"
}

I accepted this suggestion because it correctly identified that the API configuration was already working and that the problem was with the launch profile.

This was a useful example of AI helping identify the boundary between application configuration and development tooling configuration.

2. Full template regeneration for a small UI change

I asked for a small UI change: adding a "Duplicate rule" button next to the existing "Remove rule" button.

The assistant regenerated most of the HTML template instead of making the requested targeted change. The generated version removed existing `@for` loops, caused the Action, Match When, and Operator dropdowns to disappear, removed the existing Remove Rule button, and inserted explanatory prose into the HTML.

The application still rendered, so the problem was not immediately obvious.

I rejected the change and reverted the file.

This changed my workflow for subsequent AI-assisted edits: I selected the exact lines requiring modification, requested smaller changes, and rejected diffs that were substantially larger than the requested change.

3. Performance optimisation suggestions

I asked how to improve the simulation loop that evaluates the 25,000 historical requests.

The assistant suggested several approaches including:

caching aggregate results;
rebuilding cached results in the background;
caching compiled rules;
parallelising evaluation with thread-local summaries;
reducing allocations;
profiling before optimisation.

I rejected the background cache because the main use case includes unsaved drafts that can change frequently, making a cache less useful, and persisted simulation history is outside the requested scope.

I also rejected parallel evaluation because merging changed-decision results could introduce nondeterministic ordering. Reproducibility is one of the hard requirements, so the same rule set and data must produce identical results.

I accepted the recommendation to profile first. The measurement showed that the database read was more significant than the rule-evaluation loop at the current 25,000-row scale, so I did not introduce unnecessary optimisation.

4. Generated unit tests

The assistant generated tests for operator compatibility around `item_code`.

The tests were useful, but I strengthened them before accepting them.

One generated test checked only:

expect(r.live).toEqual({})


I changed this to validate the complete error collection:

expect(r.all).toEqual({})

The original assertion could pass even when another validation error existed, so it was weaker than intended.

I also moved the tests into the operator-compatibility test section instead of the `live` versus `all` validation section because their purpose was operator compatibility, not validation timing.

5. UI structure

The assistant initially produced a single page containing saved rule sets, the rule builder, and simulation results.

I rejected this structure.

I chose to separate the workflow into:

/rule-sets/new
/rule-sets/{id}

for creating and editing rule sets.

This better matches the workflow of an operations user who starts from existing rule sets rather than always starting from a blank form.

6. Database setup

The assistant suggested removing my SQL database setup and making the EF migration the only source of the schema.

I rejected that approach because I already had a working SQL Server setup and seed-loading process.

The actual issue was that two schema definitions had drifted apart: the EF migration used one request table definition while the SQL setup scripts used `Requests` and `Requests_Staging`.

I resolved the conflict by removing only the duplicate tables that the API did not use instead of dropping the working database and replacing the entire setup.

Bug Found Outside the AI Suggestions

One important issue was not caught by the tooling.

GET /api/rule-sets/{id} returned enum values as numbers:

{
  "conditionJoin": 0,
  "action": 1,
  "operator": 10
}

The UI expected enum names, so loaded rule sets resulted in empty dropdown selections even though POST requests worked when strings were supplied.

I fixed this by registering:

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    });

Using allowIntegerValues: false also prevents clients from silently sending numeric enum values.

This reinforced the need to test both serialization directions and not assume that because one API path works, the entire API contract is correct.

Which Parts of the Design Were Mine

The following architectural decisions were made by me and were not delegated to the AI:

ASP.NET Core Web API with EF Core and SQL Server.
Relational `RuleSet -> Rule -> RuleCondition` database structure.
Loading the historical CSV into SQL Server instead of parsing it for every simulation request.
Treating historical requests as read-only during simulation.
First-match-wins rule evaluation.
Higher priority rules evaluated first.
DisplayOrder as the tie-break for equal-priority rules.
Explicit NO_MATCH behavior.
Supporting both saved rule-set simulation and unsaved draft simulation.
Keeping simulation results transient rather than persisting simulation history.
Server-side pagination of changed decisions.
Server-side validation as the final validation boundary.
Exposing API enums as meaningful strings rather than numeric values.
Preserving missing region values as NULL rather than inventing a value.
Keeping database constraints to protect the integrity of historical data.

The AI was used as an implementation and review assistant, but these decisions were driven by the exercise requirements, observed data, and engineering judgement.

Working Style and Safeguards

I used the following safeguards when working with AI-assisted changes:

1. I committed the repository before significant AI changes.
2. I preferred targeted changes over whole-file regeneration.
3. I reviewed every generated diff before accepting it.
4. I reverted changes that exceeded the requested scope.
5. I validated generated tests by checking that they could actually fail for the wrong implementation.
6. I compared AI suggestions against the explicit requirements before implementing them.
7. I avoided introducing additional architecture or infrastructure unless there was a clear requirement or measured need.

Generated vs Hand-Written

Approximately:

AI-generated or AI-assisted: 20%

Hand-written or substantially rewritten: 80%

The percentage is an estimate rather than an exact line count. The final implementation includes substantial manual design, review, correction, and rewriting.

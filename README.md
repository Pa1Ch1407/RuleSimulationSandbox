# Rule Simulation Sandbox - Backend

This repository is the backend foundation for the Rule Simulation Sandbox take-home exercise.

## Stack

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core 10
- SQL Server
- xUnit

The SQL Server provider is the standard EF Core provider for SQL Server. See Microsoft's EF Core SQL Server provider documentation for installation/configuration details.

## 1. Create the database

Run:

```sql
sql/01_create_database.sql
```

Then update the connection string in:

```text
src/RuleSimulation.Api/appsettings.json
```

## 2. Load the 25,000 requests

The exercise says the CSV should be loaded into the database rather than parsed on each API call.

Update the CSV path in:

```text
sql/02_bulk_load_requests.sql
```

and execute it in SSMS/Azure Data Studio.

## 3. Run the API

Prerequisite: .NET 10 SDK and SQL Server.

```bash
dotnet restore
dotnet run --project src/RuleSimulation.Api
```

Swagger is available in Development mode at the application's `/swagger` URL.

## Rule semantics

1. Only enabled rules are evaluated.
2. Higher priority wins.
3. Equal priority is resolved by `DisplayOrder` ascending, then `RuleId` ascending.
4. First matching rule wins.
5. Conditions within one rule are joined by either AND or OR.
6. No matching rule produces `NO_MATCH`.
7. Simulation is read-only against Requests (`AsNoTracking`, no request writes).
8. A simulation may use a saved rule set or an unsaved draft.

## Baseline comparison

The exercise defines projected actions `AUTO_APPROVE`, `AUTO_DECLINE`, `ROUTE_SPECIALIST`, `MANUAL_REVIEW`, while historical outcomes are `AUTO_APPROVED`, `AUTO_DECLINED`, `ROUTED_SPECIALIST`, `MANUAL_APPROVED`, `MANUAL_DECLINED`.

Because the source does not specify a direct mapping for `MANUAL_REVIEW`, this backend compares decision families:

- AUTO_APPROVE <-> AUTO_APPROVED
- AUTO_DECLINE <-> AUTO_DECLINED
- ROUTE_SPECIALIST <-> ROUTED_SPECIALIST
- MANUAL_REVIEW <-> either MANUAL_APPROVED or MANUAL_DECLINED

That assumption should remain explicitly documented in the final submission.

## API surface

### Rule sets

```http
GET    /api/rule-sets
GET    /api/rule-sets/{id}
POST   /api/rule-sets
PUT    /api/rule-sets/{id}
DELETE /api/rule-sets/{id}
```

### Simulation

```http
POST /api/simulations
```

The request accepts either `ruleSetId` or an inline `draft` rule set. `page` and `pageSize` control the changed-decision list.

## Example simulation request

```json
{
  "ruleSetId": 1,
  "page": 1,
  "pageSize": 50
}
```

or:

```json
{
  "draft": {
    "name": "Draft safety rules",
    "enabled": true,
    "rules": [
      {
        "name": "Duplicate requests",
        "priority": 100,
        "enabled": true,
        "conditionJoin": "AND",
        "action": "AUTO_DECLINE",
        "displayOrder": 1,
        "conditions": [
          {
            "field": "flagged_duplicate",
            "operator": "IS_TRUE",
            "value": null,
            "values": null
          }
        ]
      }
    ]
  },
  "page": 1,
  "pageSize": 50
}
```

## Tests

The evaluation engine has focused unit tests for:

- numeric operators;
- boundary values;
- enum/string membership;
- booleans;
- AND/OR semantics;
- priority ordering;
- equal-priority tie-break;
- disabled rules;
- no-match;
- request immutability.

Run:

```bash
dotnet test
```

## Next steps

After the backend is stable, build the plain UI required by the exercise, add optional explainability, strengthen server-side validation, and add API-level tests around rule-set CRUD and simulation pagination.

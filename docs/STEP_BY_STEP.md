# Step-by-step implementation plan

## Step 1 - SQL Server

1. Create `RuleSimulation` database.
2. Create Requests, RuleSets, Rules, RuleConditions.
3. Load `requests_seed.csv` once.
4. Verify 25,000 rows.
5. Verify request IDs are unique.

## Step 2 - API shell

1. Create ASP.NET Core Web API.
2. Add EF Core SQL Server provider.
3. Configure `AppDbContext`.
4. Confirm the application starts and Swagger loads.

## Step 3 - Rule-set CRUD

1. Implement GET list.
2. Implement GET single.
3. Implement POST.
4. Implement PUT.
5. Implement DELETE.
6. Add stronger server-side validation.

## Step 4 - Rule evaluation engine

1. Resolve a request field.
2. Evaluate the operator.
3. Combine conditions.
4. Sort rules deterministically.
5. Stop at the first match.
6. Return `NO_MATCH` when none matches.

## Step 5 - Simulation

1. Accept saved rule set or draft.
2. Read historical requests with `AsNoTracking`.
3. Evaluate every request on the server.
4. Aggregate projected actions.
5. Calculate baseline agreement.
6. Count rule attribution.
7. Paginate changed decisions.

## Step 6 - Unit tests

Focus on the evaluator, especially boundary values and equal-priority ties.

## Step 7 - UI

Build the rule builder and results screens only after the API contract is stable.

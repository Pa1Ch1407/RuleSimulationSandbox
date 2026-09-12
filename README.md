# Rule Simulation Sandbox

This repository contains the implementation of the **Rule Simulation Sandbox** take-home exercise.

The solution is split into two main parts:

* **Backend** – ASP.NET Core Web API responsible for rule persistence, evaluation, and simulation.
* **Frontend** – Angular application that allows users to create, edit, manage, and simulate rule sets.

The application allows a reviewer to build rules visually, save them, run simulations against the request dataset, and inspect the impact of the rules.

---

# Solution Overview

The Rule Simulation Sandbox allows users to define a collection of business rules and simulate how those rules would behave against historical requests.

A typical workflow is:

1. Create a new rule set.
2. Add one or more rules.
3. Define conditions for each rule.
4. Configure priority, action, enabled status, and AND/OR condition behavior.
5. Save the rule set or run it directly as an unsaved draft.
6. Review the simulation results.
7. Inspect summary statistics, baseline comparison, rule attribution, and changed decisions.

The UI communicates with the ASP.NET Core backend through REST APIs.

---

# Repository Structure

```text
RuleSimulationSandbox
│
├── src/
│   └── RuleSimulation.Api/        # ASP.NET Core Web API
│
├── tests/
│   └── ...                        # Backend unit tests
│
├── sql/
│   ├── 01_create_database.sql
│   └── 02_bulk_load_requests.sql
│
└── angular/
    ├── src/
    │   └── app/
    │       ├── components/        # Reusable UI components
    │       ├── core/              # Models, API service, validation
    │       └── pages/             # Application pages
    │
    ├── package.json
    └── proxy.conf.json
```

---

# Technology Stack

## Backend

* ASP.NET Core Web API
* .NET 10
* Entity Framework Core 10
* SQL Server
* xUnit

## Frontend

* Angular
* TypeScript
* Angular Signals
* Angular Router
* Angular Forms
* HttpClient
* Jasmine/Karma test setup

---

# 1. Backend Setup

## Create the database

Run:

```sql
sql/01_create_database.sql
```

Then update the connection string in:

```text
src/RuleSimulation.Api/appsettings.json
```

---

# 2. Load the Request Dataset

The exercise requires the CSV data to be loaded into the database instead of parsing the CSV file for every API request.

Update the CSV path in:

```text
sql/02_bulk_load_requests.sql
```

Then execute the script using SQL Server Management Studio or Azure Data Studio.

The dataset contains approximately **25,000 requests** used by the simulation engine.

---

# 3. Run the Backend API

Prerequisites:

* .NET 10 SDK
* SQL Server

Run:

```bash
dotnet restore
dotnet run --project src/RuleSimulation.Api
```

The API can then be accessed through Swagger.

```text
/swagger
```

---

# Rule Evaluation Semantics

The rule evaluation engine follows these rules:

1. Only enabled rules are evaluated.
2. Rules with a higher priority are evaluated first.
3. If two rules have the same priority:

   * `DisplayOrder` ascending is used.
   * `RuleId` ascending is used as the final tie-breaker.
4. The first matching rule determines the decision.
5. Conditions within a rule are combined using either:

   * `AND`
   * `OR`
6. If no rule matches, the result is:

```text
NO_MATCH
```

7. Simulation is read-only against the request dataset.
8. Requests are queried using `AsNoTracking`.
9. Simulation does not modify the historical request records.
10. A simulation can run against either:

    * a saved rule set;
    * an unsaved draft rule set.

---

# Baseline Comparison Assumption

The exercise defines projected rule actions as:

```text
AUTO_APPROVE
AUTO_DECLINE
ROUTE_SPECIALIST
MANUAL_REVIEW
```

Historical request outcomes are:

```text
AUTO_APPROVED
AUTO_DECLINED
ROUTED_SPECIALIST
MANUAL_APPROVED
MANUAL_DECLINED
```

Because the exercise does not explicitly define how `MANUAL_REVIEW` maps to historical outcomes, the implementation compares decisions using the following decision families:

| Projected Action   | Historical Outcome                     |
| ------------------ | -------------------------------------- |
| `AUTO_APPROVE`     | `AUTO_APPROVED`                        |
| `AUTO_DECLINE`     | `AUTO_DECLINED`                        |
| `ROUTE_SPECIALIST` | `ROUTED_SPECIALIST`                    |
| `MANUAL_REVIEW`    | `MANUAL_APPROVED` or `MANUAL_DECLINED` |

This assumption is intentionally documented because it affects the baseline comparison results.

---

# API Surface

## Rule Sets

```http
GET    /api/rule-sets
GET    /api/rule-sets/{id}
POST   /api/rule-sets
PUT    /api/rule-sets/{id}
DELETE /api/rule-sets/{id}
```

These endpoints support creating, retrieving, updating, and deleting saved rule sets.

---

## Simulation

```http
POST /api/simulations
```

The simulation endpoint accepts either:

* `ruleSetId` for a saved rule set; or
* `draft` for an unsaved rule set.

The changed-decision results support pagination through:

```text
page
pageSize
```

---

# Example Simulation Request

## Simulating a Saved Rule Set

```json
{
  "ruleSetId": 1,
  "page": 1,
  "pageSize": 50
}
```

## Simulating an Unsaved Draft

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

---

# Frontend Application

The Angular application provides the UI required to work with rule sets and simulations.

The frontend is located in:

```text
angular/
```

The application is structured around pages, reusable components, API integration, validation, and tests.

```text
angular/src/app
│
├── components/
│   ├── rule-set-list
│   ├── rule-builder
│   └── simulation-results
│
├── core/
│   ├── api.service
│   ├── models
│   ├── validation
│   ├── field-catalog
│   ├── editable
│   └── examples
│
└── pages/
    ├── dashboard.page
    └── rule-set-editor.page
```

---

# Frontend Features

## 1. Rule Set Dashboard

The dashboard displays all saved rule sets.

For each rule set, the reviewer can see:

* Rule set name
* Number of rules
* Enabled/disabled status
* Last saved date
* Open action
* Delete action

The dashboard also provides:

```text
Start a new rule set
```

to create a new rule set.

The UI retrieves rule sets using:

```http
GET /api/rule-sets
```

Deletion uses:

```http
DELETE /api/rule-sets/{id}
```

A confirmation dialog is displayed before deleting a rule set.

---

# 2. Rule Set Editor

The rule set editor supports both creating and editing rule sets.

Routes include:

```text
/rule-sets/new
```

for a new rule set, and:

```text
/rule-sets/{id}
```

for editing an existing saved rule set.

The editor supports:

* Rule set name
* Enabled/disabled state
* Adding rules
* Removing rules
* Reordering rules
* Rule priority
* Rule enabled state
* Rule actions
* AND/OR condition joining
* Adding conditions
* Removing conditions
* Reordering conditions

The UI dynamically updates the available operators based on the selected field.

When a field changes, incompatible operators or values are reset to prevent invalid combinations.

---

# 3. Rule Ordering

Rules can be moved up or down in the UI.

This allows the reviewer to visually control rule ordering.

The backend evaluation behavior still follows the documented rule semantics:

1. Priority descending.
2. Display order ascending.
3. Rule ID ascending.

The UI uses ordering controls to update the editable rule collection before saving or simulating.

---

# 4. Client-Side Validation

The frontend performs validation before saving or running a simulation.

Validation errors are shown in the UI.

The validation system distinguishes between:

* live validation while editing;
* full validation after attempting to save or run;
* server-side validation errors returned by the API.

The UI combines client-side and server-side validation errors so the user can see all relevant problems in the appropriate fields.

If the rule set contains validation problems, the user is prevented from:

```text
Saving
```

or:

```text
Running a simulation
```

until the issues are resolved.

---

# 5. Unsaved Change Detection

The editor tracks the current state of the rule set.

A snapshot is maintained after loading or saving.

If the user modifies the rule set, the application detects that the current version differs from the saved baseline.

When navigating away with unsaved changes, the user receives a confirmation prompt.

This helps prevent accidental loss of work.

---

# 6. Saved Rule Set vs Draft Simulation

The frontend supports two simulation modes.

## Saved Rule Set Simulation

If the rule set is saved and has not been modified since the last save, the UI sends:

```json
{
  "ruleSetId": 1
}
```

to the backend.

## Draft Simulation

If the rule set contains unsaved changes, the UI sends the current rule definition as a draft.

Conceptually:

```json
{
  "draft": {
    "name": "Current draft",
    "rules": []
  }
}
```

This allows the reviewer to experiment with rule changes without saving them first.

---

# 7. Simulation Result Consistency

After a simulation is executed, the application stores the exact simulation request and a snapshot of the rule set.

This is important when the user changes the rule builder after running a simulation.

The result page continues paging through the **same simulation input** that was originally executed.

In other words:

* Run simulation.
* Modify rules.
* Navigate simulation pages.

The pagination still uses the original simulation request rather than silently applying the new edits.

The UI also detects when the displayed simulation result is stale compared to the currently edited rule set.

---

# 8. Simulation Results

The simulation results component provides several views of the simulation output.

## Summary

Displays the number and percentage of requests assigned to each action:

```text
Auto-approve
Auto-decline
Route to specialist
Manual review
No rule matched
```

The percentage is calculated relative to the total dataset count.

---

## Baseline Comparison

The backend compares projected decisions against historical outcomes using the documented decision-family mapping.

This allows the reviewer to understand how the proposed rule set differs from the historical request outcomes.

---

## Rule Attribution

The simulation results include per-rule attribution information.

The UI highlights useful situations such as:

* Disabled rules
* Rules that never fired
* Rules that match a very large percentage of requests
* Broad rules that match more than a quarter of requests

For example:

```text
Never fired. Check its conditions or priority.
```

or:

```text
Fires on 90% or more of all requests.
```

This provides lightweight explainability for reviewing the behavior of a rule set.

---

## Changed Decision List

The changed-decision list is paginated.

Supported page sizes are:

```text
25
50
100
200
```

The browser does not need to load the complete changed-decision dataset at once.

Instead, when the user changes the page, the frontend calls the simulation endpoint again using the original simulation request and the requested page.

---

# API Integration

The frontend communicates with the backend through a central `ApiService`.

The service provides methods for:

```text
listRuleSets()
getRuleSet(id)
createRuleSet()
updateRuleSet(id)
deleteRuleSet(id)
simulate()
```

The frontend uses relative API URLs:

```text
/api
```

During development, the Angular proxy configuration forwards API requests to the ASP.NET Core backend.

This avoids hardcoding the backend URL throughout the application.

---

# Error Handling

The frontend handles both network and API validation errors.

If the backend cannot be reached, the user receives a message indicating that the API should be started.

API errors are converted into a common error structure containing:

```text
message
fieldErrors
```

Validation errors returned by the backend are merged with client-side validation errors and displayed in the UI.

---

# Example Rule Sets

The frontend includes predefined example rule sets.

These examples can be loaded into the editor for demonstration and testing.

An example is loaded as an unsaved rule set, allowing the reviewer to:

* inspect the rule configuration;
* modify the rules;
* run a draft simulation;
* save the example as a new rule set.

---

# Frontend Setup

Navigate to the Angular project:

```bash
cd angular
```

Install dependencies:

```bash
npm install
```

Run the frontend:

```bash
npm start
```

or, depending on the configured Angular scripts:

```bash
ng serve
```

The frontend uses the development proxy configuration to communicate with the ASP.NET Core API.

Before using the application, ensure that the backend API is running.

---

# Running Tests

## Backend Tests

The backend includes focused tests for the rule evaluation engine.

Coverage includes:

* Numeric operators
* Boundary values
* Enum and string membership
* Boolean conditions
* AND semantics
* OR semantics
* Rule priority ordering
* Equal-priority tie-breaking
* Disabled rules
* No-match behavior
* Request immutability

Run:

```bash
dotnet test
```

---

# Frontend Tests

The Angular application also includes unit tests for the main frontend functionality.

Tests cover areas including:

## API Service

Tests verify communication with:

* Rule set list endpoint
* Rule set retrieval
* Rule set creation
* Rule set updates
* Rule set deletion
* Simulation endpoint
* API and network error handling

## Rule Builder

Tests cover behavior such as:

* Adding rules
* Removing rules
* Reordering rules
* Adding conditions
* Removing conditions
* Reordering conditions
* Field changes
* Operator changes
* Value reset behavior

## Validation

Tests cover client-side rule set validation and validation error handling.

## Editable Rule Set Conversion

Tests verify conversion between:

* API DTOs
* Editable UI models
* API request models

## Rule Set List

Tests verify rendering and interaction for saved rule sets.

## Simulation Results

Tests verify:

* Summary calculations
* Action labels
* Outcome labels
* Attribution messages
* Pagination behavior

## Dashboard

Tests cover loading and displaying rule sets as well as dashboard interactions.

## Rule Set Editor

Tests cover editor behavior including:

* Loading a rule set
* Creating a new rule set
* Loading examples
* Saving
* Running simulations
* Draft versus saved simulation behavior
* Dirty state detection
* Stale simulation detection
* Pagination
* Error handling

Run the frontend tests with:

```bash
cd angular
npm test
```

---

# Design Decisions

## Draft-First Simulation

The application allows simulation without forcing the user to save changes.

This makes experimentation easier and prevents unnecessary persistence of temporary rule configurations.

---

## Centralized API Access

All HTTP communication is handled through a dedicated API service.

This keeps components focused on UI behavior and makes the API layer easier to test.

---

## Reusable Components

The UI is divided into reusable components:

```text
RuleSetList
RuleBuilder
SimulationResults
```

The pages are responsible for orchestration while components focus on their specific UI responsibilities.

---

## Client and Server Validation

Client-side validation provides immediate feedback.

Server-side validation remains the final authority.

Both error sources are presented to the user so that API validation failures are not hidden.

---

## Simulation Snapshot

A simulation snapshot is maintained to ensure that pagination continues to represent the exact simulation that was originally executed.

This prevents confusing behavior when a user edits rules after viewing results.

---

# How to Review the Application

A reviewer can follow this workflow:

## 1. Start SQL Server

Create the database and load the request dataset.

## 2. Start the Backend

```bash
dotnet run --project src/RuleSimulation.Api
```

Verify the API through Swagger.

```text
/swagger
```

## 3. Start the Angular Application

```bash
cd angular
npm install
npm start
```

## 4. Create a Rule Set

Use:

```text
Start a new rule set
```

Add rules and conditions.

## 5. Run a Draft Simulation

Modify the rules and select the simulation action without saving.

The current rule definition is sent as an inline draft.

## 6. Save the Rule Set

Save the configuration.

The rule set receives an ID and can subsequently be retrieved and edited.

## 7. Run the Saved Rule Set

Run a simulation without changing the saved configuration.

The frontend sends the rule set ID instead of the full draft.

## 8. Review Results

Inspect:

* Action summary
* Baseline comparison
* Rule attribution
* Changed decisions
* Pagination

## 9. Modify the Rules

After a simulation, change one or more rules.

The UI identifies that the existing simulation results are based on an earlier version of the rule set.

---

# Assumptions and Limitations

The following assumption is important for evaluating the simulation results:

```text
MANUAL_REVIEW
```

is compared against either:

```text
MANUAL_APPROVED
```

or:

```text
MANUAL_DECLINED
```

because the exercise does not define a more specific mapping.

The implementation documents this behavior explicitly so that the reviewer can understand how baseline comparisons are calculated.

---

# Future Improvements

Possible next steps include:

* Additional API-level integration tests
* End-to-end UI tests
* More advanced rule explainability
* Rule execution traces for individual requests
* Improved validation messages
* Authentication and authorization
* Audit history for rule set changes
* Exporting simulation results
* Performance testing with larger datasets

---

# Summary

The completed solution provides:

* Rule set CRUD functionality
* Visual rule creation and editing
* Rule and condition reordering
* Dynamic field/operator handling
* Client-side and server-side validation
* Unsaved change detection
* Saved and draft simulations
* Paginated changed-decision results
* Simulation consistency through request snapshots
* Rule attribution and basic explainability
* Backend evaluation tests
* Frontend unit tests

The backend remains responsible for rule evaluation and simulation, while the Angular frontend provides a clear interface for creating, testing, and reviewing rule behavior.

# Decisions

- Higher priority wins — deterministic and intuitive; assumes larger number means higher importance.
- Equal priority uses `DisplayOrder`, then ID — avoids unspecified database ordering; adds one ordering field.
- First matching rule wins — simple attribution and predictable outcomes; later matching rules are ignored.
- `NO_MATCH` is explicit — preserves the distinction between no action and an actual rule action.
- Baseline comparison uses decision families — necessary because `MANUAL_REVIEW` has no exact historical counterpart; product owner could settle whether manual outcomes should instead count as changes.
- Rules use relational tables — easier CRUD/validation/querying than a single JSON document; requires more mapping code.
- Simulation is not persisted — fewer tables/state and naturally supports draft simulation; historical run comparison is unavailable.
- Historical requests are read-only — strongest protection against corrupting baseline data; adds a little operational permission/configuration work.

- API uses resource-oriented endpoints — keeps rule-set CRUD and simulation responsibilities separate; introduces multiple endpoints instead of one generic endpoint.
- API accepts saved rule sets or an unsaved draft for simulation — directly supports experimentation without persistence; requires two input paths and validation logic.
- DTOs are separate from EF entities — prevents database structure from becoming the public API contract; requires additional mapping code.
- Enums are exposed as strings in JSON — improves readability and avoids clients depending on numeric enum values; requires explicit JSON enum configuration.
- Validation is performed server-side even when the UI validates — protects the API from invalid/non-browser clients; creates some duplicated validation between UI and API.
- API returns `400 Bad Request` for invalid rule definitions with structured validation errors — makes client correction straightforward; requires consistent error handling.
- Simulation results are aggregated server-side — avoids sending 25,000 rows to the browser and keeps rule evaluation centralized; uses more server-side CPU.
- Changed decisions are paginated — keeps HTTP responses bounded and supports the primary results view; requires deterministic pagination ordering.
- Simulation reads requests with no-tracking access — reinforces the read-only simulation guarantee and reduces EF tracking overhead; entities cannot be modified through the tracked context during the run.
- Database enforces key data constraints — prevents invalid actions/outcomes and broken relationships even if API validation is bypassed; adds migration/schema maintenance.
- Seed data is loaded once into SQL Server rather than parsed by the API — separates ingestion from runtime simulation and provides stable historical data; requires a separate database setup step.
- Missing `region` values are stored as `NULL` — preserves the supplied dataset instead of inventing a value; rule evaluation must define how `NULL` behaves for region conditions.
- Simulation output is ordered deterministically by request ID — makes repeated runs and pagination reproducible; ordering is not based on insertion time or database default order.
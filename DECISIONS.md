# Decisions

- Higher priority wins — deterministic and intuitive; assumes larger number means higher importance.
- Equal priority uses `DisplayOrder`, then ID — avoids unspecified database ordering; adds one ordering field.
- First matching rule wins — simple attribution and predictable outcomes; later matching rules are ignored.
- `NO_MATCH` is explicit — preserves the distinction between no action and an actual rule action.
- Baseline comparison uses decision families — necessary because `MANUAL_REVIEW` has no exact historical counterpart; product owner could settle whether manual outcomes should instead count as changes.
- Rules use relational tables — easier CRUD/validation/querying than a single JSON document; requires more mapping code.
- Simulation is not persisted — fewer tables/state and naturally supports draft simulation; historical run comparison is unavailable.
- Historical requests are read-only — strongest protection against corrupting baseline data; adds a little operational permission/configuration work.

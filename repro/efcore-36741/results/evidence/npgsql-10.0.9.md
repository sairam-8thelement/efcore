# Npgsql provider — skip detail (EF Core 10.0.9)

Not auto-written by `TranslationEvidence` (that type only records on a
*caught exception* from a compiled-query call; a skipped provider never
reaches that call at all — `ProviderGuard.ShouldRun` returns early before
any test body executes). This file instead records, once, the fixture-level
probe failure that caused all 14 Npgsql-tagged tests to report **SKIP**
(not a silent omission) in `results/baseline-10.0.9.md`.

## Probe result

- Provider: Npgsql
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- `NpgsqlFixture.IsAvailable`: `false`
- `NpgsqlFixture.SkipReason`:
  ```
  Npgsql: no reachable PostgreSQL server at the configured connection (Docker daemon may not be running, or no container listening on the configured port): 28P01: password authentication failed for user "postgres"
  ```

A PostgreSQL server **is** listening on `localhost:5432` in this
environment (the connection reaches the server — the failure is
authentication, not an unreachable host). The project's default probe
credentials (`postgres`/`postgres`, `src/NpgsqlFixture.cs`) do not match
whatever credentials that server is actually configured with. Per this
task's instructions not to hunt for credentials belonging to a service this
project does not own, no attempt was made to discover the real ones; the
fixture therefore correctly reports the provider as unavailable rather than
guessing or failing destructively. `EFCORE36741_NPGSQL_CONNSTRING` can be
set to override the probe connection string and re-run this provider.

## Tests skipped (14 of 14 Npgsql-tagged tests)

Each writes `SKIPPED: <SkipReason above>` to its own test output before
returning, per `ProviderGuard.ShouldRun` — visible per-test in the TRX, not
just at the fixture level:

```
NpgsqlExecuteUpdateCompiledQueryTests.Inline_ExecuteUpdateAsync_updates_all_rows
NpgsqlExecuteUpdateCompiledQueryTests.Inline_ExecuteUpdateAsync_with_filter_updates_only_target
NpgsqlExecuteUpdateCompiledQueryTests.CompiledAsync_ExecuteUpdate_updates_all_rows
NpgsqlExecuteUpdateCompiledQueryTests.CompiledAsync_ExecuteUpdate_with_filter_updates_only_target
NpgsqlExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_updates_all_rows
NpgsqlExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_with_filter_updates_only_target
NpgsqlExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows
NpgsqlExecuteDeleteCompiledQueryTests.CompiledSync_ExecuteDelete_removes_all_rows
NpgsqlExecuteDeleteCompiledQueryTests.CompiledSync_ExecuteDelete_with_filter_removes_only_target
NpgsqlExecuteDeleteCompiledQueryTests.CompiledAsync_body_ExecuteDelete_removes_all_rows
NpgsqlExecuteDeleteCompiledQueryTests.CompiledAsync_body_ExecuteDelete_with_filter_removes_only_target
NpgsqlExecuteDeleteCompiledQueryTests.CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target
NpgsqlSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_update
NpgsqlSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_delete
```

These report as xunit `Passed` (the test method returns normally after
writing the `SKIPPED:` line — see `ProviderCollections.cs`'s `ProviderGuard`
doc comment for why), which is why `results/baseline-10.0.9.md` reclassifies
them from the raw TRX's "25 passed" into 11 genuine PASS + 14 SKIP, rather
than reporting them as real passes.

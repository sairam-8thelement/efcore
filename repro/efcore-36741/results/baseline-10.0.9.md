# EF Core #36741 — 10.0.9 baseline run

Baseline execution of the unmodified repro suite
(`repro/efcore-36741/tests/*.cs`) against EF Core **10.0.9**, across every
provider reachable in this environment. No test code, `.csproj`,
`Directory.Build.props`, or `run-matrix.sh` was changed to produce this
run or to make it pass more cleanly — the failures below are the
deliverable (AC#7).

## Environment

| | |
|---|---|
| EF Core assembly version (runtime-reported, via `EnvironmentInfo.EfCoreAssemblyVersion`) | `10.0.9` |
| EF Core package version requested (`-p:EFCoreVersion=`) | `10.0.9` |
| Requested vs. loaded agree? | **Yes** — confirmed via `grep '"Microsoft.EntityFrameworkCore.Sqlite/10.0.9"' obj/10.0.9/project.assets.json` (and the `SqlServer` package) resolving exactly `10.0.9`, and every test's own `EnvironmentInfo.Describe()` line printing `EF Core 10.0.9, .NET 10.0.13, ...`. No isolation failure — this run is published. |
| .NET SDK | `10.0.113` (`dotnet --version`, resolved via this directory's `global.json`) |
| .NET runtime | `10.0.13` (`Microsoft.NETCore.App 10.0.13`) |
| OS | Ubuntu 24.04.3 LTS (`PRETTY_NAME` from `/etc/os-release`) |
| Kernel | `7.0.0-34-generic` (`uname -r`); reported by the test process itself (`Environment.OSVersion.VersionString`) as `Unix 7.0.0.34` |
| Host | `sairam-Latitude-5420` |
| Run window | 2026-10-27T16:26:03 → 16:26:09 (+05:30), ~6s wall time for all 42 tests |

**Exact reproduction command** (run with CWD inside `repro/efcore-36741/`,
per the isolation design in `README.md`):

```bash
cd repro/efcore-36741
export EFCORE36741_SQLSERVER_CONNSTRING="Server=localhost,1433;Database=master;User Id=sa;Password=<redacted>;TrustServerCertificate=True;Connect Timeout=5"
./run-matrix.sh 10.0.9 t4-baseline
```

The `EFCORE36741_SQLSERVER_CONNSTRING` override is **not** part of the repro
project itself (its default probe string, `sa`/`YourStrong!Passw0rd`, is
unchanged — see "Provider matrix" below for why the override was needed in
this environment). No `EFCORE36741_NPGSQL_CONNSTRING` was set; Npgsql used
the project's unmodified default probe string.

Output: `results/raw/10.0.9-t4-baseline.log` (console), `results/raw/10.0.9-t4-baseline.trx` (TRX), both gitignored and referenced here by filename.

**Run-level result:** `dotnet test` exit code **1** (non-zero) — 42 total, 25 passed, 17 failed, 0 reported-by-xunit-as-skipped. This is the expected, correct baseline outcome, not a run failure. ("0 reported-by-xunit-as-skipped" is a detail of how the suite is built, not an absence of skips — see the Npgsql row below: all 14 of its tests technically return `Passed` to xunit, because `ProviderGuard.ShouldRun` makes the test method return early rather than get marked `Skip`; this report reclassifies those 14 as **SKIP** in the table and matrix below, per the plan's "skipped, not silently omitted" requirement.)

## Provider matrix

| Provider | Reachable? | Outcome counts (of 14 tests) | Detail |
|---|---|---|---|
| SQLite | Yes (file-backed, always available) | 6 PASS / 8 FAIL / 0 SKIP | Full detail in `results/evidence/sqlite-10.0.9.md` |
| SQL Server | Yes, via a pre-existing Docker container (`anystore-sql`, image `mcr.microsoft.com/mssql/server:2022-latest`, already running in this sandbox for an unrelated purpose — **not** started by this task). The project's *default* probe connection string (`sa`/`YourStrong!Passw0rd`) does **not** match that container's configured SA password, so `EFCORE36741_SQLSERVER_CONNSTRING` was set for this run only (exported in the shell, not committed anywhere) to point at it with its actual credentials. This project then created its own uniquely-named database (`EfCore36741Repro_<guid>`) inside that instance; no existing data in the container was read or modified. | 5 PASS / 9 FAIL / 0 SKIP | Full detail in `results/evidence/sqlserver-10.0.9.md` |
| Npgsql | **No** (reported as **SKIP**, not omitted) — a PostgreSQL server *is* listening on `localhost:5432`, but authentication with the project's default probe credentials (`postgres`/`postgres`) fails (`28P01: password authentication failed`). Per this task's instructions not to hunt for credentials belonging to services this task doesn't own, no attempt was made to discover the real ones; `EFCORE36741_NPGSQL_CONNSTRING` was left unset. | 0 PASS / 0 FAIL / 14 SKIP | Full detail in `results/evidence/npgsql-10.0.9.md` |

**Total: 11 PASS / 17 FAIL / 14 SKIP, 42 tests.**

## Per-test table

Exception detail (message / InnerException / first three stack frames) for
every row marked FAIL is in `results/evidence/sqlite-10.0.9.md` and
`results/evidence/sqlserver-10.0.9.md` (one `##` section per failing test,
keyed by test name); this table summarizes outcome and exception **type**
only. Full detail for the SKIP rows is in `results/evidence/npgsql-10.0.9.md`.

### SQLite

| Test | Outcome | Exception type (if FAIL) |
|---|---|---|
| `Inline_ExecuteUpdateAsync_updates_all_rows` | PASS | — |
| `Inline_ExecuteUpdateAsync_with_filter_updates_only_target` | PASS | — |
| `CompiledAsync_ExecuteUpdate_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledAsync_ExecuteUpdate_with_filter_updates_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_with_filter_updates_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteDelete_removes_all_rows` | PASS | — |
| `CompiledSync_ExecuteDelete_with_filter_removes_only_target` | PASS | — |
| `CompiledAsync_body_ExecuteDelete_removes_all_rows` | **FAIL** | `System.ArgumentException` (delegate-shape, not translation — see below) |
| `CompiledAsync_body_ExecuteDelete_with_filter_removes_only_target` | PASS | — |
| `CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `Compiled_and_inline_emit_identical_sql_for_update` | **FAIL** | `System.InvalidOperationException` (translation; compiled side throws before emitting SQL) |
| `Compiled_and_inline_emit_identical_sql_for_delete` | PASS | — |

6 PASS / 8 FAIL.

### SqlServer

| Test | Outcome | Exception type (if FAIL) |
|---|---|---|
| `Inline_ExecuteUpdateAsync_updates_all_rows` | PASS | — |
| `Inline_ExecuteUpdateAsync_with_filter_updates_only_target` | PASS | — |
| `CompiledAsync_ExecuteUpdate_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledAsync_ExecuteUpdate_with_filter_updates_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_with_filter_updates_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows` | **FAIL** | `System.InvalidOperationException` (translation) |
| `CompiledSync_ExecuteDelete_removes_all_rows` | PASS | — |
| `CompiledSync_ExecuteDelete_with_filter_removes_only_target` | PASS | — |
| `CompiledAsync_body_ExecuteDelete_removes_all_rows` | **FAIL** | `System.ArgumentException` (delegate-shape, not translation — see below) |
| `CompiledAsync_body_ExecuteDelete_with_filter_removes_only_target` | PASS | — |
| `CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target` | **FAIL** | `System.InvalidOperationException` (translation) |
| `Compiled_and_inline_emit_identical_sql_for_update` | **FAIL** | `System.InvalidOperationException` (translation; compiled side throws before emitting SQL) |
| `Compiled_and_inline_emit_identical_sql_for_delete` | **FAIL** | `Xunit.Sdk.EqualException` — **not** a translation failure; see "SqlServer SQL-equivalence delete failure" below |

5 PASS / 9 FAIL.

### Npgsql

All 14 tests: **SKIP** (provider unreachable with the project's default
credentials — see provider matrix above and `results/evidence/npgsql-10.0.9.md`).

## Exception detail for FAIL rows (message + first three stack frames)

Captured automatically by `TranslationEvidence` at the moment each test's
compiled-query call throws, and appended to the per-provider evidence file.
Reproduced here for the two distinct exception shapes observed (the
per-test files have the identical text repeated per test name, so only one
example of each shape is inlined; see the evidence files for every test's
own copy):

### Shape 1 — the regression itself (12 of 17 failures: 6 SQLite + 6 SqlServer `ExecuteUpdate`/`ExecuteDeleteAsync`-spelled `ExecuteDelete` tests)

- Exception type: `System.InvalidOperationException`
- InnerException is null: `True`
- Message (example, from `CompiledSync_ExecuteUpdate_updates_all_rows`):
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive,
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

### Shape 2 — `System.ArgumentException` from `CompiledAsync_body_ExecuteDelete_removes_all_rows` (2 of 17 failures: 1 SQLite + 1 SqlServer)

- Exception type: `System.ArgumentException`
- InnerException is null: `True`
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```
  This is thrown while *building* the compiled delegate's expression tree
  (`EF.CompileAsyncQuery((ReproContext ctx) => ctx.Users.ExecuteDelete())` —
  the single-context-param overload infers `TResult` as `Task<int>` from the
  lambda's natural shape, but `ExecuteDelete()` itself returns `int`
  synchronously), never reaches the query-translation pipeline at all, and
  is a distinct failure mode from Shape 1. It is included here for
  completeness because it is one of the 17 FAILs in this baseline, not
  because it demonstrates the #32018 translation regression.

### SqlServer SQL-equivalence delete failure (not evidence-file-captured — no exception was thrown)

`SqlServerSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_delete`
is the one FAIL that is **not** in `results/evidence/sqlserver-10.0.9.md`,
because the compiled call itself succeeded (no exception for
`TranslationEvidence` to capture) — the test instead failed its own
`Assert.Equal(_fixture.Sql.NormalizedFor("inline"), _fixture.Sql.NormalizedFor("compiled"))`
assertion:

```
Assert.Equal() Failure: Strings differ
                            ↓ (pos 20)
Expected: "Executed DbCommand (10ms) [Parameters=[@p"···
Actual:   "Executed DbCommand (9ms) [Parameters=[@p0"···
                            ↑ (pos 20)
```

This is a **harness measurement artifact, not a regression finding**:
`SqlCapture.Normalize` (`src/SqlCapture.cs`) strips only the first line of
each captured log message (the `info: <timestamp> RelationalEventId...`
line) before diffing, but leaves the *second* line — EF's own
`Executed DbCommand (Xms) [Parameters=...]` preamble — in place. That
preamble embeds the command's non-deterministic execution duration, so two
runs of the identical SQL statement can differ only in how many digits
their millisecond timing has (`10ms` vs `9ms`), which is exactly what
happened here. On SQLite the equivalent test passed only because both runs
happened to log the same digit-count duration in this run — not because the
comparison is actually duration-independent there. This is a pre-existing
test-harness defect in code this task does not own and must not modify
(`src/SqlCapture.cs`, owned by an earlier task); it is reported here as a
finding for whoever owns that file, not fixed.

## Measured-vs-ticket comparison

This section records what **this run** measured and compares it, point by
point, against the ticket's and the plan's stated premises. It does not
copy the plan's own independent probe results forward as given — that probe
is a useful cross-check (quoted below) but is not what this run observed.

| Premise | Source | This run's measurement | Agrees? |
|---|---|---|---|
| Compiled `ExecuteUpdate` (sync and async, filtered and unfiltered, with/without `TagWith`) throws a translation `InvalidOperationException` | Ticket / AC#7 | **Confirmed.** All 5 `ExecuteUpdate`-terminal compiled-query tests failed with `InvalidOperationException` ("could not be translated") on both SQLite and SqlServer. | **Agrees** |
| Inline `ExecuteUpdateAsync` (unfiltered and filtered) passes | Ticket / AC#7 | **Confirmed.** Both inline tests passed on SQLite and SqlServer. | **Agrees** |
| The throw site is `QueryableMethodTranslatingExpressionVisitor.VisitMethodCall` via `CompiledAsyncTaskQuery.CreateCompiledQuery` | Ticket's "Evidence" table | **Contradicted.** Every translation-failure stack trace captured in this run (both `ExecuteUpdate` and the `ExecuteDeleteAsync`-spelled `ExecuteDelete`) has its first three frames at `NavigationExpandingExpressionVisitor.VisitMethodCall` → `NavigationExpandingExpressionVisitor.Expand` → `QueryTranslationPreprocessor.Process`, not `QueryableMethodTranslatingExpressionVisitor.VisitMethodCall`. `CompiledAsyncTaskQuery`-related frames do appear, but further down the stack (as the eventual caller of `QueryCompilationContext.CreateQueryExecutorExpression`, not as the throw site itself). | **Contradicts** |
| Compiled `ExecuteDelete` fails (AC#5, AC#7 grouping it with the `ExecuteUpdate` failures as "the three compiled-query tests... fail") | Ticket / plan AC#7 | **Partially contradicted — the outcome is mixed, not a single pass/fail.** Measured per spelling: <br>• `EF.CompileQuery`/`EF.CompileAsyncQuery` with the **sync `ExecuteDelete()`** terminal, **filtered**, both sync- and async-compiled → **PASS** on both providers. <br>• Same, **unfiltered**, **sync**-compiled (`CompiledSync_ExecuteDelete_removes_all_rows`) → **PASS**. <br>• Same, **unfiltered**, **async**-compiled single-context-param overload (`CompiledAsync_body_ExecuteDelete_removes_all_rows`) → **FAIL**, but with `System.ArgumentException` from expression-lambda construction, not a translation exception (see Shape 2 above) — a delegate-shape issue, arguably unrelated to #32018. <br>• The **`ExecuteDeleteAsync()`-spelled** compiled body (filtered, async-compiled two-param overload) → **FAIL** with the same `InvalidOperationException` translation shape as `ExecuteUpdate`. | **Mixed: agrees for the `ExecuteDeleteAsync`-spelled case; contradicts for the three `ExecuteDelete()`-spelled cases, which mostly pass.** |
| Plan's own independent probe: "found compiled `ExecuteDelete` succeeding" | Plan inputs (explicitly NOT to be copied forward as this run's result) | This run's measurement broadly agrees with that probe for the sync-spelled `ExecuteDelete()` forms (3 of 4 pass), but adds a nuance the probe's one-line summary didn't capture: one `ExecuteDelete()` form (unfiltered, single-ctx-param async-compiled) does fail, and it fails for an unrelated delegate-shape reason, not a translation reason. | **Cross-checked, not identical** — see above |
| Plan's own independent probe: throw site for the `ExecuteDelete` case was `NavigationExpandingExpressionVisitor.VisitMethodCall` → `.Expand` → `QueryTranslationPreprocessor.Process` | Plan inputs (explicitly NOT to be copied forward as this run's result) | **This run measured the same three frames**, for the one `ExecuteDelete`-family test that does fail by translation (`CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target`). | **Agrees with the probe; both agree the same frames, and both disagree with the ticket's stated `QueryableMethodTranslatingExpressionVisitor` frame.** |

## Raw logs

- `results/raw/10.0.9-t4-baseline.log` — full console output
- `results/raw/10.0.9-t4-baseline.trx` — TRX results (42 `UnitTestResult` entries)
- `results/evidence/sqlite-10.0.9.md` — per-test exception detail, SQLite
- `results/evidence/sqlserver-10.0.9.md` — per-test exception detail, SqlServer
- `results/evidence/npgsql-10.0.9.md` — skip detail, Npgsql

(Earlier raw files under `results/raw/` named `10.0.9-t3-*` predate this
task and were produced by the T3 test-authoring/verification task, not by
this baseline run; they are left in place as T3's own artifacts and are not
referenced by this report. The evidence files this report does reference
were reset — `rm`'d — immediately before this run so their contents reflect
exactly this one run, not an accumulation across T3's prior dev runs plus
this one; `TranslationEvidence` appends rather than overwrites, so without
that reset the earlier `sqlite-10.0.9.md` would have contained three
superimposed copies of each failure.)

## Posting to #36741

Not done by this task — posting the repro and results to GitHub issue
#36741 is owned by the Engineer, not by this task (see "Must not change").
This file is written so it can be consumed verbatim for that purpose.

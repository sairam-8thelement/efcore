## Repro + results: compiled `ExecuteUpdate`/`ExecuteDelete` translation failure (EF Core 10)

**EF Core version(s):** 10.0.9 baseline; also probed 10.0.10, 10.0.11, 10.0.12, 10.0.13, 10.0.14
**Database provider(s):** SQLite (primary), SQL Server (via a local Docker container), PostgreSQL/Npgsql (unreachable with default credentials — reported as skipped below)
**OS:** Ubuntu 24.04.3 LTS, kernel `7.0.0-34-generic`
**.NET SDK / runtime:** SDK 10.0.113, runtime 10.0.13

I built a small isolated repro project (not part of this repo's own build) with a `User { Id, LastActive }` entity, a fixed 3-row seed, and a test suite that runs the *same* query four ways: inline `ExecuteUpdateAsync`, `EF.CompileAsyncQuery(...).ExecuteUpdate`, `EF.CompileQuery(...).ExecuteUpdate`, and compiled `ExecuteDelete`, each with an unfiltered and a parameterized-`Where` variant. The test code is unmodified between every run below — only the EF Core package version passed via `-p:EFCoreVersion=` changes.

### Baseline on EF Core 10.0.9

Inline `ExecuteUpdateAsync` passes (and its filtered variant). Every compiled `ExecuteUpdate` variant — sync, async, filtered, unfiltered, with `TagWith` — fails with:

```
System.InvalidOperationException (InnerException: null)
The LINQ expression 'DbSet<User>()
  .ExecuteUpdate(s => s.SetProperty<DateTime>(
      propertyExpression: u => u.LastActive,
      valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a
form that can be translated, or switch to client evaluation explicitly by inserting a call to
'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See
https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
```

First three stack frames (identical for every failing `ExecuteUpdate` test, both SQLite and SQL Server):

```
at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
```

**Correction to this issue's own "Thrown from" field:** the throw site measured here is `NavigationExpandingExpressionVisitor.VisitMethodCall` → `.Expand` → `QueryTranslationPreprocessor.Process` (an earlier pipeline stage), not `QueryableMethodTranslatingExpressionVisitor.VisitMethodCall`. The pipeline never reaches the later visitor because the earlier one throws first. `CompiledAsyncTaskQuery.CreateCompiledQuery` does appear in the full stack, but further down, as the caller that triggers compilation — not as the frame that throws.

**Correction to this issue's `ExecuteDelete` framing:** compiled `ExecuteDelete()` (the sync-spelled terminal operator) does **not** regress — it passes in 3 of 4 forms tested (sync-compiled unfiltered/filtered, async-compiled filtered), including with `TagWith`. Only two `ExecuteDelete`-family cases fail, and for two different reasons unrelated to the `ExecuteDelete()` spelling itself:
- `EF.CompileAsyncQuery((ctx) => ctx.Users.ExecuteDelete())` (single-context-param overload, no filter) fails with `System.ArgumentException: Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task\`1[System.Int32]'` while *building* the delegate — a delegate-shape/overload-resolution issue (that overload infers `TResult=Task<int>` from the lambda shape), not a query-translation failure, and it throws before reaching the translation pipeline at all.
- `EF.CompileAsyncQuery((ctx, id) => ctx.Users.Where(u => u.Id == id).ExecuteDeleteAsync())` — the `ExecuteDeleteAsync()` spelling, a distinct `MethodInfo` from `ExecuteDelete()` — fails with the same translation exception and stack as `ExecuteUpdate`.

So the actual regression surface is: compiled **`ExecuteUpdate`** (every form) and compiled-async bodies spelled **`ExecuteDeleteAsync()`**. Compiled `ExecuteDelete()` itself works. This narrower framing matches this issue's own title ("ExecuteUpdate no longer works within compiled queries").

### Provider matrix (10.0.9 baseline)

| Provider | Reachable | Result |
|---|---|---|
| SQLite | Yes | 6 PASS / 8 FAIL (identical failure set/messages as below) |
| SQL Server (Docker, `mcr.microsoft.com/mssql/server:2022-latest`) | Yes | 5 PASS / 9 FAIL — same 8 translation/delegate-shape failures as SQLite, plus one unrelated SQL-text-comparison flake (see "Known harness artifact" below) |
| PostgreSQL / Npgsql | **No — reported as SKIP, not omitted.** A server listens on `localhost:5432`, but authentication with the project's default probe credentials fails (`28P01: password authentication failed`). No attempt was made to discover the real credentials for a service this project doesn't own. | 0 PASS / 0 FAIL / 14 SKIP |

**Known harness artifact (not a regression finding):** the SQL-text-equivalence test for `ExecuteDelete` occasionally fails on a captured-log timing digit (`Executed DbCommand (3ms)` vs `(1ms)`) because the comparison doesn't strip that non-deterministic preamble. It flaked across repeated runs and across the five probed 10.x versions below, independent of EF Core version. The `ExecuteUpdate` SQL-equivalence test fails for the real reason on every run: the compiled side throws before emitting any SQL at all.

### Fixed-build probe (10.0.10 through 10.0.14, SQLite)

Every stable EF Core 10.x patch newer than 10.0.9, up to and including **10.0.14** (the newest on nuget.org as of this report), reproduces the identical regression: every compiled `ExecuteUpdate` test and the `ExecuteDeleteAsync()`-spelled compiled delete test fail with the same exception type, message shape, and stack trace as 10.0.9. **No released build carries a fix.**

EF Core 11 previews/RCs (newest: `11.0.0-rc.3.26522.148`) could not be probed in this environment: they ship `net11.0`-only assets, and no `net11.0` SDK/runtime is installed here.

**Re-run recipe for whenever a fix ships** (no test-code changes needed — only the version argument varies):
```
./run-matrix.sh <new-version> <label> Sqlite
```
A green run (exit code 0) on all 14 SQLite tests, including the two SQL-equivalence tests passing by `LogTo`-captured text comparison, would confirm the fix and that compiled/inline SQL are identical.

### Reproduction

```bash
cd repro/efcore-36741         # must run from here — global.json pins the SDK used
./run-matrix.sh 10.0.9 baseline        # reproduces the regression
./run-matrix.sh 10.0.14 latest Sqlite  # confirms it persists through the newest patch
```

Full per-test exception detail (message + first three stack frames) and raw logs are in this project's `results/` directory alongside this report.

---

Happy to run this against a build carrying a fix, or against SQL Server/Npgsql with real credentials, if that would help narrow things down further.

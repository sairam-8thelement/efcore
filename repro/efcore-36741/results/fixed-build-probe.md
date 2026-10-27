# EF Core #36741 — fixed-build probe (SQLite, EF Core 10.x patch releases)

Determines whether any EF Core build newer than the 10.0.9 baseline
(`results/baseline-10.0.9.md`, owned by T4) carries a fix for the compiled
`ExecuteUpdate`/`ExecuteDelete` translation regression, by running the
**byte-identical** repro suite (no change to any `.cs` file, the `.csproj`,
`Directory.Build.props`, or `run-matrix.sh`) against each candidate version,
varying only `-p:EFCoreVersion=<version>`.

**Conclusion: fixed-build run: PENDING — no build carrying the fix is
available as of this date (2026-10-27).** Every released EF Core 10.x patch
newer than the 10.0.9 baseline (10.0.10 through 10.0.14, the newest release
on nuget.org as of this run) reproduces the identical regression signature.
EF Core 11 previews cannot be probed in this environment at all (see
"EF Core 11 previews" below). See "Evidence for the PENDING conclusion"
for the full supporting detail.

## Environment

| | |
|---|---|
| .NET SDK | `10.0.113` (`dotnet --version`, resolved via this directory's `global.json`) |
| .NET runtime | `10.0.13` (`Microsoft.NETCore.App 10.0.13`) |
| OS | Ubuntu 24.04.3 LTS (`PRETTY_NAME` from `/etc/os-release`) |
| Kernel | `7.0.0-34-generic` (`uname -r`); reported by the test process itself (`Environment.OSVersion.VersionString`) as `Unix 7.0.0.34` |
| Host | `sairam-Latitude-5420` |
| Provider | SQLite only, per this task's scope (SQL Server/Npgsql breadth is T4's job, not re-run here) |
| Run window | 2026-10-27 16:32:09 → 16:33:36 (+05:30), ~90s wall time for all five versions |

**Sequencing note:** this probe was run strictly after T4's 10.0.9 baseline
run completed, and each of the five versions below was run strictly
sequentially (never two versions concurrently), per this task's
instructions — not because any acceptance criterion needs the overlap, but
to guarantee no two `-p:EFCoreVersion` runs could share restore state, a
container, or a port.

## Versions enumerated from nuget.org at run time

Queried live (not from memory/prior probes) via:

```bash
curl -s "https://api.nuget.org/v3-flatcontainer/microsoft.entityframeworkcore/index.json"
```

Full `10.0.*` / `11.*` slice of the returned `versions` array:

```
10.0.0-preview.1.25081.1, 10.0.0-preview.2.25163.8, 10.0.0-preview.3.25171.6,
10.0.0-preview.4.25258.110, 10.0.0-preview.5.25277.114, 10.0.0-preview.6.25358.103,
10.0.0-preview.7.25380.108, 10.0.0-rc.1.25451.107, 10.0.0-rc.2.25502.107,
10.0.0, 10.0.1, 10.0.2, 10.0.3, 10.0.4, 10.0.5, 10.0.6, 10.0.7, 10.0.8, 10.0.9,
10.0.10, 10.0.11, 10.0.12, 10.0.13, 10.0.14,
11.0.0-preview.1.26104.118, 11.0.0-preview.2.26159.112, 11.0.0-preview.3.26207.106,
11.0.0-preview.4.26230.115, 11.0.0-preview.5.26302.115, 11.0.0-preview.6.26359.118,
11.0.0-preview.7.26381.103, 11.0.0-rc.1.26425.128, 11.0.0-rc.2.26504.105,
11.0.0-rc.3.26522.148
```

**Result: `10.0.14` is still the newest stable release.** No `10.0.15` (or
later 10.x) exists on nuget.org as of this run — the plan's premise that
"released EF Core 10 patches available on nuget.org are 10.0.0 through
10.0.14" is reconfirmed live, not just carried forward from an earlier
probe. The only releases newer than 10.0.14 are EF Core 11 previews/RCs
(newest: `11.0.0-rc.3.26522.148`), addressed separately below.

All five stable patches strictly newer than the 10.0.9 baseline —
**10.0.10, 10.0.11, 10.0.12, 10.0.13, 10.0.14** — were probed. This is the
complete set; none were skipped.

## Per-version results

Command pattern (run from `repro/efcore-36741/`, SQLite-only filter):

```bash
./run-matrix.sh <version> t5-probe Sqlite
```

| Version requested | Version loaded (`EnvironmentInfo`, from each run's own stdout) | Agree? | Command | Outcome | Compiled `ExecuteUpdate` succeeded? |
|---|---|---|---|---|---|
| 10.0.10 | `EF Core 10.0.10, .NET 10.0.13, Ubuntu 24.04.3 LTS` | **Yes** | `./run-matrix.sh 10.0.10 t5-probe Sqlite` | 6 PASS / 8 FAIL (exit code 1) | **No** — same translation exception as baseline |
| 10.0.11 | `EF Core 10.0.11, .NET 10.0.13, Ubuntu 24.04.3 LTS` | **Yes** | `./run-matrix.sh 10.0.11 t5-probe Sqlite` | 5 PASS / 9 FAIL (exit code 1) | **No** — same translation exception as baseline |
| 10.0.12 | `EF Core 10.0.12, .NET 10.0.13, Ubuntu 24.04.3 LTS` | **Yes** | `./run-matrix.sh 10.0.12 t5-probe Sqlite` | 5 PASS / 9 FAIL (exit code 1) | **No** — same translation exception as baseline |
| 10.0.13 | `EF Core 10.0.13, .NET 10.0.13, Ubuntu 24.04.3 LTS` | **Yes** | `./run-matrix.sh 10.0.13 t5-probe Sqlite` | 5 PASS / 9 FAIL (exit code 1) | **No** — same translation exception as baseline |
| 10.0.14 | `EF Core 10.0.14, .NET 10.0.13, Ubuntu 24.04.3 LTS` | **Yes** | `./run-matrix.sh 10.0.14 t5-probe Sqlite` | 6 PASS / 8 FAIL (exit code 1) | **No** — same translation exception as baseline |

Every row's requested-vs-loaded version **agrees** — no isolation failure to
report for any of the five runs. This was independently cross-checked for
each version via
`grep -c 'Microsoft.EntityFrameworkCore.Sqlite/<version>' obj/<version>/project.assets.json`,
which returned `2` (present) for all five, confirming each restore pinned
exactly the requested package version, consistent with T1's isolation
design (`obj/<version>/`, `bin/<version>/` per `-p:EFCoreVersion`).

Full console output and TRX: `results/raw/<version>-t5-probe.log` /
`results/raw/<version>-t5-probe.trx` for each of the five versions
(gitignored, referenced here by filename, same convention as T4's
baseline).

### The 6/8 vs 5/9 split is a pre-existing harness flake, not a version difference

Every one of the five versions fails the **identical set of 7 tests** that
fail on the 10.0.9 baseline for the regression itself (the
`ExecuteUpdate`-terminal compiled tests, the `ExecuteDeleteAsync`-spelled
compiled delete, and the delegate-shape `ArgumentException` case) — see
"Evidence for the PENDING conclusion" below for the exact test-name list.

The one test whose outcome varies run-to-run is
`SqliteSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_delete`
(FAILs on 10.0.11/10.0.12/10.0.13, PASSes on 10.0.9/10.0.10/10.0.14). Its
failure is **not** a new exception — it is the identical
`SqlCapture.Normalize`-related timing-digit-count artifact already
documented in T4's `results/baseline-10.0.9.md` ("SqlServer SQL-equivalence
delete failure" section): the captured SQL log line's
`Executed DbCommand (Xms) [...]` preamble embeds non-deterministic
execution duration, so two runs of identical SQL can differ only in digit
count (e.g. `(3ms)` vs `(1ms)`), confirmed below for 10.0.11:

```
Assert.Equal() Failure: Strings differ
                            ↓ (pos 20)
Expected: "Executed DbCommand (3ms) [Parameters=[@p0"···
Actual:   "Executed DbCommand (1ms) [Parameters=[@p0"···
```

This is a pre-existing defect in `src/SqlCapture.cs`, a file this task does
not own and must not modify (and did not modify). It is reported here,
exactly as T4 reported its SQL-Server analogue, as a finding for whoever
owns that file — not as evidence of a version-dependent behavior change,
and not counted toward the fixed/unfixed determination below.

### `Compiled_and_inline_emit_identical_sql_for_update` fails for the real reason on every version

Unlike the `_delete` variant above, the `_update` SQL-equivalence test fails
identically on **all five** probed versions for the actual regression: the
compiled side throws the translation exception before any SQL is emitted,
exactly as documented for 10.0.9 in T4's baseline. This is counted as one of
the 7 regression failures, not as a flake.

## EF Core 11 previews: unprobeable in this environment, reconfirmed

Checked directly (not just carried forward from the plan's prior probe):

```bash
curl -s ".../microsoft.entityframeworkcore/11.0.0-rc.3.26522.148/microsoft.entityframeworkcore.nuspec" \
  | grep -o 'targetFramework="[^"]*"' | sort -u
# → targetFramework="net11.0"   (only TFM present)

dotnet --list-runtimes
# → Microsoft.AspNetCore.App 8.0.32, 10.0.13
# → Microsoft.NETCore.App    8.0.32, 10.0.13
# (no net11.0 / 11.x runtime installed)

dotnet --list-sdks
# → 8.0.132, 10.0.113   (no 11.x SDK installed)
```

EF Core 11 (`11.0.0-preview.*` through `11.0.0-rc.3.26522.148`, the newest
available) ships `net11.0`-only assets. Pointing `-p:EFCoreVersion=` at any
11.x version would require a `net11.0` TFM change to
`EfCore36741Repro.csproj` and an installed `net11.0` runtime — both out of
scope for this task (**"Must not add a net11.0 target, install runtimes...
to manufacture a fixed build"**) and, independent of scope, the installed
SDK roster above shows no 11.x SDK/runtime is even present in this
environment to attempt it with. EF Core 11 therefore cannot be probed here
under any version of this task's constraints; it is reported as **unprobed
by design**, not silently skipped.

## This repository's own `main` branch: the rewrite is still absent upstream

Independently cross-checked (read-only; no file in this repository outside
`repro/efcore-36741/` was modified):

```bash
grep -E "ExecuteUpdate|UpdateSettersBuilder|ITuple" \
  src/EFCore/Query/Internal/ExpressionTreeFuncletizer.cs
```

Zero matches, on this worktree's checked-out `main` (12.0-alpha,
`b4a08c0306`). The class the maintainer's attributed root cause
(issue #32018's query-pipeline rework) would need to special-case for
`ExecuteUpdate`/`ExecuteDelete` funcletization still contains no such
handling in the project's own upstream development branch, 12.0-alpha —
consistent with (not proof of, but corroborating) the hypothesis that no
released build yet carries a fix, since even the project's own
further-ahead main branch doesn't show the rewrite landing.

## Evidence for the PENDING conclusion

1. **All five probed 10.x versions still fail** the same 7-test regression
   signature as the 10.0.9 baseline (table above; per-test detail in
   `results/evidence/sqlite-10.0.10.md` through
   `results/evidence/sqlite-10.0.14.md`, one file per version, each
   containing the exception message and first three stack frames for every
   failing test — auto-captured by `src/TranslationEvidence.cs` at the
   moment each compiled-query call throws, identical mechanism to T4's
   `results/evidence/sqlite-10.0.9.md`).

   Exact failing-test-name set, identical across 10.0.10 / 10.0.11 /
   10.0.12 / 10.0.13 / 10.0.14 (plus the flaky 8th on three of the five, see
   above):

   ```
   SqliteExecuteDeleteCompiledQueryTests.CompiledAsync_body_ExecuteDelete_removes_all_rows
   SqliteExecuteDeleteCompiledQueryTests.CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target
   SqliteExecuteUpdateCompiledQueryTests.CompiledAsync_ExecuteUpdate_updates_all_rows
   SqliteExecuteUpdateCompiledQueryTests.CompiledAsync_ExecuteUpdate_with_filter_updates_only_target
   SqliteExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_updates_all_rows
   SqliteExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_with_filter_updates_only_target
   SqliteExecuteUpdateCompiledQueryTests.CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows
   SqliteSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_update
   ```

   Example exception (identical shape on every version, message text
   unchanged character-for-character from the 10.0.9 baseline except the
   embedded `DateTime` literal, which is non-deterministic across runs by
   test design — see `results/evidence/sqlite-10.0.14.md` for the literal
   captured on that version):

   ```
   System.InvalidOperationException
   The LINQ expression 'DbSet<User>()
     .ExecuteUpdate(s => s.SetProperty<DateTime>(
         propertyExpression: u => u.LastActive,
         valueExpression: ...))' could not be translated. Either rewrite the
   query in a form that can be translated, or switch to client evaluation
   explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable',
   'ToList', or 'ToListAsync'. See
   https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
   ```
   ```
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
   at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
   ```

2. **EF Core 11 previews are `net11.0`-only** and this environment has no
   `net11.0` SDK/runtime installed (`dotnet --list-sdks` /
   `--list-runtimes` above show only `8.0.x` and `10.0.13`), and probing
   them would require an out-of-scope TFM/runtime change this task is
   explicitly forbidden from making.

3. **This repository's own 12.0-alpha `main` branch** has zero occurrences
   of `ExecuteUpdate`, `UpdateSettersBuilder`, or `ITuple` in
   `src/EFCore/Query/Internal/ExpressionTreeFuncletizer.cs` — the
   furthest-ahead code available to inspect still shows no trace of the
   rewrite the maintainer attributed the regression to needing.

No test code, `.csproj`, `Directory.Build.props`, or `run-matrix.sh` was
changed for any of the five runs above, or to produce this report — the
only variable across all five commands was the `-p:EFCoreVersion=` value,
satisfying this task's "no change to the test code" constraint for every
probe attempt.

## Re-run recipe (fires the moment a fixed build ships)

The instant a new EF Core 10.x (or, once this repo adds a `net11.0` TFM —
out of scope here — an EF Core 11) release appears on nuget.org, re-running
this exact probe for that one version is a single command, with **no test
code changes**:

```bash
cd repro/efcore-36741
./run-matrix.sh <new-version> <label> Sqlite
```

e.g. `./run-matrix.sh 10.0.15 fixed-recheck Sqlite`.

- If it exits 0 (all 14 SQLite tests pass): that satisfies **AC#9** (fixed
  build passes on SQLite, unmodified test code) directly. AC#10 (SQL
  identical between compiled and inline) can then be read off
  `SqliteSqlEquivalenceTests` — both of its tests (`_for_update`,
  `_for_delete`) already assert this by `LogTo`-captured SQL comparison; a
  green result for both is itself the AC#10 evidence, with the captured SQL
  text available in `results/raw/<new-version>-<label>.log` and
  (if a provider gets re-added via `TranslationEvidence`) the evidence
  file — no new instrumentation needed.
- If it still fails: append a new row to the per-version table above, a new
  `results/evidence/sqlite-<new-version>.md` (auto-written by the suite —
  do not hand-edit), and update the versions-enumerated list, keeping this
  file's PENDING conclusion intact until a version actually goes green.

## If some probed version had passed

(Not applicable to this run — recorded for completeness per this task's
contract, since none of the five probed versions passed.) Had any version's
`./run-matrix.sh <version> <label> Sqlite` exited 0, this section would
instead contain: the full green `dotnet test` summary line for that
version, and the two SQL texts captured via `LogTo` by
`SqliteSqlEquivalenceTests.Compiled_and_inline_emit_identical_sql_for_update`
/ `_for_delete` showing the compiled and inline SQL strings byte-identical
after `SqlCapture.Normalize`, satisfying AC#9 and AC#10 together.

## Posting to #36741

Not done by this task — posting results to GitHub issue #36741 is owned by
the Engineer, not by this task (see "Must not change": "Must not post
anything to GitHub"). This file, together with T4's `baseline-10.0.9.md`,
is written so both can be consumed verbatim for that purpose, including the
EF Core version(s), provider (SQLite here), and OS (Ubuntu 24.04.3 LTS)
required by AC#11.

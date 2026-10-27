# EF Core #36741 repro: compiled `ExecuteUpdate`/`ExecuteDelete` translation failure

Reproduces a regression where a compiled query (`EF.CompileQuery` /
`EF.CompileAsyncQuery`) whose terminal operator is `ExecuteUpdate` or
`ExecuteDelete` throws `InvalidOperationException: ... could not be
translated`, while the identical query written inline as
`ExecuteUpdateAsync`/`ExecuteDeleteAsync` works. Attributed by the
maintainer to the query pipeline rework in #32018.

This project is deliberately isolated from the repository root's
Arcade/global.json/central-package-management/NuGet.config so it can be
built against an arbitrary EF Core version with the system-installed SDK,
independent of the repo's own pinned (and here, unresolvable) SDK.

## Isolation design

- `global.json` in this directory pins SDK `10.0.113` (`rollForward:
  latestFeature`), overriding the repo root's `global.json` (which pins a
  12.0.100 alpha SDK via `.dotnet`/`$host$` paths that don't exist in this
  worktree). **`global.json` resolution is driven by the process's current
  working directory, not the project file path** — every `dotnet` command
  below must be run with CWD inside this directory (`repro/efcore-36741/`),
  not the repo root.
- `Directory.Build.props`, `Directory.Build.targets`, and
  `Directory.Packages.props` in this directory stop MSBuild's upward
  directory walk, so the repo root's versions of those files (which set
  `ManagePackageVersionsCentrally=true` and other repo-wide settings) are
  never imported.
- `NuGet.config` in this directory clears all package sources and adds only
  `nuget.org`, so the repo root's `NuGet.config` (which clears sources and
  does *not* list nuget.org) is never consulted for this project.
- `Directory.Build.props` parameterizes `BaseIntermediateOutputPath` /
  `BaseOutputPath` by an `EFCoreVersion` MSBuild property (default `10.0.9`,
  overridable via `-p:EFCoreVersion=<version>`), producing
  `obj/<version>/` and `bin/<version>/` per version. This gives each EF
  Core version its own NuGet restore state (`project.assets.json`,
  `*.nuget.g.props/.targets`, `project.nuget.cache`,
  `*.nuget.dgspec.json`) and compiled output, so restoring/building one
  version never clobbers or gets contaminated by another's.

### Two defects found and fixed during planning (do not reintroduce)

1. **Setting `BaseIntermediateOutputPath`/`BaseOutputPath` in the `.csproj`
   body does not work.** It emits `warning MSB3539` and only isolates
   compiled output — NuGet writes `project.assets.json`,
   `*.nuget.g.props/.targets`, `project.nuget.cache`, and
   `*.nuget.dgspec.json` to `MSBuildProjectExtensionsPath`, which is fixed
   *before* `Microsoft.Common.props` is imported (and before the `.csproj`
   body's `PropertyGroup` is evaluated), so those files land in the shared
   `obj/` root regardless of what the `.csproj` body says. Fix: set these
   properties in `Directory.Build.props` instead, which is imported early
   enough to affect `MSBuildProjectExtensionsPath`. `EfCore36741Repro.csproj`
   deliberately does **not** set `BaseIntermediateOutputPath`,
   `BaseOutputPath`, or `EFCoreVersion` — all three come from
   `Directory.Build.props`.
2. **Once `obj/` has version-qualified subdirectories, a sibling version's
   stale `*.AssemblyInfo.cs` gets compiled in.** The SDK's default compile
   glob excludes only the *current* `BaseIntermediateOutputPath`, so once
   `obj/10.0.9/Debug/net10.0/*.AssemblyInfo.cs` and
   `obj/10.0.14/Debug/net10.0/*.AssemblyInfo.cs` both exist, building either
   version implicitly globs in the other's generated
   `AssemblyInfo.cs`, producing `error CS0579: Duplicate
   'TargetFrameworkAttribute' attribute`. Fix:
   `Directory.Build.props` also appends `obj/**;bin/**` to
   `DefaultItemExcludes` (excluding *all* version subfolders, not just the
   current one).

## Running

All commands must be run with the working directory inside
`repro/efcore-36741/` (so `global.json` resolves to the SDK pinned here).

**Baseline (confirms the regression on EF Core 10.0.9):**

```bash
cd repro/efcore-36741
./run-matrix.sh 10.0.9 baseline
```

Expected on 10.0.9 (per AC#7): the inline `ExecuteUpdateAsync` test and its
parameterized-filter variant pass; the three compiled-query tests (async
compiled `ExecuteUpdate`, sync compiled `ExecuteUpdate`, compiled
`ExecuteDelete`) fail with the translation exception.

**Fixed build (confirms the fix, no test-code changes):**

```bash
cd repro/efcore-36741
./run-matrix.sh <fixed-version> fixed
```

Expected (per AC#9): all tests pass on SQLite with the exact same test
code, by re-running with a different `-p:EFCoreVersion=<fixed-version>`
value.

**Provider filtering** (optional third+ argument(s), matched against a
`Provider` trait on the tests):

```bash
./run-matrix.sh 10.0.9 baseline Sqlite
./run-matrix.sh 10.0.9 baseline Sqlite SqlServer
```

Output is written to `results/raw/<version>-<label>.log` (console output)
and `results/raw/<version>-<label>.trx` (TRX). `run-matrix.sh` exits with
`dotnet test`'s own exit code — it does not swallow failures, since a
failing baseline run is the expected, correct outcome.

**Isolation check** (confirms per-version restore isolation; see below for
the run and result recorded during this task):

```bash
cd repro/efcore-36741
dotnet build -p:EFCoreVersion=10.0.9
dotnet build -p:EFCoreVersion=10.0.14
ls obj/10.0.9/project.assets.json obj/10.0.14/project.assets.json
grep -c '"Microsoft.EntityFrameworkCore.Sqlite/10.0.9"' obj/10.0.9/project.assets.json
grep -c '"Microsoft.EntityFrameworkCore.Sqlite/10.0.14"' obj/10.0.14/project.assets.json
ls obj/10.0.9/EfCore36741Repro.csproj.nuget.g.props obj/10.0.14/EfCore36741Repro.csproj.nuget.g.props
find obj -maxdepth 1 -type f   # must print nothing -- only version subdirectories allowed
```

## Isolation check — result recorded for this task (T1)

Environment: system SDKs `8.0.132` and `10.0.113` (runtimes `8.0.32` and
`10.0.13`); this directory's `global.json` resolves `dotnet` to `10.0.113`
when CWD is inside `repro/efcore-36741/` (verified via `dotnet --version`
run from this directory).

Ran, sequentially, from `repro/efcore-36741/`:

```
dotnet build -p:EFCoreVersion=10.0.9
dotnet build -p:EFCoreVersion=10.0.14
```

Results:

- Both builds: **"Build succeeded"**, **0 Errors**. No `MSB3539` warning in
  either build's output. No `CS0579` error in either build's output. (The
  only warnings present are unrelated `NU1902`/`NU1903` package-vulnerability
  advisories from transitive dependencies.)
- `obj/10.0.9/project.assets.json` and `obj/10.0.14/project.assets.json`
  both exist as separate files (sizes ~151 KB / ~153 KB respectively).
- `grep -c '"Microsoft.EntityFrameworkCore.Sqlite/10.0.9"' obj/10.0.9/project.assets.json`
  → `2` (present). `grep -c '"Microsoft.EntityFrameworkCore.Sqlite/10.0.14"' obj/10.0.14/project.assets.json`
  → `2` (present). Each restore pinned its own requested EF Core Sqlite
  version.
- `obj/10.0.9/EfCore36741Repro.csproj.nuget.g.props` and
  `obj/10.0.14/EfCore36741Repro.csproj.nuget.g.props` both exist.
- `find obj -maxdepth 1 -type f` printed nothing — `obj/` contains only the
  two version subdirectories (`10.0.9/`, `10.0.14/`), no
  `project.assets.json` or `*.nuget.g.props` at the unqualified `obj/` root.
- Compiled output likewise landed under `bin/10.0.9/Debug/net10.0/` and
  `bin/10.0.14/Debug/net10.0/` respectively, confirming output-path
  isolation in addition to restore-state isolation.

**Conclusion: isolation check PASSED.** T4/T5 can trust that a run against
`-p:EFCoreVersion=X` restores, builds, and executes against exactly EF Core
`X`'s packages, with no cross-version contamination.

(No test files exist yet at the time this check was run — T1 only owns the
build/restore scaffolding. The check above validates restore/build
isolation only; it does not run the test suite, which is added by T2/T3 and
executed by T4/T5.)

## Scope note on providers

SQL Server (via Docker) and Npgsql are wired into the project file as
version-parameterized package references for breadth, per the plan's
scope, but whether they can actually be exercised depends on Docker
availability in the environment running the suite. Per the plan, a
provider that could not be run must be reported as **skipped**, not
silently omitted, in the final report posted to #36741.

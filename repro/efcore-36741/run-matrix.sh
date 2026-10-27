#!/usr/bin/env bash
# Drives the EF Core 36741 repro test suite at an arbitrary EF Core version.
#
# Usage:
#   ./run-matrix.sh <efcore-version> <run-label> [provider ...]
#
# Examples:
#   ./run-matrix.sh 10.0.9 baseline
#   ./run-matrix.sh 10.0.14 fixed Sqlite
#   ./run-matrix.sh 10.0.9 baseline Sqlite SqlServer
#
# <efcore-version>  Version applied to the EF Core PackageReferences via
#                    -p:EFCoreVersion=<version>. Also used as the folder name
#                    under obj/ and bin/ (see Directory.Build.props) so that
#                    restoring/building one version never touches another's
#                    restore state.
# <run-label>        Caller-supplied label (e.g. "baseline", "fixed"). Used
#                    only to name the output files below -- this script has
#                    no clock dependency.
# [provider ...]     Optional list of provider names (e.g. Sqlite, SqlServer,
#                    Npgsql) to filter the run to, via `dotnet test --filter`
#                    on a "Provider" trait. If omitted, all tests run.
#
# Output:
#   results/raw/<efcore-version>-<run-label>.log   (full dotnet test console output)
#   results/raw/<efcore-version>-<run-label>.trx   (TRX test results file)
#
# Exit code: the exit code of `dotnet test`. This script does NOT swallow
# failures -- a failing baseline run (EF Core 10.0.9) is the expected,
# correct outcome for that run, and callers rely on seeing it fail.

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [[ $# -lt 2 ]]; then
  echo "Usage: $0 <efcore-version> <run-label> [provider ...]" >&2
  exit 2
fi

EFCORE_VERSION="$1"
RUN_LABEL="$2"
shift 2
PROVIDERS=("$@")

RESULTS_DIR="results/raw"
mkdir -p "$RESULTS_DIR"

LOG_FILE="$RESULTS_DIR/${EFCORE_VERSION}-${RUN_LABEL}.log"
TRX_FILE="${EFCORE_VERSION}-${RUN_LABEL}.trx"

FILTER_ARGS=()
if [[ ${#PROVIDERS[@]} -gt 0 ]]; then
  FILTER_EXPR=""
  for p in "${PROVIDERS[@]}"; do
    if [[ -z "$FILTER_EXPR" ]]; then
      FILTER_EXPR="Provider=$p"
    else
      FILTER_EXPR="$FILTER_EXPR|Provider=$p"
    fi
  done
  FILTER_ARGS=(--filter "$FILTER_EXPR")
fi

echo "Running dotnet test with EFCoreVersion=${EFCORE_VERSION} (label: ${RUN_LABEL})" | tee "$LOG_FILE"
if [[ ${#PROVIDERS[@]} -gt 0 ]]; then
  echo "Provider filter: ${PROVIDERS[*]}" | tee -a "$LOG_FILE"
fi

dotnet test \
  -p:EFCoreVersion="${EFCORE_VERSION}" \
  "${FILTER_ARGS[@]}" \
  --logger "trx;LogFileName=${TRX_FILE}" \
  --results-directory "$RESULTS_DIR" \
  2>&1 | tee -a "$LOG_FILE"

# Capture dotnet test's exit code, not tee's.
EXIT_CODE="${PIPESTATUS[0]}"

echo "Exit code: ${EXIT_CODE}" | tee -a "$LOG_FILE"

exit "$EXIT_CODE"

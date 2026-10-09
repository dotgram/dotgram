#!/usr/bin/env bash
# The whole measurement, one command: emit.sh, then prepare-analysis.sh + analyze.sh for each
# of the five packages (Finance's own extra reference on DotGram.ExpressionLanguage included),
# producing a SARIF per package under <scratch-dir>/sarif/. The CI workflow
# (.github/workflows/build.yml's generated-analysis job) calls this same script, so a local
# run and CI measure identically -- the per-package loop used to be duplicated inline in the
# workflow YAML, which is exactly the kind of place a fix lands in only one of the two copies.
#
# Usage: run-all.sh <repository-root> <scratch-dir>
#
# Leaves <scratch-dir>/emit/ (emit.sh's own output), <scratch-dir>/analysis/<pkg>/ (one
# throwaway Analysis.csproj per package) and <scratch-dir>/sarif/<pkg>.sarif behind; the
# caller reads the SARIF with SarifGate (check, to gate a build; update, to refresh
# tools/generated-analysis/counts.txt after fixing something -- docs/development.md).
set -euo pipefail

if [ $# -ne 2 ]; then echo "usage: run-all.sh <repository-root> <scratch-dir>" >&2; exit 2; fi

HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
ROOT=$(cd "$1" && pwd)
SCRATCH=$(mkdir -p "$2" && cd "$2" && pwd)

"$HERE/emit.sh" "$ROOT" "$SCRATCH/emit"

declare -A PREFIX=(
	[Sql]=DotGram.Sql
	[Web]=DotGram.Web
	[Finance]=DotGram.Finance
	[ExpressionLanguage]=DotGram.ExpressionLanguage
	[Examples]=DotGram.Examples
)

for pkg in Sql Web Finance ExpressionLanguage Examples; do
	extra=()
	if [ "$pkg" = "Finance" ]; then
		# DotGram.Finance's Fix/FixValidator.Load.cs calls DotGram.ExpressionLanguage for real
		# (a genuine ProjectReference in the shipped package, not just the analyzer), which the
		# throwaway Analysis.csproj needs as an ordinary reference to resolve (prepare-analysis.sh's
		# own usage comment).
		extra=("$ROOT/.build/bin/DotGram.ExpressionLanguage/release_net10.0/DotGram.ExpressionLanguage.dll")
	fi

	"$HERE/prepare-analysis.sh" "$pkg" "${PREFIX[$pkg]}" "$ROOT" "$SCRATCH/emit" \
		"$SCRATCH/analysis/$pkg" "$SCRATCH/sarif/$pkg.sarif" "${extra[@]}"
	"$HERE/analyze.sh" "$pkg" "$SCRATCH/analysis/$pkg" "$SCRATCH/sarif/$pkg.sarif"
done

echo "run-all: five SARIF(s) ready under $SCRATCH/sarif/"

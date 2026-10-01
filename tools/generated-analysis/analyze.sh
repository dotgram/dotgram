#!/usr/bin/env bash
# The CI job's own step 3: build the analysis project prepare-analysis.sh wrote, producing a
# SARIF errorlog. TreatWarningsAsErrors is false in the template, so dotnet build still exits
# non-zero only on a real compile error in the copy (a missing reference, a type the copied
# .g.cs needs that is not in scope), never on a style finding; "|| true" below deliberately
# does not fail THIS script on that, because the SARIF is still written and still worth
# reading -- SarifGate itself is what must catch a compile error (it reads the SARIF's own
# "level":"error" results explicitly, docs/development.md), not an exit code this script
# would otherwise throw away the detail of.
#
# Usage: analyze.sh <package> <analysis-dir> <sarif-path>
set -euo pipefail

if [ $# -ne 3 ]; then
	echo "usage: analyze.sh <package> <analysis-dir> <sarif-path>" >&2
	exit 2
fi

PKG=$1 DIR=$2 SARIF=$3

echo "== analyze: $PKG =="
dotnet build "$DIR/Analysis.csproj" -c Release -nodeReuse:false || true

if [ ! -f "$SARIF" ]; then
	echo "analyze: $PKG: no SARIF produced at $SARIF -- treat as a build failure, not zero findings" >&2
	exit 1
fi

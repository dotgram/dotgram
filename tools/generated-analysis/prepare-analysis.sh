#!/usr/bin/env bash
# The CI job's own step 2: one analysis project per package.
#
# Copies the emitted .g.cs that belong to this package (filtered by filename prefix, since a
# multi-project -t:Rebuild also emits generated files for whatever the package depends on --
# DotGram.Finance's own build emits DotGram.ExpressionLanguage's files too, which are not
# Finance's to analyse) into <analysis-dir>/gen/, header-stripped, writes .editorconfig and
# Analysis.csproj from the templates beside this script, referencing the package's own sources
# in place from the repository.
#
# Usage: prepare-analysis.sh <package> <prefix> <repository-root> <scratch-emit-dir> <analysis-dir> <sarif-path> [extra-ref.dll ...]
#
# A package whose hand-written sources call into another DotGram package at compile time
# (DotGram.Finance's Fix/FixValidator.Load.cs calls DotGram.ExpressionLanguage, a real
# ProjectReference in the product, not just the analyzer) needs that other assembly's
# already-built .dll passed as one or more trailing arguments.
set -euo pipefail

if [ $# -lt 6 ]; then
	echo "usage: prepare-analysis.sh <package> <prefix> <repository-root> <scratch-emit-dir> <analysis-dir> <sarif-path> [extra-ref.dll ...]" >&2
	exit 2
fi

PKG=$1 PREFIX=$2 ROOT=$3 SCRATCH_EMIT=$4 OUT=$5 SARIF=$6
shift 6
EXTRA_REFS=("$@")
HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
TEMPLATES=$HERE/templates

# Resolved to an absolute path, the same as emit.sh does for its own copy of this same argument:
# Analysis.csproj's {{SRC_ROOT}} is an MSBuild <Compile Include> path, which MSBuild resolves
# against the PROJECT FILE's own directory (OUT, a scratch directory), never against whatever
# directory the caller's shell happened to be in -- a relative "." here silently compiled gen/
# alone, zero hand-written sources, no error, no warning, and the real counts this job exists to
# gate on were a few thousand diagnostics short of what the hand-written half alone carries.
ROOT=$(cd "$ROOT" && pwd)

declare -A SRC_DIRS=(
	[Sql]="$ROOT/src/DotGram.Sql"
	[Web]="$ROOT/src/DotGram.Web"
	[Finance]="$ROOT/src/DotGram.Finance"
	[ExpressionLanguage]="$ROOT/src/DotGram.ExpressionLanguage"
	[Examples]="$ROOT/examples/DotGram.Examples"
)
SRC_ROOT=${SRC_DIRS[$PKG]}
GEN_SRC="$SCRATCH_EMIT/$PKG/gen/DotGram/DotGram.Generation.GramGenerator"

mkdir -p "$OUT/gen"
rm -f "$OUT"/gen/*.g.cs
mkdir -p "$(dirname "$SARIF")"

shopt -s nullglob
copied=0
for f in "$GEN_SRC"/*.g.cs; do
	name=$(basename "$f")
	case "$name" in
		*.DotGramReport.g.cs) continue ;;
	esac
	if [ "$name" = "DotGram.Attributes.g.cs" ] || [[ "$name" == "$PREFIX"* ]]; then
		# Strip the auto-generated header (line 1) so neither the header comment nor (see
		# .editorconfig) the *.g.cs filename pattern makes analyzers skip the file.
		tail -n +2 "$f" > "$OUT/gen/$name"
		if [ "$name" = "DotGram.Attributes.g.cs" ]; then
			# [global::Microsoft.CodeAnalysis.Embedded] only marks these types compiler/IDE
			# internal; this ad-hoc project has no Microsoft.CodeAnalysis reference to resolve
			# the name against (CS0234), and the marker affects only IDE-hidden-ness, not any
			# rule this job runs, so it is dropped from this copy only.
			sed -i '/\[global::Microsoft\.CodeAnalysis\.Embedded\]/d' "$OUT/gen/$name"
		fi
		copied=$((copied + 1))
	fi
done
echo "$PKG: copied $copied generated file(s) into $OUT/gen (prefix '$PREFIX')"
if [ "$copied" -eq 0 ]; then
	echo "prepare-analysis: no generated files matched prefix '$PREFIX' under $GEN_SRC -- did emit.sh run for $PKG?" >&2
	exit 1
fi

EXTRA_XML=$(mktemp)
if [ ${#EXTRA_REFS[@]} -gt 0 ]; then
	{
		printf '\t<ItemGroup>\n'
		printf '\t\t<!-- Real (non-analyzer) references the package own hand-written sources need at compile time, passed in by the caller (see script usage). -->\n'
		for dll in "${EXTRA_REFS[@]}"; do
			# Same MSBuild-resolves-against-the-project-file rule as SRC_ROOT above: a relative
			# path the caller wrote against the REPOSITORY root (as the usage comment and every
			# caller so far do) is made absolute against $ROOT here, or Analysis.csproj's own
			# HintPath would look for it beside itself in the scratch directory and silently
			# fail the build (CS0234) rather than find a real DLL that does exist.
			case "$dll" in
				/*) resolved=$dll ;;
				*)  resolved=$ROOT/$dll ;;
			esac
			printf '\t\t<Reference Include="%s"><HintPath>%s</HintPath></Reference>\n' "$(basename "$resolved" .dll)" "$resolved"
		done
		printf '\t</ItemGroup>\n'
	} > "$EXTRA_XML"
fi

sed \
	-e "s#{{PKG}}#$PKG#g" \
	-e "s#{{SRC_ROOT}}#$SRC_ROOT#g" \
	-e "s#{{SARIF}}#$SARIF#g" \
	-e "/{{EXTRA_REFS}}/r $EXTRA_XML" \
	-e "/{{EXTRA_REFS}}/d" \
	"$TEMPLATES/Analysis.csproj.template" > "$OUT/Analysis.csproj"
rm -f "$EXTRA_XML"

cp "$TEMPLATES/editorconfig.template" "$OUT/.editorconfig"

echo "$PKG: analysis project ready at $OUT/Analysis.csproj (sarif -> $SARIF)"

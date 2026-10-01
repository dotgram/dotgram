#!/usr/bin/env bash
# The CI job's own step 1: for each shipped package plus examples/DotGram.Examples, a clean
# `-t:Rebuild -p:EmitCompilerGeneratedFiles=true`, one project at a time -- the analyzer project
# (src/DotGram) races if built concurrently in one worktree by more than one of these. Also
# builds with -warnaserror -p:DotGramAnalyzeGenerated=true, so this same rebuild validates our
# own in-build analyzer (tools/DotGram.GeneratedAnalyzers' DGA001/DGA003/DGA004) too: there is
# no separate "ordinary build with the property on" step, since these five projects are the
# only ones that reference the analyzer at all, and this script was already rebuilding them.
#
# Usage: emit.sh <repository-root> <scratch-emit-dir>
set -euo pipefail

if [ $# -ne 2 ]; then echo "usage: emit.sh <repository-root> <scratch-emit-dir>" >&2; exit 2; fi

ROOT=$(cd "$1" && pwd)
SCRATCH=$(mkdir -p "$2" && cd "$2" && pwd)

declare -A PROJECTS=(
	[Sql]="src/DotGram.Sql/DotGram.Sql.csproj"
	[Web]="src/DotGram.Web/DotGram.Web.csproj"
	[Finance]="src/DotGram.Finance/DotGram.Finance.csproj"
	[ExpressionLanguage]="src/DotGram.ExpressionLanguage/DotGram.ExpressionLanguage.csproj"
	[Examples]="examples/DotGram.Examples/DotGram.Examples.csproj"
)

for pkg in Sql Web Finance ExpressionLanguage Examples; do
	proj="$ROOT/${PROJECTS[$pkg]}"
	out="$SCRATCH/$pkg/gen"
	echo "== emit: $pkg ($proj) =="
	dotnet build "$proj" -c Release -t:Rebuild -warnaserror \
		-p:EmitCompilerGeneratedFiles=true \
		-p:CompilerGeneratedFilesOutputPath="$out" \
		-p:DotGramAnalyzeGenerated=true \
		-nodeReuse:false
done

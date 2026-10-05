#!/usr/bin/env bash
# SPIKE (not for merging). Builds the DotGram.Finance DLL sets of the columns of the exposure-shape benchmark:
#
#     benchmarks/DotGram.Finance.ShapeSpike/build-columns.sh [column ...]      (default: A B C D F)
#
# For each column: regenerates the three versions with FIX_SHAPE=<column> (generate.cs), builds DotGram.Finance
# Release for both frameworks with -warnaserror, and keeps DotGram.Finance.dll and DotGram.ExpressionLanguage.dll
# under $COLUMNS/<column>/{net10.0,netstandard2.0}/, with the static measures in $COLUMNS/<column>/static.txt.
# Heavy: run it through /work/tools/quiet-run.sh, outside any timing window. Leaves the generated sources as
# the control (A) writes them, which is what is committed.
set -euo pipefail

root=$(cd "$(dirname "$0")/../.." && pwd)
columns=${COLUMNS_DIR:-/ramdisk/build/dotgram/fix-h1-spike/columns}
logs=${LOGS_DIR:-/ramdisk/logs}
export PATH=$HOME/.dotnet:$PATH
export MSBUILDDISABLENODEREUSE=1
cd "$root"

list=("$@")
[ ${#list[@]} -gt 0 ] || list=(A B C D F)

for col in "${list[@]}"; do
	echo "== column $col"
	start=$(date +%s)
	FIX_SHAPE=$col dotnet run src/DotGram.Finance/Fix/generate.cs > "$logs/fix-h1-spike-gen-$col.log" 2>&1
	rm -rf src/DotGram.Finance/obj src/DotGram.Finance/bin
	dotnet build src/DotGram.Finance/DotGram.Finance.csproj -c Release -p:FixShape=$col -warnaserror -nodeReuse:false \
		> "$logs/fix-h1-spike-build-$col.log" 2>&1 || { echo "build of $col failed, see $logs/fix-h1-spike-build-$col.log" >&2; exit 1; }
	seconds=$(( $(date +%s) - start ))

	for asset in net10.0 netstandard2.0; do
		mkdir -p "$columns/$col/$asset"
		cp src/DotGram.Finance/bin/Release/$asset/DotGram.Finance.dll "$columns/$col/$asset/"
		cp src/DotGram.ExpressionLanguage/bin/Release/$asset/DotGram.ExpressionLanguage.dll "$columns/$col/$asset/"
	done

	{
		echo "column $col"
		for v in Fix42 Fix44 Fix50; do
			all=$(cat src/DotGram.Finance/Fix/$v/*.cs | wc -c)
			msg=$(cat src/DotGram.Finance/Fix/$v/FixMessage.Types.cs src/DotGram.Finance/Fix/$v/FixMessage.Header.cs $(ls src/DotGram.Finance/Fix/$v/FixComponents.cs 2>/dev/null) | wc -c)
			echo "generated $v all=$all messages=$msg"
		done
		for asset in net10.0 netstandard2.0; do
			echo "dll $asset $(stat -c %s "$columns/$col/$asset/DotGram.Finance.dll")"
		done
		echo "generate+build seconds $seconds (one reading, informational)"
	} > "$columns/$col/static.txt"
done

# the tree as committed: the control's sources
FIX_SHAPE=A dotnet run src/DotGram.Finance/Fix/generate.cs > "$logs/fix-h1-spike-gen-A-restore.log" 2>&1
rm -rf src/DotGram.Finance/obj src/DotGram.Finance/bin
echo "done: $columns"

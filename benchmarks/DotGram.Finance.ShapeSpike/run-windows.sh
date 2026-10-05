#!/usr/bin/env bash
# SPIKE (not for merging). Runs one window of the exposure-shape benchmark, or prepares and dry-runs it.
#
#   benchmarks/DotGram.Finance.ShapeSpike/run-windows.sh build            outside any window: builds the launcher
#   benchmarks/DotGram.Finance.ShapeSpike/run-windows.sh dry              outside any window: --job dry, every row, every
#                                                                         column (A1 A2 B C D F), net10.0 then net8.0
#   /work/tools/quiet-hold.sh "fix-h1: W1, until ~HH:MM" benchmarks/DotGram.Finance.ShapeSpike/run-windows.sh W1
#
# Windows (columns A1 A2 B C F; add D with SPIKE_COLUMNS=A1,A2,B,C,D,F):
#   W1  net10.0                       Fix44, every row (the rows of category W1)           13 rows
#   W2  net10.0                       Fix42 and Fix50: P2, E1, E2                           6 rows
#   W3  net8.0 (netstandard2.0 asset) Fix44 I1-I7, E1, E2                                   9 rows
#   W4  net10.0, DOTNET_TieredPGO=0   Fix44 I1-I5, I7                                       6 rows (allocation is the gate)
# --launchCount 3 --warmupCount 5 --iterationCount 15, every job; the process runs on processors 0-7 and 16-23
# (the stand's half: the first CCD with its siblings), through taskset. BenchmarkDotNet builds the benchmark
# once per column at the start of the run (that build is inside the window; the DotGram.Finance builds are not:
# they are the column DLLs under $COLUMNS_DIR, made beforehand by build-columns.sh).
#
# Results land in $OUT/<window>/ (default /ramdisk/build/dotgram/fix-h1-spike/results): BenchmarkDotNet's artifacts
# (*-report-full.json, *.csv, *-github.md), the log of the run, and run.txt with what was run.
set -euo pipefail

window=${1:?usage: run-windows.sh build|dry|W1|W2|W3|W4}
root=$(cd "$(dirname "$0")/../.." && pwd)
project=$root/benchmarks/DotGram.Finance.ShapeSpike/DotGram.Finance.ShapeSpike.csproj
dll=$root/benchmarks/DotGram.Finance.ShapeSpike/bin/Release/net10.0/DotGram.Finance.ShapeSpike.dll
columns_dir=${COLUMNS_DIR:-/ramdisk/build/dotgram/fix-h1-spike/columns}
out=${OUT:-/ramdisk/build/dotgram/fix-h1-spike/results}
cpus=${DOTGRAM_STAND_CPUS:-0-7,16-23}
cols=${SPIKE_COLUMNS:-A1,A2,B,C,F}
export PATH=$HOME/.dotnet:$PATH
export MSBUILDDISABLENODEREUSE=1
cd "$root"

if [ "$window" = build ]; then
	/work/tools/quiet-run.sh dotnet build "$project" -c Release -warnaserror -nodeReuse:false
	exit
fi

[ -f "$dll" ] || { echo "run-windows.sh: build the launcher first (run-windows.sh build)" >&2; exit 2; }
for c in ${cols//,/ }; do
	[ -f "$columns_dir/${c:0:1}/net10.0/DotGram.Finance.dll" ] || { echo "run-windows.sh: no column ${c:0:1} under $columns_dir" >&2; exit 2; }
done

mkdir -p "$out/$window"

# what BenchmarkDotNet does not say: which commit, which column DLLs
{
	echo "window $window, started $(date -Is)"
	echo "commit $(git rev-parse HEAD)"
	echo "columns $cols, processors $cpus"
	echo "env DOTNET_TieredPGO=${DOTNET_TieredPGO:-unset}"
	for f in "$columns_dir"/*/*/DotGram.Finance.dll; do
		echo "$(sha256sum "$f" | cut -c1-16) $(stat -c %s "$f") $f"
	done
} > "$out/$window/run.txt"

common=(--columns "$cols" --columns-dir "$columns_dir" --artifacts "$out/$window" --buildTimeout 900)

run() {
	# a dry run is not a timing: nothing pins it
	if [ "$window" = dry ]; then
		dotnet "$dll" "$@"
	else
		taskset -c "$cpus" dotnet "$dll" "$@"
	fi
}

status=0
case $window in
	dry)
		for rt in net10.0 net8.0; do
			echo "== dry, $rt"
			run --job dry --runtime $rt --columns "A1,A2,B,C,D,F" --columns-dir "$columns_dir" --artifacts "$out/dry-$rt" --buildTimeout 900 --filter '*' || status=$?
		done
		;;
	W1) run --runtime net10.0 --anyCategories W1 "${common[@]}" 2>&1 | tee "$out/$window/run.log"; status=${PIPESTATUS[0]} ;;
	W2) run --runtime net10.0 --anyCategories W2 "${common[@]}" 2>&1 | tee "$out/$window/run.log"; status=${PIPESTATUS[0]} ;;
	W3) run --runtime net8.0  --anyCategories W3 "${common[@]}" 2>&1 | tee "$out/$window/run.log"; status=${PIPESTATUS[0]} ;;
	W4) run --runtime net10.0 --pgo 0 --anyCategories W4 "${common[@]}" 2>&1 | tee "$out/$window/run.log"; status=${PIPESTATUS[0]} ;;
	*) echo "unknown window $window" >&2; exit 2 ;;
esac

echo "window $window ended $(date -Is), exit $status" >> "$out/$window/run.txt"
exit $status

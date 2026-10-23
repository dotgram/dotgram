#!/usr/bin/env bash
# run-gates.sh: the release gates of DotGram.Cli, on the shipped grammar projects of this repository.
#
#     tests/DotGram.Cli.Tests/gates/run-gates.sh [--keep] [--no-g3] [--no-g9] [--runs <n>]
#
# It tests the commit checked out (HEAD) in a worktree of its own, so the checkout it is started from is not
# touched and uncommitted changes are not part of the run. Everything it writes is in one scratch folder,
# $DOTGRAM_GATES_DIR or a new one under $TMPDIR, deleted at the end (--keep keeps it). docs/development.md,
# "The tool's release gates", says what each gate holds and what to do when one fails.
#
#   G2   the generator hosted by hand over the compiler arguments of a real build (diagnostic: it isolates the
#        hosting from the tool's reading of the project)
#   G2b  the README's command, `dotnet dotgram generate src/<P>/<P>.csproj -o ...`, run with the tool installed
#        from its package, byte-equal to what `dotnet build` generates, for every framework of every project;
#        also with --line-paths absolute and with --framework for each framework
#   G3   DotGram.Sql, DotGram.Web, DotGram.ExpressionLanguage and DotGram.Finance built from the tool's output
#        with the generator's reference removed, both frameworks, and their test projects green
#   G9   wall time and peak memory of the tool on each project, and of the generator in a cold build of
#        DotGram.Sql: directional unless the machine is quiet
#
# Exit 0 when every gate passed, 1 when one failed, 2 on a usage or setup error.
set -euo pipefail

keep=0; g3=1; g9=1; runs=3
while [ $# -gt 0 ]; do
	case "$1" in
		--keep) keep=1 ;;
		--no-g3) g3=0 ;;
		--no-g9) g9=0 ;;
		--runs) runs=$2; shift ;;
		-h|--help) sed -n '2,22p' "$0"; exit 0 ;;
		*) echo "run-gates.sh: unknown argument '$1'" >&2; exit 2 ;;
	esac
	shift
done

here=$(cd "$(dirname "$0")" && pwd)
repo=$(git -C "$here" rev-parse --show-toplevel)
commit=$(git -C "$repo" rev-parse --short HEAD)

if [ -n "${DOTGRAM_GATES_DIR:-}" ]; then
	work=$DOTGRAM_GATES_DIR
	mkdir -p "$work"
else
	work=$(mktemp -d "${TMPDIR:-/tmp}/dotgram-gates.XXXXXX")
fi
work=$(cd "$work" && pwd)
tree=$work/tree
logs=$work/logs
mkdir -p "$logs" "$work/tmp" "$work/built"

cleanup() {
	set +e
	cd /
	if [ -d "$tree" ]; then git -C "$repo" worktree remove --force "$tree" > /dev/null 2>&1; fi
	git -C "$repo" worktree prune > /dev/null 2>&1
	if [ "$keep" = 1 ]; then echo "kept: $work"; else rm -rf "$work"; fi
}
trap cleanup EXIT

# Nothing of MSBuild's outlives a step, and nothing the steps write goes outside the scratch folder.
export MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 TMPDIR=$work/tmp

results=()
failed=0
result() { # gate subject verdict [detail]
	results+=("$(printf '%-4s %-34s %-5s %s' "$1" "$2" "$3" "${4:-}")")
	if [ "$3" = FAIL ]; then failed=1; fi
}
step() { echo; echo "== $*"; }
die() { echo "run-gates.sh: $*" >&2; exit 2; }

step "worktree of $commit in $tree"
git -C "$repo" worktree add --detach "$tree" HEAD > "$logs/worktree.log" 2>&1 || { cat "$logs/worktree.log"; die "git worktree add failed"; }
cd "$tree"

# Every shipped project that compiles a grammar: the generator among its analyzers. Referenced projects
# come first (Finance takes ExpressionLanguage).
projects=(DotGram.ExpressionLanguage)
for csproj in src/*/*.csproj; do
	name=$(basename "$csproj" .csproj)
	case "$name" in DotGram|DotGram.VisualStudio|DotGram.ExpressionLanguage) continue ;; esac
	if grep -Pzq 'DotGram[\\/]DotGram\.csproj"[^>]*OutputItemType="Analyzer"' "$csproj"; then projects+=("$name"); fi
done
echo "grammar projects: ${projects[*]}"

frameworks() { # project -> its target frameworks, one per line
	local several one
	several=$(dotnet msbuild "src/$1/$1.csproj" -nologo -getProperty:TargetFrameworks -p:Configuration=Release)
	one=$(dotnet msbuild "src/$1/$1.csproj" -nologo -getProperty:TargetFramework -p:Configuration=Release)
	echo "${several:-$one}" | tr ';' '\n' | sed 's/^ *//; s/ *$//; /^$/d'
}

step "the tool, from its package"
dotnet build src/DotGram.Cli/DotGram.Cli.csproj -c Release -nodeReuse:false > "$logs/tool-build.log" 2>&1 || { tail -20 "$logs/tool-build.log"; die "building the tool failed"; }
dotnet pack src/DotGram.Cli/DotGram.Cli.csproj -c Release --no-build -o "$work/feed" > "$logs/tool-pack.log" 2>&1 || { tail -20 "$logs/tool-pack.log"; die "packing the tool failed"; }
package=$(basename "$(ls "$work"/feed/DotGram.Cli.*.nupkg)" .nupkg)
version=${package#DotGram.Cli.}
cat > "$work/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="gates" value="$work/feed" />
  </packageSources>
</configuration>
EOF
# Installed into a folder of its own, from a feed that has nothing else, with a packages folder of its own:
# a package of the same version in the machine's cache or on nuget.org is not what is tested.
NUGET_PACKAGES=$work/nuget dotnet tool install DotGram.Cli --version "$version" --tool-path "$work/tool" --configfile "$work/nuget.config" > "$logs/tool-install.log" 2>&1 \
	|| { cat "$logs/tool-install.log"; die "installing the tool failed"; }
export PATH=$work/tool:$PATH
dotnet dotgram --version
echo "nupkg: $(du -k "$work/feed/$package.nupkg" | cut -f1) KB"

step "the comparisons (gates/Gates.cs)"
roslyn=$(sed -n 's:.*<DotGramCliRoslynVersion>\(.*\)</DotGramCliRoslynVersion>.*:\1:p' Directory.Packages.props)
mkdir -p "$work/app"
sed "s/ROSLYN_VERSION/$roslyn/" "$here/Gates.cs" > "$work/app/Gates.cs"
cp global.json "$work/app/"
(cd "$work/app" && dotnet build Gates.cs --artifacts-path "$work/app/out" > "$logs/gates-app.log" 2>&1) || { tail -20 "$logs/gates-app.log"; die "building Gates.cs failed"; }
gates_dll=$(find "$work/app/out/bin" -name Gates.dll | head -1)
gates() { dotnet "$gates_dll" "$@"; }

step "restore"
for p in "${projects[@]}"; do
	dotnet restore "src/$p/$p.csproj" > "$logs/restore-$p.log" 2>&1 || { tail -20 "$logs/restore-$p.log"; die "restoring $p failed"; }
done

# What the build generates: each project and framework built on its own, the compiler forced to run (the
# project file touched), its generated files copied out of the project's own folder for them (a folder given
# on the command line would be every referenced project's too) and its command line written where G2 reads it.
emitted() { # project framework -> where the compiler writes the project's generated files
	local path
	path=$(dotnet msbuild "src/$1/$1.csproj" -nologo -getProperty:CompilerGeneratedFilesOutputPath -p:Configuration=Release -p:TargetFramework="$2" -p:EmitCompilerGeneratedFiles=true)
	case "$path" in /*) echo "$path" ;; *) echo "$tree/src/$1/$path" ;; esac
}
declare -A tfms
step "builds"
for p in "${projects[@]}"; do
	tfms[$p]=$(frameworks "$p" | tr '\n' ' ')
	for t in ${tfms[$p]}; do
		touch "src/$p/$p.csproj"
		emit=$(emitted "$p" "$t")
		rm -rf "$emit"
		if ! dotnet msbuild "src/$p/$p.csproj" -nologo -t:Build -p:Configuration=Release -p:TargetFramework="$t" -nodeReuse:false \
				-p:ProvideCommandLineArgs=true -p:EmitCompilerGeneratedFiles=true \
				-getItem:CscCommandLineArgs > "$work/built/$p.$t.args.json" 2> "$logs/build-$p-$t.log"; then
			tail -20 "$logs/build-$p-$t.log"; die "building $p for $t failed"
		fi
		mkdir -p "$work/built/$p"
		cp -r "$emit" "$work/built/$p/$t"
		echo "$p $t: $(ls "$work/built/$p/$t/DotGram/DotGram.Generation.GramGenerator" | wc -l) files"
	done
done

generated() { echo "$work/built/$1/$2/DotGram/DotGram.Generation.GramGenerator"; }

step "G2: the generator over the build's own compiler arguments"
for p in "${projects[@]}"; do
	for t in ${tfms[$p]}; do
		if gates g2 "$work/built/$p.$t.args.json" "$(generated "$p" "$t")" "$tree/src/$p" > "$logs/g2-$p-$t.log" 2>&1
		then result G2 "$p $t" pass "$(tail -1 "$logs/g2-$p-$t.log" | sed 's/^ *//')"
		else result G2 "$p $t" FAIL "$logs/g2-$p-$t.log"; cat "$logs/g2-$p-$t.log"
		fi
	done
done

# One run of the tool: what it printed goes to the log, the time and peak memory to <log>.time.
tool() { # log arguments...
	local log=$1; shift
	/usr/bin/time -f '%e %M' -o "$log.time" dotnet dotgram "$@" > "$log" 2>&1
}

step "G2b: --line-paths absolute, every framework at once and each alone"
for p in "${projects[@]}"; do
	out=$work/abs/$p
	if tool "$logs/g2b-abs-$p.log" generate "src/$p/$p.csproj" -o "$out" --line-paths absolute; then
		for t in ${tfms[$p]}; do
			if gates compare "$(generated "$p" "$t")" "$out" > "$logs/g2b-abs-$p-$t.cmp" 2>&1
			then result G2b "$p abs (all) vs $t" pass "$(tail -1 "$logs/g2b-abs-$p-$t.cmp" | sed 's/^ *//')"
			else result G2b "$p abs (all) vs $t" FAIL "$logs/g2b-abs-$p-$t.cmp"; cat "$logs/g2b-abs-$p-$t.cmp"
			fi
		done
	else
		result G2b "$p abs (all)" FAIL "the tool exited $?: $logs/g2b-abs-$p.log"; tail -20 "$logs/g2b-abs-$p.log"
	fi
	for t in ${tfms[$p]}; do
		out=$work/abs-$t/$p
		if tool "$logs/g2b-abs-$p-$t.log" generate "src/$p/$p.csproj" -o "$out" --line-paths absolute --framework "$t" \
				&& gates compare "$(generated "$p" "$t")" "$out" > "$logs/g2b-abs-$p-$t-alone.cmp" 2>&1
		then result G2b "$p abs --framework $t" pass
		else result G2b "$p abs --framework $t" FAIL "$logs/g2b-abs-$p-$t.log"; cat "$logs/g2b-abs-$p-$t-alone.cmp" 2>/dev/null || true
		fi
	done
done

if [ "$g9" = 1 ]; then
	step "G9: timings (load: $(cut -d' ' -f1-3 /proc/loadavg))"
	for p in "${projects[@]}"; do
		n=1; if [ "$p" = DotGram.Sql ]; then n=$runs; fi
		times=()
		for i in $(seq 1 "$n"); do
			rm -rf "$work/g9/$p"
			tool "$logs/g9-$p-$i.log" generate "src/$p/$p.csproj" -o "$work/g9/$p" || { result G9 "$p" FAIL "$logs/g9-$p-$i.log"; continue 2; }
			times+=("$(cat "$logs/g9-$p-$i.log.time")")
		done
		wall=$(printf '%s\n' "${times[@]}" | cut -d' ' -f1 | sort -n | tr '\n' ' ')
		rss=$(printf '%s\n' "${times[@]}" | cut -d' ' -f2 | sort -n | tail -1)
		result G9 "$p tool" time "wall s: $wall; peak RSS $((rss / 1024)) MB; ${tfms[$p]}"
	done
	# The generator in a cold compile of DotGram.Sql: a compiler process of its own, the generator's time as
	# the compiler reports it.
	for t in ${tfms[DotGram.Sql]:-}; do
		touch src/DotGram.Sql/DotGram.Sql.csproj
		dotnet msbuild src/DotGram.Sql/DotGram.Sql.csproj -nologo -t:Build -p:Configuration=Release -p:TargetFramework="$t" -nodeReuse:false \
			-p:UseSharedCompilation=false -p:ReportAnalyzer=true -v:d -clp:PerformanceSummary > "$logs/g9-csc-$t.log" 2>&1 || true
		gen=$(grep -m1 -o 'Total generator execution time: [0-9.]* seconds' "$logs/g9-csc-$t.log" | grep -o '[0-9.]*' | head -1)
		dotgram=$(grep -m1 -E '^\s*[0-9.<]+\s+[0-9]+\s+DotGram.Generation.GramGenerator' "$logs/g9-csc-$t.log" | awk '{print $1}')
		csc=$(grep -E '^\s*[0-9]+ ms\s+Csc\s' "$logs/g9-csc-$t.log" | tail -1 | awk '{print $1}')
		result G9 "DotGram.Sql cold csc $t" time "Csc task ${csc:-?} ms; generators ${gen:-?} s, of it GramGenerator ${dotgram:-?} s"
	done
fi

# The README's command exactly, from the repository root into the project's own Generated folder, and
# compared with the build's text with each relative #line path read against that folder. A project another
# one references goes last: once its folder holds the files, its build would generate them twice.
step "G2b: the README command"
order=()
for p in "${projects[@]}"; do [ "$p" = DotGram.ExpressionLanguage ] || order+=("$p"); done
order+=(DotGram.ExpressionLanguage)
for p in "${order[@]}"; do
	out=src/$p/Generated
	if tool "$logs/g2b-readme-$p.log" generate "src/$p/$p.csproj" -o "$out"; then
		for t in ${tfms[$p]}; do
			if gates compare "$(generated "$p" "$t")" "$out" --relative-to "$out" > "$logs/g2b-readme-$p-$t.cmp" 2>&1
			then result G2b "$p README vs $t" pass "$(tail -1 "$logs/g2b-readme-$p-$t.cmp" | sed 's/^ *//')"
			else result G2b "$p README vs $t" FAIL "$logs/g2b-readme-$p-$t.cmp"; cat "$logs/g2b-readme-$p-$t.cmp"
			fi
		done
	else
		result G2b "$p README" FAIL "the tool exited: $logs/g2b-readme-$p.log"; tail -20 "$logs/g2b-readme-$p.log"
	fi
done

# A grammar project without the generator: its reference removed, the files of the README command compiled
# as its own, the command run again as CI would (it must change nothing), both frameworks built, its test
# projects run.
declare -A tests_of=(
	[DotGram.Sql]="tests/DotGram.Sql.Tests"
	[DotGram.Web]="tests/DotGram.Tests"
	[DotGram.ExpressionLanguage]="tests/DotGram.Tests"
	[DotGram.Finance]="tests/DotGram.Finance.Tests"
)
if [ "$g3" = 1 ]; then
	step "G3: built from the tool's output, without the generator"
	for p in "${projects[@]}"; do
		if [ -z "${tests_of[$p]:-}" ]; then rm -rf "src/$p/Generated"; fi
	done
	# A test project here that compiles a grammar project's sources again, by a glob (the traced and tape
	# builds), would take its Generated folder too: it is excluded there, as such a project would.
	for f in $(grep -rlE 'src\\DotGram\.[A-Za-z]+\\\*\*' --include='*.csproj' --include='*.props' tests); do
		perl -pi -e 's{(Exclude="(?:[^"]*;)?)([^";]*src\\DotGram\.\w+\\)obj\\\*\*}{$1$2Generated\\**;$2obj\\**}g' "$f"
	done
	g3_projects=()
	for p in "${projects[@]}"; do if [ -n "${tests_of[$p]:-}" ]; then g3_projects+=("$p"); fi; done
	for p in "${g3_projects[@]}"; do
		perl -0pi -e 's{<ProjectReference Include="\.\.[\\/]DotGram[\\/]DotGram\.csproj"[^>]*/>}{}s' "src/$p/$p.csproj"
		if grep -q 'DotGram[\\/]DotGram\.csproj' "src/$p/$p.csproj"; then die "$p: the generator's reference was not removed"; fi
		cp -r "src/$p/Generated" "$work/before-$p"
	done
	for p in "${g3_projects[@]}"; do
		if ! tool "$logs/g3-again-$p.log" generate "src/$p/$p.csproj" -o "src/$p/Generated"; then
			result G3 "$p regenerated" FAIL "$logs/g3-again-$p.log"; tail -20 "$logs/g3-again-$p.log"
		elif diff -r "$work/before-$p" "src/$p/Generated" > "$logs/g3-again-$p.diff" 2>&1; then
			result G3 "$p regenerated" pass "no file changed"
		else
			result G3 "$p regenerated" FAIL "$logs/g3-again-$p.diff"; head -20 "$logs/g3-again-$p.diff"
		fi
	done
	for p in "${g3_projects[@]}"; do
		emitted=$(dotnet msbuild "src/$p/$p.csproj" -nologo -getProperty:CompilerGeneratedFilesOutputPath -p:Configuration=Release -p:TargetFramework=net10.0)
		case "$emitted" in /*) ;; *) emitted=src/$p/$emitted ;; esac
		rm -rf "$emitted/DotGram"
		if dotnet build "src/$p/$p.csproj" -c Release -nodeReuse:false > "$logs/g3-build-$p.log" 2>&1; then
			if [ -d "$emitted/DotGram" ]; then result G3 "$p build ${tfms[$p]}" FAIL "the generator ran: $emitted/DotGram"
			else result G3 "$p build ${tfms[$p]}" pass "no generator"
			fi
		else
			result G3 "$p build ${tfms[$p]}" FAIL "$logs/g3-build-$p.log"; grep -E 'error' "$logs/g3-build-$p.log" | sort -u | head -10
		fi
	done
	for t in $(printf '%s\n' "${tests_of[@]}" | sort -u); do
		name=$(basename "$t")
		if ! dotnet build "$t/$name.csproj" -c Release -nodeReuse:false > "$logs/g3-build-$name.log" 2>&1; then
			result G3 "$name" FAIL "build: $logs/g3-build-$name.log"; grep -E 'error' "$logs/g3-build-$name.log" | sort -u | head -10
			continue
		fi
		dotnet test --project "$t/$name.csproj" -c Release --no-build > "$logs/g3-test-$name.log" 2>&1 && code=0 || code=$?
		summary=$(grep -E '^\s*(total|failed|succeeded|skipped):' "$logs/g3-test-$name.log" | tr -s ' \n' ' ')
		total=$(grep -m1 -oE '^\s*total:\s*[0-9]+' "$logs/g3-test-$name.log" | grep -oE '[0-9]+' || echo 0)
		if [ "$code" = 0 ] && [ "${total:-0}" -gt 0 ]; then result G3 "$name" pass "$summary"
		else result G3 "$name" FAIL "exit $code; $summary $logs/g3-test-$name.log"; grep -E '^\s*failed ' "$logs/g3-test-$name.log" | head -20
		fi
	done
fi

echo
echo "== results ($commit, DotGram.Cli $version, SDK $(dotnet --version))"
printf '%s\n' "${results[@]}"
if [ "$failed" = 1 ]; then echo "FAILED: logs in $logs"; keep=1; exit 1; fi
echo "all gates passed"

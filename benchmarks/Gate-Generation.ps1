<#
.SYNOPSIS
	Holds the generator's time on the grammars to a base commit's, measured in the same run.

.DESCRIPTION
	The generator's time is machine-state dependent: the base of 2026-09-18 was read at 4.1 s on
	T-SQL at 01:49 and 5.0 s by the same commit rebuilt that evening. So a head is not held to a number
	written down on another day but to the base rebuilt here, alternately with it: base, head, base,
	head, on the stand's processors (taskset, D147), one build at a time, and the ratio of the medians is what is quoted; the
	milliseconds are not. A host whose ratio is more than the tolerance (20%) away from 1, by at least
	the floor (100 ms), is named on a line of its own.

	Each host is read twice. Cold is what a build's own generation reports: a fresh process, the generator's JIT included, which is
	what a build pays once per compiler-server process and what a `dotnet build` with the server off pays every time. Warm is the
	same compilation run again in a process that has already run it (DotGram.GenerationWarm, which loads the generator of the commit it
	is measuring and runs it over the project's own files `-WarmRuns` more times, the compile cache off): the cost a solution build, whose
	compilations share a server, and the IDE pay. On a small grammar cold is two to three times warm, 30-40% of it the generator's own JIT,
	so the cold ratio of a small host overstates what a change to the generator costs; the warm one does not. Each round's warm figure is
	the median of its runs after the first, and the ratio is held by the same rule as the cold one, named on a line of its own marked warm.

	DotGram.Sql and DotGram.Examples are rebuilt in a worktree of each commit (`-t:Rebuild`, so that the
	generator runs), and the generator's own reports are read. The compiler's own count of the generator's time
	(`-p:ReportAnalyzer=true`, per compilation of each project and target framework) is read beside them: it is the
	one a base without the reports can give (they arrived on 2026-09-17, after v0.1.0), and the table of it is
	held to the same tolerance. It builds; it times the build's
	generation, so it is a timing: it takes the window itself (WindowLib.ps1: the lock, the wait for builds, the quiet check)
	and lets it go when it ends. It is the machine's quiet that makes the ratio mean anything.

.EXAMPLE
	pwsh benchmarks/Gate-Generation.ps1 -Base d4c58dfd -Head HEAD
#>
param(
	[Parameter(Mandatory)][string]$Base,
	[string]$Head = 'HEAD',
	[int]$Rounds = 3,
	[int]$WarmRuns = 3,
	[double]$Tolerance = 0.20,
	[double]$FloorMilliseconds = 100,
	[string]$Worktrees,
	[double]$SlotMinutes = 60,
	[double]$WaitMinutes = 20,

	# Every project that hosts a grammar. It was DotGram.Sql and DotGram.Examples only, so that "No host moved" was true of two projects of six and read as a statement about the repository
	# (expr, 2026-09-21: a change to DotGram.ExpressionLanguage was not built at all). The report names the projects and how many hosts each gave; a project that gave none is a hole, not a pass.
	[string[]]$Projects = @(
		'src/DotGram.Sql/DotGram.Sql.csproj',
		'examples/DotGram.Examples/DotGram.Examples.csproj',
		'src/DotGram.ExpressionLanguage/DotGram.ExpressionLanguage.csproj',
		'src/DotGram.Web/DotGram.Web.csproj',
		'src/DotGram.Finance/DotGram.Finance.csproj')
)

$ErrorActionPreference = 'Stop'

$repo = (git rev-parse --show-toplevel).Trim()

function Median($values) {
	$sorted = $values | Sort-Object
	$middle = [int][math]::Floor($sorted.Count / 2)
	if ($sorted.Count % 2) { $sorted[$middle] } else { ($sorted[$middle - 1] + $sorted[$middle]) / 2 }
}

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

$Projects = @(Split-FileList $Projects)

if (-not $Worktrees) { $Worktrees = Join-Path (Get-ScratchRoot) 'gate' }

$cpus = Get-StandCpus
$env:MSBUILDDISABLENODEREUSE = '1'

$sides = [ordered]@{
	base = @{ Commit = (git -C $repo rev-parse --short $Base).Trim(); Dir = Join-Path $Worktrees 'base' }
	head = @{ Commit = (git -C $repo rev-parse --short $Head).Trim(); Dir = Join-Path $Worktrees 'head' }
}

foreach ($side in $sides.Values) {
	if (Test-Path $side.Dir) {
		git -C $side.Dir checkout --detach -q $side.Commit
	}
	else {
		git -C $repo worktree add --detach $side.Dir $side.Commit | Out-Null
	}
}

$what   = "Gate-Generation.ps1 -Base $Base -Head $Head -Rounds $Rounds"
$window = Enter-Window $what (Get-Date).AddMinutes($SlotMinutes) $WaitMinutes

if ($window -is [string]) { Stop-Run 3 $window }

$summary = [regex]'DotGram: (?<host>[^,]+), (?<rules>\d+) normalized rules, (?<bytes>\d+) bytes UTF-8 C#, (?<ms>[\d.]+) ms generation'
$taken = @{ base = @{}; head = @{} }
$warmed = @{ base = @{}; head = @{} }
$perProject = @{ base = @{}; head = @{} }

# The compiler's count: at detailed verbosity with ReportAnalyzer, each compilation prints the generators' seconds after its command line (`/out:` and the path of the assembly in `.build/obj/<project>/release_<tfm>`). Only the compilations
# of the project itself are kept: a -t:Rebuild rebuilds what the project references too, and that is the referenced project's row.
$compilerLine  = [regex]'/out:(?<out>\S+\.dll)'
$generatorLine = [regex]'^\s*(?<seconds>\d+[.,]\d+)\s+\S+\s+DotGram\.Generation\.GramGenerator\s*$'
$compiled = @{ base = @{}; head = @{} }

try {
	$quiet = Wait-Quiet $WaitMinutes
	$quiet.Text

	if (-not $quiet.Quiet) { Stop-Run 5 "The machine is not quiet: $($quiet.Why). Nothing was built." }

	# The warm reading is one program, built once from the head's tree and given the generator of each side.
	$warmProject = Join-Path $sides.head.Dir 'benchmarks/DotGram.GenerationWarm/DotGram.GenerationWarm.csproj'
	$warmDll     = $null

	if ($WarmRuns -gt 0) {
		if (-not (Test-Path $warmProject)) { throw "The head has no $warmProject`: -WarmRuns 0 turns the warm reading off." }

		& taskset -c $cpus dotnet build $warmProject -c Release -v:quiet -m:1 -nodeReuse:false | Out-Null

		if ($LASTEXITCODE -ne 0) { throw 'DotGram.GenerationWarm did not build' }

		$warmDll = Get-ChildItem (Join-Path $sides.head.Dir '.build') -Recurse -Filter DotGram.GenerationWarm.dll | Where-Object { $_.FullName -notmatch '[\\/]obj[\\/]' } | Select-Object -First 1 -ExpandProperty FullName
	}

	for ($round = 1; $round -le $Rounds; $round++) {
		foreach ($name in $sides.Keys) {
			$dir = $sides[$name].Dir

			foreach ($project in $Projects) {
				# The report level is set to "true" (the full report) on both sides: the property is a switch on a commit that predates the levels and the full level on one that has them.
				$assembly = [IO.Path]::GetFileNameWithoutExtension($project)
				$out      = $null

				& taskset -c $cpus dotnet build (Join-Path $dir $project) -c Release -t:Rebuild -v:detailed -m:1 -nodeReuse:false -p:UseSharedCompilation=false -p:DotGramReportGeneration=true -p:ReportAnalyzer=true | ForEach-Object {
					if (($m = $compilerLine.Match($_)).Success) { $out = $m.Groups['out'].Value }
					elseif ($out -and ($m = $generatorLine.Match($_)).Success) {
						if ([IO.Path]::GetFileNameWithoutExtension($out) -eq $assembly) {
							# The folder is `obj/Release/<tfm>` on a commit from before the build output moved, `.build/obj/<project>/release_<tfm>` since for a project of several frameworks and plain
							# `.build/obj/<project>/release` for one of a single framework: the key is the framework in every case, read from the project when the folder does not name it.
							$folder = Split-Path $out | Split-Path -Leaf
							if ($folder -match '^release_(?<tfm>.+)$') { $framework = $Matches['tfm'] }
							elseif ($folder -eq 'release') {
								$declared  = ([xml](Get-Content -LiteralPath (Join-Path $dir $project) -Raw)).Project.PropertyGroup.TargetFramework | Where-Object { $_ } | Select-Object -First 1
								$framework = if ($declared) { $declared } else { $folder }
							}
							else { $framework = $folder }
							$key = "$assembly $framework"
							if (-not $compiled[$name].ContainsKey($key)) { $compiled[$name][$key] = @() }
							$compiled[$name][$key] += 1000 * [double]::Parse($m.Groups['seconds'].Value.Replace(',', '.'), [Globalization.CultureInfo]::InvariantCulture)
						}

						$out = $null
					}
				}

				if ($LASTEXITCODE -ne 0) { throw "$name $project did not build" }

				# The reports are in the project's folder under .build/obj, or in the project's own folder in a tree from before the build output moved there.
				$projectRoot = Join-Path $dir ".build/obj/$assembly"
				if (-not (Test-Path $projectRoot)) { $projectRoot = Split-Path (Join-Path $dir $project) }
				$hosts = 0

				foreach ($report in Get-ChildItem $projectRoot -Recurse -Filter '*.DotGramReport.g.cs' -ErrorAction SilentlyContinue) {
					$m = $summary.Match((Get-Content -LiteralPath $report.FullName -Raw))

					if ($m.Success) {
						$key = $m.Groups['host'].Value
						if (-not $taken[$name].ContainsKey($key)) { $taken[$name][$key] = @() }
						$taken[$name][$key] += [double]$m.Groups['ms'].Value
						$hosts++
					}
				}

				$perProject[$name][$project] = $hosts

				if ($WarmRuns -gt 0 -and $hosts -gt 0) {
					# The generator the build above just made, in whichever folder the commit's build output is: `.build/bin/DotGram/release*` since the output moved, `src/DotGram/bin/Release` before.
					$generator = Get-ChildItem $dir -Recurse -Filter DotGram.dll | Where-Object { $_.FullName -match '[\\/]DotGram[\\/](bin[\\/])?[Rr]elease' -and $_.FullName -notmatch '[\\/]obj[\\/]' } | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName

					if (-not $generator) { throw "$name`: no DotGram.dll built for the warm reading" }

					# Run 0 of the process is cold again and is left to the builds' own reports; the runs after it are the warm ones, and a round's figure for a host is their median.
					$runsOfHost = @{}

					& taskset -c $cpus dotnet $warmDll $generator (Join-Path $dir $project) ($WarmRuns + 1) | ForEach-Object {
						$fields = $_ -split "`t"

						if ($fields.Count -eq 4 -and $fields[0] -eq 'host' -and [int]$fields[2] -gt 0) {
							if (-not $runsOfHost.ContainsKey($fields[1])) { $runsOfHost[$fields[1]] = @() }
							$runsOfHost[$fields[1]] += [double]::Parse($fields[3], [Globalization.CultureInfo]::InvariantCulture)
						}
					}

					if ($LASTEXITCODE -ne 0) { throw "$name $project did not run warm" }

					foreach ($key in $runsOfHost.Keys) {
						if (-not $warmed[$name].ContainsKey($key)) { $warmed[$name][$key] = @() }
						$warmed[$name][$key] += Median $runsOfHost[$key]
					}
				}
			}

			"round $round $name $($sides[$name].Commit) $(Get-Date -Format HH:mm:ss)"
		}
	}
}
finally {
	Exit-Window $window "the window of pid $PID ended: $what"
}

''
"Generator time, head $($sides.head.Commit) against base $($sides.base.Commit), median of $Rounds alternating rounds (same cores):"
''
($jit = 'JIT of the compiler process the generator runs in (the environment of this script, inherited by the builds): ' + ((@('DOTNET_TieredCompilation', 'DOTNET_TieredPGO', 'DOTNET_TC_QuickJitForLoops', 'DOTNET_ReadyToRun') | ForEach-Object { $value = [Environment]::GetEnvironmentVariable($_); if ($value) { "$_=$value" } else { "$_ unset" } }) -join ', ') + ' (unset is the runtime default). Processors ' + $cpus + ' (taskset), default priority, report level true on both sides, node reuse and the shared compiler server off.')
'Projects rebuilt, and the hosts each gave (a project that gave 0 is a hole in this report, not a pass):'
foreach ($project in $Projects) { '- {0}: base {1}, head {2}' -f $project, $perProject.base[$project], $perProject.head[$project] }
''
$named = @()

# One table: a row per key, base against head, and the rows that moved beyond the tolerance named.
function Held([string]$what, [hashtable]$base, [hashtable]$head, [string]$mark = '') {
	"| $what | base ms (min-max) | head ms (min-max) | head / base |"
	'| --- | ---: | ---: | ---: |'

	foreach ($key in ($head.Keys | Sort-Object)) {
		if (-not $base.ContainsKey($key)) { $script:named += "$key$mark`: new"; continue }

		$b = Median $base[$key]
		$h = Median $head[$key]
		$ratio = $h / $b

		$bRange = $base[$key] | Measure-Object -Minimum -Maximum
		$hRange = $head[$key] | Measure-Object -Minimum -Maximum

		'| {0} | {1:N0} ({2:N0}-{3:N0}) | {4:N0} ({5:N0}-{6:N0}) | {7:N2}x |' -f $key, $b, $bRange.Minimum, $bRange.Maximum, $h, $hRange.Minimum, $hRange.Maximum, $ratio

		if ([math]::Abs($ratio - 1) -gt $Tolerance -and [math]::Abs($h - $b) -ge $FloorMilliseconds) {
			$script:named += '{0}{4}: {1:N0} ms to {2:N0} ms ({3:+0%;-0%})' -f $key, $b, $h, ($ratio - 1), $mark
		}
	}
}

if ($taken.base.Count -gt 0) { Held 'host' $taken.base $taken.head }
else { "The base wrote no generation reports (they arrived on 2026-09-17; v0.1.0 predates them), so no host is held: the compiler's count below is the gate." }

if ($WarmRuns -gt 0 -and $warmed.head.Count -gt 0) {
	''
	"The same hosts warm: the compilation run $WarmRuns more times in a process that has run the generator once (DotGram.GenerationWarm; each round's figure is the median of those runs), held by the same rule. The table above is a fresh process's, the generator's JIT in it; this one is the cost of a compile after that, which is what a solution build and the IDE pay:"
	''
	if ($warmed.base.Count -gt 0) { Held 'host (warm)' $warmed.base $warmed.head ' (warm)' }
	else { 'The base wrote no generation reports, so no host is held warm either.' }
}

''
"The compiler's count of the generator (ReportAnalyzer), per compilation of each project and target framework; it includes what the generator does besides writing the hosts:"
''
Held 'compilation' $compiled.base $compiled.head

''
if ($named.Count -eq 0) { 'Nothing in the tables above moved by more than the tolerance.' }
foreach ($line in $named) { "- DEVIATION $line" }

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

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

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
$perProject = @{ base = @{}; head = @{} }

# The compiler's count: at detailed verbosity with ReportAnalyzer, each compilation prints the generators' seconds after its command line (`/out:obj/Release/<tfm>/<name>.dll`). Only the compilations
# of the project itself are kept: a -t:Rebuild rebuilds what the project references too, and that is the referenced project's row.
$compilerLine  = [regex]'/out:(?<out>\S+\.dll)'
$generatorLine = [regex]'^\s*(?<seconds>\d+[.,]\d+)\s+\S+\s+DotGram\.Generation\.GramGenerator\s*$'
$compiled = @{ base = @{}; head = @{} }

try {
	$quiet = Wait-Quiet $WaitMinutes
	$quiet.Text

	if (-not $quiet.Quiet) { Stop-Run 5 "The machine is not quiet: $($quiet.Why). Nothing was built." }

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
							$key = "$assembly $(Split-Path (Split-Path $out) -Leaf)"
							if (-not $compiled[$name].ContainsKey($key)) { $compiled[$name][$key] = @() }
							$compiled[$name][$key] += 1000 * [double]::Parse($m.Groups['seconds'].Value.Replace(',', '.'), [Globalization.CultureInfo]::InvariantCulture)
						}

						$out = $null
					}
				}

				if ($LASTEXITCODE -ne 0) { throw "$name $project did not build" }

				$projectRoot = Split-Path (Join-Path $dir $project)
				$hosts = 0

				foreach ($report in Get-ChildItem $projectRoot -Recurse -Filter '*.DotGramReport.g.cs' -ErrorAction SilentlyContinue) {
					$m = $summary.Match((Get-Content $report.FullName -Raw))

					if ($m.Success) {
						$key = $m.Groups['host'].Value
						if (-not $taken[$name].ContainsKey($key)) { $taken[$name][$key] = @() }
						$taken[$name][$key] += [double]$m.Groups['ms'].Value
						$hosts++
					}
				}

				$perProject[$name][$project] = $hosts
			}

			"round $round $name $($sides[$name].Commit) $(Get-Date -Format HH:mm:ss)"
		}
	}
}
finally {
	Exit-Window $window "the window of pid $PID ended: $what"
}

function Median($values) {
	$sorted = $values | Sort-Object
	$middle = [int][math]::Floor($sorted.Count / 2)
	if ($sorted.Count % 2) { $sorted[$middle] } else { ($sorted[$middle - 1] + $sorted[$middle]) / 2 }
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
function Held([string]$what, [hashtable]$base, [hashtable]$head) {
	"| $what | base ms (min-max) | head ms (min-max) | head / base |"
	'| --- | ---: | ---: | ---: |'

	foreach ($key in ($head.Keys | Sort-Object)) {
		if (-not $base.ContainsKey($key)) { $script:named += "$key`: new"; continue }

		$b = Median $base[$key]
		$h = Median $head[$key]
		$ratio = $h / $b

		$bRange = $base[$key] | Measure-Object -Minimum -Maximum
		$hRange = $head[$key] | Measure-Object -Minimum -Maximum

		'| {0} | {1:N0} ({2:N0}-{3:N0}) | {4:N0} ({5:N0}-{6:N0}) | {7:N2}x |' -f $key, $b, $bRange.Minimum, $bRange.Maximum, $h, $hRange.Minimum, $hRange.Maximum, $ratio

		if ([math]::Abs($ratio - 1) -gt $Tolerance -and [math]::Abs($h - $b) -ge $FloorMilliseconds) {
			$script:named += '{0}: {1:N0} ms to {2:N0} ms ({3:+0%;-0%})' -f $key, $b, $h, ($ratio - 1)
		}
	}
}

if ($taken.base.Count -gt 0) { Held 'host' $taken.base $taken.head }
else { "The base wrote no generation reports (they arrived on 2026-09-17; v0.1.0 predates them), so no host is held: the compiler's count below is the gate." }

''
"The compiler's count of the generator (ReportAnalyzer), per compilation of each project and target framework; it includes what the generator does besides writing the hosts:"
''
Held 'compilation' $compiled.base $compiled.head

''
if ($named.Count -eq 0) { 'Nothing in the tables above moved by more than the tolerance.' }
foreach ($line in $named) { "- DEVIATION $line" }

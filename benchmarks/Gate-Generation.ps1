<#
.SYNOPSIS
	Holds the generator's time on the grammars to a base commit's, measured in the same run.

.DESCRIPTION
	The generator's time is machine-state dependent: the base of 2026-09-18 was read at 4.1 s on
	T-SQL at 01:49 and 5.0 s by the same commit rebuilt that evening. So a head is not held to a number
	written down on another day but to the base rebuilt here, alternately with it: base, head, base,
	head, on the same cores, one build at a time, and the ratio of the medians is what is quoted; the
	milliseconds are not. A host whose ratio is more than the tolerance (20%) away from 1, by at least
	the floor (100 ms), is named on a line of its own.

	DotGram.Sql and DotGram.Examples are rebuilt in a worktree of each commit (`-t:Rebuild`, so that the
	generator runs), and the generator's own reports are read. It builds; it times the build's
	generation. Run it in a timing window: it is the machine's quiet that makes the ratio mean anything.

.EXAMPLE
	./benchmarks/Gate-Generation.ps1 -Base d4c58dfd -Head HEAD
#>
param(
	[Parameter(Mandatory)][string]$Base,
	[string]$Head = 'HEAD',
	[int]$Rounds = 3,
	[double]$Tolerance = 0.20,
	[double]$FloorMilliseconds = 100,
	[string]$Worktrees = (Join-Path ([IO.Path]::GetTempPath()) 'dotgram-gate'),
	[UInt64]$Affinity = 0xFFFF
)

$ErrorActionPreference = 'Stop'

$repo = (git rev-parse --show-toplevel).Trim()
$me = Get-Process -Id $PID

$me.ProcessorAffinity = [IntPtr]$Affinity
$me.PriorityClass = 'High'
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

$summary = [regex]'DotGram: (?<host>[^,]+), (?<rules>\d+) normalized rules, (?<bytes>\d+) bytes UTF-8 C#, (?<ms>[\d.]+) ms generation'
$taken = @{ base = @{}; head = @{} }

for ($round = 1; $round -le $Rounds; $round++) {
	foreach ($name in $sides.Keys) {
		$dir = $sides[$name].Dir

		foreach ($project in 'src\DotGram.Sql\DotGram.Sql.csproj', 'examples\DotGram.Examples\DotGram.Examples.csproj') {
			dotnet build (Join-Path $dir $project) -c Release -t:Rebuild -v:quiet -m:1 -nodeReuse:false -p:UseSharedCompilation=false | Out-Null

			if ($LASTEXITCODE -ne 0) { throw "$name $project did not build" }
		}

		foreach ($report in Get-ChildItem $dir -Recurse -Filter '*.DotGramReport.g.cs') {
			$m = $summary.Match((Get-Content $report.FullName -Raw))

			if ($m.Success) {
				$key = $m.Groups['host'].Value
				if (-not $taken[$name].ContainsKey($key)) { $taken[$name][$key] = @() }
				$taken[$name][$key] += [double]$m.Groups['ms'].Value
			}
		}

		"round $round $name $($sides[$name].Commit) $(Get-Date -Format HH:mm:ss)"
	}
}

function Median($values) {
	$sorted = $values | Sort-Object
	$middle = [int][math]::Floor($sorted.Count / 2)
	if ($sorted.Count % 2) { $sorted[$middle] } else { ($sorted[$middle - 1] + $sorted[$middle]) / 2 }
}

''
"Generator time, head $($sides.head.Commit) against base $($sides.base.Commit), median of $Rounds alternating rounds (same cores):"
''
'| host | base ms (min-max) | head ms (min-max) | head / base |'
'| --- | ---: | ---: | ---: |'

$named = @()

foreach ($hostName in ($taken.head.Keys | Sort-Object)) {
	if (-not $taken.base.ContainsKey($hostName)) { $named += "$hostName`: new"; continue }

	$b = Median $taken.base[$hostName]
	$h = Median $taken.head[$hostName]
	$ratio = $h / $b

	$bRange = $taken.base[$hostName] | Measure-Object -Minimum -Maximum
	$hRange = $taken.head[$hostName] | Measure-Object -Minimum -Maximum

	'| {0} | {1:N0} ({2:N0}-{3:N0}) | {4:N0} ({5:N0}-{6:N0}) | {7:N2}x |' -f $hostName, $b, $bRange.Minimum, $bRange.Maximum, $h, $hRange.Minimum, $hRange.Maximum, $ratio

	if ([math]::Abs($ratio - 1) -gt $Tolerance -and [math]::Abs($h - $b) -ge $FloorMilliseconds) {
		$named += '{0}: {1:N0} ms to {2:N0} ms ({3:+0%;-0%})' -f $hostName, $b, $h, ($ratio - 1)
	}
}

''
if ($named.Count -eq 0) { 'No host moved by more than the tolerance.' }
foreach ($line in $named) { "- DEVIATION $line" }

<#
.SYNOPSIS
	Runs a BenchmarkDotNet assembly as a timing window: pinned, announced, and with the affinity of every benchmark process READ back from the process.

.DESCRIPTION
	BenchmarkDotNet starts a process of its own for every benchmark case, so pinning the launcher says nothing about where a case was
	measured. This script does what the stand's own runs do (docs/development.md, "A timing is taken in an announced window") and adds the
	one thing they do not need: it watches the descendants of the run and reads, for EACH worker (the processes whose command line carries
	`--benchmarkId`), the processor affinity and the priority class the operating system reports. A worker that is not on the mask fails the run at once; the run
	is stopped and its numbers are not to be quoted. A worker whose priority is below High is raised to High and the raise is written down.

	What it does, in order:
	  0. refuses to start (exit 5, no flag lifts it) when the machine is not quiet: more than 6% of it in use outside this script over three seconds, or any build tool above 0.15 core; what it saw is printed and written to run.txt either way;
	  1. refuses to start when another window is announced and its process is alive (`dotgram-timing-window.txt` in the temp directory), or when BDN is asked
	     to run in process (`--inProcess`: a number taken there is not a number of this stand);
	  2. announces the window (pid, started, until, what) in the same format as the stand's own runs, and puts the file back to `idle` when the run ends (benchmarks/Window.ps1 reads it);
	  3. pins itself to logical processors 0-15 (0xFFFF) at high priority and starts `dotnet <Assembly> <BdnArgs> --artifacts <out>`; the children inherit the mask;
	  4. every 100 ms reads the workers that appeared and checks each once (a worker that starts and ends between two reads is not checked, and run.txt says how many
	     workers BDN executed, from its own log, against how many were read: a case of a real run lasts many seconds, a dry job's does not);
	  5. (-Commit names the commit of what is under test when the assembly is built outside the repository, e.g. "library 1a2b3c4d, main 5e6f7a8b"; without it the commit is read from the repository the assembly sits in, and says "no repository" when there is none. -Note is written into the announcement and into run.txt: "a pricing probe, its numbers are not to be quoted")
	  6. at the end writes run.txt beside BDN's artifacts: the commit, the mask, the JIT variables, every worker checked (pid, affinity, priority,
	     when) and the exit code, so that a reader a month later has the header a paired report carries.

	It builds nothing: build the assembly first, on 16-31 and before the window (benchmarks/Build-Side.ps1 pins builds there). BDN itself compiles a generated
	project once at the start of a run; that is inside the window and on 0-15, and is the only build that is. Nobody else builds while a window is
	announced, on either half: 16-31 shares the last-level cache and the memory bandwidth with 0-15 (benchmarks/README.md).

	Every class that compares two things carries an A/A row (the same method under two names), and a ratio is read against its spread: the resolution is the hour's.

.EXAMPLE
	& .\benchmarks\Run-Bdn.ps1 -Assembly .\benchmarks\DotGram.Benchmarks\bin\Release\net10.0\DotGram.Benchmarks.dll -Label sql-tsql -BdnArgs '--filter','*ScriptDomBenchmarks*' -LimitMinutes 45
#>
param(
	[Parameter(Mandatory)][string]$Assembly,
	[Parameter(Mandatory)][string]$Label,
	[string[]]$BdnArgs = @(),
	[double]$LimitMinutes = 60,
	[UInt64]$Affinity = 0xFFFF,
	[string]$Root = 'T:\TEMP\dotgram-bdn',
	[string]$Note = '',
	[string]$Commit = ''
)

$ErrorActionPreference = 'Stop'

$Assembly = (Resolve-Path $Assembly).Path
$window   = Join-Path ([IO.Path]::GetTempPath()) 'dotgram-timing-window.txt'
$stamp    = Get-Date -Format 'yyyyMMdd-HHmmss'
$out      = Join-Path $Root "$Label-$stamp"
$repo     = (git -C (Split-Path $Assembly) rev-parse --show-toplevel 2>$null)

if ($BdnArgs -contains '--inProcess' -or $BdnArgs -contains '-i') { Write-Error 'BDN in process is not a timing of this stand (benchmarks/README.md): run it out of process.'; exit 2 }

if (-not (Test-Path $window)) { Write-Warning 'The announcement file did not exist (it is meant to exist always, idle when there is no window): something deleted it. It is created now.' }

if (Test-Path $window) {
	$owner = (Get-Content $window | Where-Object { $_ -like 'pid *' } | Select-Object -First 1)
	$ownerPid = if ($owner) { [int]($owner.Substring(4)) } else { 0 }

	if ($ownerPid -ne 0 -and $ownerPid -ne $PID -and (Get-Process -Id $ownerPid -ErrorAction SilentlyContinue)) {
		Write-Error ("Another timing window is announced and its process is alive: " + ((Get-Content $window) -join '; ')); exit 3
	}
}

# The quiet check: a build or a run that somebody started and forgot is found by the instrument, not by memory (2026-09-21: a restore and a compile were still running when a window was announced).
# The CPU time of every readable process over three seconds, outside this script. Two refusals, and no flag that lifts either: the machine as a whole above 6% (an idle machine here reads 1.7-2.8%, twelve samples of
# 2026-09-21: a light keeper at 0.3 core, the sessions' own processes about 0.2), and any build tool (dotnet, MSBuild, the compiler servers, csc, git) above 0.15 core, which a small compile alone would not
# lift the total to 6% for. What it saw is printed and written into run.txt whether it passes or not, so the thresholds can be moved by the records and not by memory.
$quietPercent = 6
$buildTools   = 'dotnet', 'MSBuild', 'VBCSCompiler', 'csc', 'vbcscompiler', 'cl', 'link', 'git', 'nuget', 'node', 'msbuild'

function CpuSample {
	$sample = @{}

	foreach ($process in Get-Process) {
		try { if ($process.Id -ne $PID -and $process.Id -ne 0) { $sample[$process.Id] = @($process.ProcessName, $process.TotalProcessorTime.TotalSeconds) } } catch { }
	}

	$sample
}

$before  = CpuSample
Start-Sleep -Seconds 3
$after   = CpuSample
$cores   = [int]$env:NUMBER_OF_PROCESSORS
$busy    = @(foreach ($id in $after.Keys) { if ($before.ContainsKey($id)) { [pscustomobject]@{ Id = $id; Name = $after[$id][0]; Cores = [math]::Round(($after[$id][1] - $before[$id][1]) / 3, 3) } } })
$total   = ($busy | Measure-Object Cores -Sum).Sum
$percent = [math]::Round(100 * $total / $cores, 1)
$top     = ($busy | Sort-Object Cores -Descending | Select-Object -First 3 | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores)" }) -join '; '
$builders = @($busy | Where-Object { $_.Name -in $buildTools -and $_.Cores -gt 0.15 })
$quiet    = "Quiet check before the start: $percent% of the machine ($([math]::Round($total, 2)) of $cores logical processors) in use outside this script, three seconds; busiest: $top."

$quiet

if ($percent -gt $quietPercent -or $builders.Count -gt 0) {
	$why = if ($builders.Count -gt 0) { "a build tool is running: $(($builders | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores) cores" }) -join '; ')" } else { "more than $quietPercent% of the machine is in use" }
	Write-Error "The machine is not quiet: $why. $quiet Nothing was announced or started."
	exit 5
}

New-Item -ItemType Directory -Force $out | Out-Null

$started = Get-Date
$until   = $started.AddMinutes($LimitMinutes)
$what    = "Run-Bdn.ps1 $Label $($BdnArgs -join ' ')$(if ($Note) { ' [' + $Note + ']' })"

Set-Content $window @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what")

$me = Get-Process -Id $PID
$me.ProcessorAffinity = [IntPtr][int64]$Affinity
$me.PriorityClass = 'High'

$checked   = @{}
$rows      = New-Object System.Collections.Generic.List[string]
$truncated = $false
$failure   = $null
$exit      = $null
$process   = $null

function Descendants([int]$parent) {
	foreach ($child in @(Get-CimInstance Win32_Process -Filter "ParentProcessId=$parent" -ErrorAction SilentlyContinue)) {
		$child
		Descendants $child.ProcessId
	}
}

function Quote([string]$one) { if ($one -match '[\s"]') { '"' + $one.Replace('"', '\"') + '"' } else { $one } }

try {
	$argumentList = (@($Assembly) + $BdnArgs + @('--artifacts', $out) | ForEach-Object { Quote $_ }) -join ' '
	$process = Start-Process -FilePath 'dotnet' -ArgumentList $argumentList -PassThru -NoNewWindow -WorkingDirectory (Split-Path $Assembly) -RedirectStandardOutput (Join-Path $out 'bdn.log') -RedirectStandardError (Join-Path $out 'bdn.err.log')

	"Window announced $($started.ToString('HH:mm:ss')), until $($until.ToString('HH:mm:ss')): $what; mask 0x$($Affinity.ToString('X')), out $out"

	while (-not $process.HasExited) {
		Start-Sleep -Milliseconds 100

		if ((Get-Date) -gt $until) { $truncated = $true; break }

		# One query for every worker there is (a walk of the whole tree costs several hundred milliseconds and a short case is over before it ends); the workers of THIS run are its direct children.
		foreach ($worker in @(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe' AND CommandLine LIKE '%--benchmarkId%'" -ErrorAction SilentlyContinue | Where-Object { $_.ParentProcessId -eq $process.Id })) {
			if ($checked.ContainsKey($worker.ProcessId)) { continue }

			$live = Get-Process -Id $worker.ProcessId -ErrorAction SilentlyContinue

			if (-not $live) { continue }

			$mask     = [UInt64][int64]$live.ProcessorAffinity
			$priority = "$($live.PriorityClass)"
			$note     = ''

			if ($priority -notin 'High', 'RealTime') {
				$live.PriorityClass = 'High'
				$note     = " (was $priority, raised by the script)"
				$priority = "$((Get-Process -Id $worker.ProcessId).PriorityClass)"
			}

			$checked[$worker.ProcessId] = $true
			$rows.Add("| $($worker.ProcessId) | 0x$($mask.ToString('X')) | $priority$note | $((Get-Date).ToString('HH:mm:ss')) |")
			"worker $($worker.ProcessId): affinity 0x$($mask.ToString('X')), priority $priority$note"

			if ($mask -ne $Affinity) { $failure = "worker $($worker.ProcessId) runs on 0x$($mask.ToString('X')), not on 0x$($Affinity.ToString('X')): the run is stopped and its numbers are not to be quoted"; break }
		}

		if ($failure) { break }
	}

	if ($failure -or $truncated) {
		foreach ($d in @(Descendants $process.Id)) { Stop-Process -Id $d.ProcessId -Force -ErrorAction SilentlyContinue }
		Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
	}
	else {
		$process.WaitForExit()
		$exit = $process.ExitCode
	}
}
finally {
	# The file stays and says idle: its absence must mean "something is wrong with the stand", never "no window" (benchmarks/Window.ps1).
	if ((Test-Path $window) -and ((Get-Content $window | Select-Object -First 1) -eq "pid $PID")) { Set-Content $window @('idle', "since $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))", "why the window of pid $PID ended: $what") }
}

if (-not $failure -and -not $truncated -and $checked.Count -eq 0) { $failure = 'no benchmark worker was seen, so no case was checked: the run measured nothing this script can vouch for' }

$executed = if (Test-Path (Join-Path $out 'bdn.log')) { @(Select-String -Path (Join-Path $out 'bdn.log') -Pattern '^// Execute:').Count } else { 0 }
$commit = if ($Commit) { $Commit } elseif ($repo) { (git -C $repo rev-parse --short HEAD) + $(if (git -C $repo status --porcelain 2>$null) { ' (+ uncommitted changes)' } else { '' }) } else { 'no repository' }
$jit    = 'DOTNET_TieredCompilation', 'DOTNET_TieredPGO', 'DOTNET_TC_QuickJitForLoops', 'DOTNET_ReadyToRun' | ForEach-Object { $v = [Environment]::GetEnvironmentVariable($_); if ($v) { "$_=$v" } else { "$_ unset" } }

$report = @(
	"# BenchmarkDotNet run $Label, $($started.ToString('yyyy-MM-dd HH:mm')) - $((Get-Date).ToString('HH:mm'))",
	'',
	"Assembly $Assembly, commit $commit. Arguments: $($BdnArgs -join ' '). Machine $env:COMPUTERNAME, launcher and workers on mask 0x$($Affinity.ToString('X')) at high priority.",
	"JIT: $($jit -join ', ') (unset is the runtime's default: tiered compilation on, dynamic PGO on).",
	$(if ($Note) { "**$Note**" }),
	$quiet,
	"Exit code: $(if ($null -ne $exit) { $exit } else { 'none (stopped)' }). $(if ($truncated) { 'STOPPED at the limit of ' + $LimitMinutes + ' minutes: the results are incomplete.' })",
	$(if ($failure) { "**FAILED: $failure**" } else { "Every worker that was read back from the operating system was on the mask. Coverage: BDN executed $executed workers (its log), $($checked.Count) were read$(if ($checked.Count -lt $executed) { '; the others ended between two reads, and the mask they had is the inherited one, not a reading' })." }),
	'',
	'| worker pid | affinity read | priority read | at |',
	'| --- | --- | --- | --- |'
) + $rows

Set-Content (Join-Path $out 'run.txt') $report

$report
if ($failure) { exit 4 }
if ($truncated) { exit 6 }
if ($exit) { exit $exit }

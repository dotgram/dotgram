<#
.SYNOPSIS
	Runs a BenchmarkDotNet assembly as a timing window: announced, on the stand's half of the machine, and with the processors of every benchmark process READ back from the kernel.

.DESCRIPTION
	BenchmarkDotNet starts a process of its own for every benchmark case, so where the launcher runs says nothing about where a case was measured. This script does what Run-Announced.ps1 does for the
	stand (the window lock, the wait for builds, the quiet check, `taskset`; D147) and adds the one thing the stand does not need: it watches the children of the run and reads, for EACH worker (the
	processes whose command line carries `--benchmarkId`), the processors the kernel allows it (`Cpus_allowed_list`). A worker that is not on the stand's processors fails the run at once; the run is
	stopped and its numbers are not to be quoted.

	What it does, in order:
	  0. refuses when BDN is asked to run in process (`--inProcess`: a number taken there is not a number of this stand), and on a system that is not Linux;
	  1. takes the window (WindowLib.ps1, Enter-Window): refuses (exit 3) when another is open, waits at most -WaitMinutes for the builds that hold the builds lock (benchmarks/Aside.sh) to end;
	  2. the quiet check (exit 5, no flag lifts it): more than 6% of the machine in use outside this script over three seconds, or any build tool above 0.15 core; what it saw is printed and written to run.txt either way;
	  3. reads the state of what will be measured (below) and refuses a quotable run on a tree that is not clean (exit 5; -Probe lets it through, stamped);
	  4. starts `taskset -c <cpus> dotnet <Assembly> <BdnArgs> --artifacts <out>` (processors 0-7 and 16-23 by default: the first CCD with its SMT siblings); the children inherit the processors. This
	     script itself is not pinned: it watches from the other half;
	  5. every 100 ms reads the workers that appeared and checks each once, by pid and start time (a worker that starts and ends between two reads is not checked, and run.txt says how many
	     workers BDN executed, from its own log, against how many were read: a case of a real run lasts many seconds, a dry job's does not);
	  6. (-Commit names the commit of what is under test when the assembly is built outside the repository, e.g. "library 1a2b3c4d, main 5e6f7a8b"; it is printed beside the reading, never in place
	     of it. -Note is written into the announcement and into run.txt: "a pricing probe, its numbers are not to be quoted")
	  7. at the end writes run.txt beside BDN's artifacts (<scratch>/bdn/<label>-<time>, /ramdisk/build/dotgram on this machine): the commit, the processors, the JIT variables, every worker checked
	     (pid, processors, when) and the exit code, so that a reader a month later has the header a paired report carries.

	It builds nothing: build the assembly first, through Aside.sh and before the window. BDN compiles a generated project once at the start of a run; that is inside the window and on the stand's
	processors, and is the only build that is. The priority cannot be raised on this machine (no privilege to lower a nice value): BDN's own attempt to raise its workers fails with a warning, and every
	timing runs at the default priority.

	Every class that compares two things carries an A/A row (the same method under two names), and a ratio is read against its spread: the resolution is the hour's.

.EXAMPLE
	pwsh benchmarks/Run-Bdn.ps1 -Assembly benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll -Label sql-tsql -BdnArgs '--filter','*ScriptDomBenchmarks*' -LimitMinutes 45
#>
param(
	[Parameter(Mandatory)][string]$Assembly,
	[Parameter(Mandatory)][string]$Label,
	[string[]]$BdnArgs = @(),
	[double]$LimitMinutes = 60,
	[double]$WaitMinutes = 20,
	[string]$Root,
	[string]$Note = '',
	[string]$Commit = '',
	[switch]$Within,
	[switch]$Probe
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

$Assembly = (Resolve-Path $Assembly).Path
$stamp    = Get-Date -Format 'yyyyMMdd-HHmmss'
$cpus     = Get-StandCpus
$cpuSet   = ConvertTo-CpuSet $cpus
$repo     = (git -C (Split-Path $Assembly) rev-parse --show-toplevel 2>$null)

if (-not $Root) { $Root = Join-Path (Get-ScratchRoot) 'bdn' }

$out = Join-Path $Root "$Label-$stamp"

if ($BdnArgs -contains '--inProcess' -or $BdnArgs -contains '-i') { Stop-Run 2 'BDN in process is not a timing of this stand (benchmarks/README.md): run it out of process.' }

# -Within: this run is one step of a window that its CALLER opened in the same process (benchmarks/Run-BdnQueue.ps1): the step neither takes the window nor lets it go, so that the window has no gap
# between its steps. The quiet check still runs before every step.
if ($Within -and (Get-WindowState) -ne 'Within') { Stop-Run 3 '-Within needs the window to be open and held by the calling process.' }

# THE STATE OF WHAT WILL BE MEASURED, read and never asserted (2026-09-21: BenchmarkDotNet does not run the dll it is given; at the start of every run it builds a generated project that references the benchmark
# PROJECT, so it rebuilds the project and what it references from the SOURCES as they are then. A tree that somebody edits while a window is open is measured as edited, and a caller's sentence about the commit
# hid what this script would have read: -Commit is now printed beside the reading and can never replace it.)
function ProjectDirectory([string]$assemblyPath) {
	$name = [IO.Path]::GetFileNameWithoutExtension($assemblyPath)

	for ($dir = Split-Path $assemblyPath; $dir; $dir = Split-Path $dir) {
		if (Test-Path (Join-Path $dir "$name.csproj")) { return $dir }
	}

	return $null
}

function SourceFiles([string]$dir) {
	Get-ChildItem $dir -Recurse -File -Include '*.cs', '*.csproj', '*.props', '*.targets', '*.json' -ErrorAction SilentlyContinue |
		Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object FullName
}

function SourceHash([string]$dir) {
	$lines = foreach ($file in SourceFiles $dir) { "$($file.FullName.Substring($dir.Length)) $((Get-FileHash $file.FullName -Algorithm SHA256).Hash)" }

	[BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes(($lines -join "`n")))).Replace('-', '').Substring(0, 16)
}

$project      = ProjectDirectory $Assembly
$stateLines   = @()
$stateRefusal = @()
$hashAtStart  = $null

if (-not $project) {
	$stateLines += "State of the sources: no project file named after the assembly was found above it, so BenchmarkDotNet's rebuild cannot be watched; the run is not quotable."
	$stateRefusal += 'no project directory found for the assembly'
}
else {
	$assemblyTime = (Get-Item $Assembly).LastWriteTime
	$newer        = @(SourceFiles $project | Where-Object { $_.LastWriteTime -gt $assemblyTime })
	$hashAtStart  = SourceHash $project
	$root         = git -C $project rev-parse --show-toplevel 2>$null
	$head         = if ($root) { (git -C $root rev-parse --short HEAD) } else { 'not in a repository' }
	$tracked      = if ($root) { @(git -C $root status --porcelain --untracked-files=no 2>$null) } else { @() }
	$untracked    = if ($root) { @(git -C $root status --porcelain -- $project 2>$null | Where-Object { $_ -like '`?`?*' }) } else { @() }
	$stateLines  += "State of the sources, read at $((Get-Date).ToString('HH:mm:ss')): project $project; repository HEAD $head; $($tracked.Count) tracked files modified in the repository, $($untracked.Count) untracked (not ignored) in the project (a benchmark project outside the repository is untracked by design: the source hash below is what watches it); source hash of the project $hashAtStart; $($newer.Count) source files NEWER than the assembly ($($assemblyTime.ToString('HH:mm:ss')))$(if ($newer.Count -gt 0) { ', newest ' + ($newer | Sort-Object LastWriteTime -Descending | Select-Object -First 1).Name + ' at ' + ($newer | Sort-Object LastWriteTime -Descending | Select-Object -First 1).LastWriteTime.ToString('HH:mm:ss') })."

	if ($tracked.Count -gt 0) { $stateRefusal += "the repository has $($tracked.Count) modified tracked files ($(($tracked | Select-Object -First 3) -join '; '))" }
	if ($newer.Count -gt 0) { $stateRefusal += "$($newer.Count) source files are newer than the assembly, so BenchmarkDotNet will build something other than the assembly it was given" }
}

if ($Commit) { $stateLines += "Caller says: $Commit (a sentence, printed beside the reading above and never in place of it)." }

$stateLines

if ($stateRefusal.Count -gt 0) {
	if ($Probe) { $stateLines += "PROBE: a run marked a probe is allowed on this state, and its numbers are not to be quoted ($($stateRefusal -join '; '))." }
	else { Stop-Run 5 "The tree is not in a state to be measured: $($stateRefusal -join '; '). A quotable run needs a clean, frozen tree built once before the window (or -Probe, and the numbers are then not quoted). Nothing was started." }
}

$started   = Get-Date
$until     = $started.AddMinutes($LimitMinutes)
$what      = "Run-Bdn.ps1 $Label $($BdnArgs -join ' ')$(if ($Note) { ' [' + $Note + ']' })"
$window    = $null

if (-not $Within) {
	$window = Enter-Window $what $until $WaitMinutes

	if ($window -is [string]) { Stop-Run 3 $window }
}

$checked   = @{}
$rows      = New-Object System.Collections.Generic.List[string]
$truncated = $false
$failure   = $null
$exit      = $null
$process   = $null

# The children of a process, from every one of its threads (/proc/<pid>/task/<tid>/children).
function Children([int]$parent) {
	foreach ($task in @(Get-ChildItem "/proc/$parent/task" -Directory -ErrorAction SilentlyContinue)) {
		foreach ($child in ((Get-Content (Join-Path $task.FullName 'children') -Raw -ErrorAction SilentlyContinue) -split '\s+')) { if ($child) { [int]$child } }
	}
}

function Quote([string]$one) { if ($one -match '[\s"]') { '"' + $one.Replace('"', '\"') + '"' } else { $one } }

function Descendants([int]$parent) {
	foreach ($child in @(Children $parent)) {
		$child
		Descendants $child
	}
}

try {
	# The quiet check runs inside the window, after the builds it waited for have ended: a build or a run that somebody started without Aside.sh is found by the instrument, not by memory
	# (2026-09-21: a restore and a compile were still running when a window was announced). No flag lifts it.
	$quiet = Measure-Quiet
	$quiet.Text

	if (-not $quiet.Quiet) { Stop-Run 5 "The machine is not quiet: $($quiet.Why). $($quiet.Text) Nothing was started." }

	New-Item -ItemType Directory -Force $out | Out-Null

	# BDN builds its generated project inside the window; a build node or a compiler server left behind by it would sit on the stand's processors.
	$env:MSBUILDDISABLENODEREUSE = '1'
	$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'

	$argumentList = (@('-c', $cpus, 'dotnet', $Assembly) + $BdnArgs + @('--artifacts', $out) | ForEach-Object { Quote $_ }) -join ' '
	$process = Start-Process -FilePath 'taskset' -ArgumentList $argumentList -PassThru -WorkingDirectory (Split-Path $Assembly) -RedirectStandardOutput (Join-Path $out 'bdn.log') -RedirectStandardError (Join-Path $out 'bdn.err.log')

	"Window open $($started.ToString('HH:mm:ss')), until $($until.ToString('HH:mm:ss')): $what; processors $cpus, out $out"

	while (-not $process.HasExited) {
		Start-Sleep -Milliseconds 100

		if ((Get-Date) -gt $until) { $truncated = $true; break }

		# The workers of THIS run are its direct children (taskset executes dotnet in its own process, so the pid is the launcher's).
		foreach ($id in @(Children $process.Id)) {
			$command = (Get-Content "/proc/$id/cmdline" -Raw -ErrorAction SilentlyContinue) -replace "`0", ' '

			if ($command -notlike '*--benchmarkId*') { continue }

			# A worker is told apart by its pid AND its start time: pids are recycled (2026-09-21, 58 of 60 read, the two unread were second bearers of recycled pids).
			$live = Get-Process -Id $id -ErrorAction SilentlyContinue

			if (-not $live) { continue }

			$key = "$id@$($live.StartTime.Ticks)"

			if ($checked.ContainsKey($key)) { continue }

			$allowed = Get-ProcessCpus $id

			if (-not $allowed) { continue }

			$checked[$key] = $true
			$rows.Add("| $id | $allowed | $((Get-Date).ToString('HH:mm:ss')) |")
			"worker ${id}: processors $allowed"

			if ((ConvertTo-CpuSet $allowed) -ne $cpuSet) { $failure = "worker $id runs on processors $allowed, not on $cpus`: the run is stopped and its numbers are not to be quoted"; break }
		}

		if ($failure) { break }
	}

	if ($failure -or $truncated) {
		foreach ($id in @(Descendants $process.Id)) { Stop-Process -Id $id -Force -ErrorAction SilentlyContinue }
		Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
	}
	else {
		$process.WaitForExit()
		$exit = $process.ExitCode
	}
}
finally {
	if ($window) { Exit-Window $window "the window of pid $PID ended: $what" }
}

if (-not $failure -and -not $truncated -and $checked.Count -eq 0) { $failure = 'no benchmark worker was seen, so no case was checked: the run measured nothing this script can vouch for' }

$hashAtEnd = if ($project) { SourceHash $project } else { $null }

if ($project -and $hashAtEnd -ne $hashAtStart) { $failure = "the sources of the project changed while the run was going (source hash $hashAtStart at the start, $hashAtEnd at the end): BenchmarkDotNet builds from them, so what was measured is not what the start line says" }

$executed = if (Test-Path (Join-Path $out 'bdn.log')) { @(Select-String -Path (Join-Path $out 'bdn.log') -Pattern '^// Execute:').Count } else { 0 }
$commit = if ($repo) { (git -C $repo rev-parse --short HEAD) + $(if (git -C $repo status --porcelain 2>$null) { ' (+ uncommitted changes)' } else { '' }) } else { 'no repository' }
$jit    = 'DOTNET_TieredCompilation', 'DOTNET_TieredPGO', 'DOTNET_TC_QuickJitForLoops', 'DOTNET_ReadyToRun' | ForEach-Object { $v = [Environment]::GetEnvironmentVariable($_); if ($v) { "$_=$v" } else { "$_ unset" } }

$report = @(
	"# BenchmarkDotNet run $Label, $($started.ToString('yyyy-MM-dd HH:mm')) - $((Get-Date).ToString('HH:mm'))",
	'',
	"Assembly $Assembly, commit $commit. Arguments: $($BdnArgs -join ' '). Machine $([Environment]::MachineName), launcher and workers under taskset -c $cpus, default priority (it cannot be raised here).",
	"JIT: $($jit -join ', ') (unset is the runtime's default: tiered compilation on, dynamic PGO on).",
	$(if ($Note) { "**$Note**" }),
	$quiet.Text,
	$stateLines,
	"Source hash at the end: $hashAtEnd$(if ($hashAtEnd -eq $hashAtStart) { ' (the same as at the start)' } else { ' (NOT the same as at the start)' }).",
	"Exit code: $(if ($null -ne $exit) { $exit } else { 'none (stopped)' }). $(if ($truncated) { 'STOPPED at the limit of ' + $LimitMinutes + ' minutes: the results are incomplete.' })",
	$(if ($failure) { "**FAILED: $failure**" } else { "Every worker that was read back from the kernel was on the stand's processors. Coverage: BDN executed $executed workers (its log), $($checked.Count) were read$(if ($checked.Count -lt $executed) { '; the others ended between two reads, and the processors they had are the inherited ones, not a reading' })." }),
	'',
	'| worker pid | processors read | at |',
	'| --- | --- | --- |'
) + $rows

if (Test-Path $out) { Set-Content (Join-Path $out 'run.txt') $report }

$report
if ($failure) { exit 4 }
if ($truncated) { exit 6 }
# An explicit code on every path: a caller (Run-BdnQueue.ps1) reads $LASTEXITCODE, and a stale one left by a failed `git` above would stop its queue.
exit $(if ($exit) { $exit } else { 0 })

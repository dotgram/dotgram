<#
.SYNOPSIS
	Runs any timing script as an announced window: the quiet check, the announcement (with the start time of this process), the pin to 0-15 at high priority, the state of what is measured read and written down, and the announcement put back to idle at the end.

.DESCRIPTION
	For a timing that is not BenchmarkDotNet (a fresh process per sample, a harness of its own): `Run-Bdn.ps1` does this for BDN and reads the workers back; this script does the rest of it for a script that starts its own processes. It does NOT read those processes back: whether they sit on the mask is theirs to show, and the safe way is that they start on it, because they inherit this process's mask
	(a script that pins a process AFTER it started, `$run.ProcessorAffinity = ...`, lets its first milliseconds run anywhere, which is the part a first-call figure is made of).

	Refuses to start (exit 5) on a machine that is not quiet (more than 6% of it in use outside this script over three seconds, or a build tool above 0.15 core; what it saw is printed and written to run.txt either way) or (exit 3) when a window is announced and its announcer is alive (pid and start time). Then it announces, pins itself to logical processors 0-15 at high priority, runs `pwsh -File <Script> <Arguments>` with the output shown and written to output.txt, and writes run.txt in `T:\TEMP\dotgram-window\<label>-<time>`: the quiet reading, the state of the repository the script sits in (HEAD, modified tracked files), the sha256 of every file named in -Artifacts (the harness, the libraries: what was actually run), the mask and the exit code.
	A quotable run refuses (exit 5) on modified tracked files in the repository; -Probe lets it through, stamped, and its numbers are not quoted.

.EXAMPLE
	& .\benchmarks\Run-Announced.ps1 -Label fix-first-validate -Script .\benchmarks\FirstCall\Fix\Run-Validate.ps1 -SlotMinutes 15 -Artifacts .\benchmarks\FirstCall\Fix\bin\Release\net10.0\fixfirst.exe, <library dll>
#>
param(
	[Parameter(Mandatory)][string]$Label,
	[Parameter(Mandatory)][string]$Script,
	[string[]]$Arguments = @(),
	[double]$SlotMinutes = 15,
	[string[]]$Artifacts = @(),
	[string]$Note = '',
	[UInt64]$Affinity = 0xFFFF,
	[string]$Root = 'T:\TEMP\dotgram-window',
	[switch]$Probe
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

$Script = (Resolve-Path $Script).Path
$window = Get-WindowFile
$stamp  = Get-Date -Format 'yyyyMMdd-HHmmss'
$out    = Join-Path $Root "$Label-$stamp"

# The quiet check (the same rule as Run-Bdn.ps1: 6% of the machine, or a build tool above 0.15 core; keyed by pid and start time).
$buildTools = 'dotnet', 'MSBuild', 'VBCSCompiler', 'csc', 'vbcscompiler', 'cl', 'link', 'git', 'nuget', 'node', 'msbuild'

function CpuSample {
	$sample = @{}

	foreach ($process in Get-Process) {
		try { if ($process.Id -ne $PID -and $process.Id -ne 0) { $sample["$($process.Id)@$($process.StartTime.Ticks)"] = @($process.ProcessName, $process.TotalProcessorTime.TotalSeconds, $process.Id) } } catch { }
	}

	$sample
}

$before   = CpuSample
Start-Sleep -Seconds 3
$after    = CpuSample
$cores    = [int]$env:NUMBER_OF_PROCESSORS
$busy     = @(foreach ($id in $after.Keys) { if ($before.ContainsKey($id)) { [pscustomobject]@{ Id = $after[$id][2]; Name = $after[$id][0]; Cores = [math]::Round(($after[$id][1] - $before[$id][1]) / 3, 3) } } })
$total    = ($busy | Measure-Object Cores -Sum).Sum
$percent  = [math]::Round(100 * $total / $cores, 1)
$top      = ($busy | Sort-Object Cores -Descending | Select-Object -First 3 | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores)" }) -join '; '
$builders = @($busy | Where-Object { $_.Name -in $buildTools -and $_.Cores -gt 0.15 })
$quiet    = "Quiet check before the start: $percent% of the machine ($([math]::Round($total, 2)) of $cores logical processors) in use outside this script, three seconds; busiest: $top."

$quiet

if ($percent -gt 6 -or $builders.Count -gt 0) {
	$why = if ($builders.Count -gt 0) { "a build tool is running: $(($builders | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores) cores" }) -join '; ')" } else { 'more than 6% of the machine is in use' }
	Write-Error "The machine is not quiet: $why. $quiet Nothing was announced or started."
	exit 5
}

# A window that is announced and whose announcer is alive (pid and start time).
if (Test-Path $window) {
	$lines = @(Get-Content $window)

	if ((Get-WindowOwnerState $lines) -in 'Alive', 'AliveByPid') { Write-Error ("Another timing window is announced and its process is alive: " + ($lines -join '; ')); exit 3 }
}

# The state of what is measured, read.
$scriptDirectory = Split-Path $Script
$root  = git -C $scriptDirectory rev-parse --show-toplevel 2>$null
$head  = if ($root) { (git -C $root rev-parse --short HEAD) } else { 'not in a repository' }
$modified = if ($root) { @(git -C $root status --porcelain --untracked-files=no 2>$null) } else { @() }
$state = @("State of the repository the script sits in, read at $((Get-Date).ToString('HH:mm:ss')): HEAD $head; $($modified.Count) tracked files modified$(if ($modified.Count -gt 0) { ' (' + (($modified | Select-Object -First 3) -join '; ') + ')' }).")

foreach ($artifact in $Artifacts) {
	if (Test-Path $artifact) { $item = Get-Item $artifact; $state += "Artifact $($item.FullName): sha256 $((Get-FileHash $item.FullName -Algorithm SHA256).Hash.Substring(0, 16)), $($item.Length) bytes, written $($item.LastWriteTime.ToString('HH:mm:ss'))." }
	else { $state += "Artifact ${artifact}: NOT FOUND." }
}

$state

if ($modified.Count -gt 0) {
	if ($Probe) { $state += "PROBE: allowed on a repository with modified tracked files; its numbers are not to be quoted." }
	else { Write-Error "The repository has $($modified.Count) modified tracked files ($(($modified | Select-Object -First 3) -join '; ')). A quotable run needs a clean tree (or -Probe, and the numbers are then not quoted). Nothing was announced or started."; exit 5 }
}

New-Item -ItemType Directory -Force $out | Out-Null

$started = Get-Date
$what    = "Run-Announced.ps1 $Label$(if ($Probe) { ' [PROBE]' })$(if ($Note) { ' [' + $Note + ']' })"
Set-Content $window (New-WindowAnnouncement $started $started.AddMinutes($SlotMinutes) $what)

$me = Get-Process -Id $PID
$me.ProcessorAffinity = [IntPtr][int64]$Affinity
$me.PriorityClass = 'High'

"Window announced $($started.ToString('HH:mm:ss')), until $($started.AddMinutes($SlotMinutes).ToString('HH:mm:ss')): $what; mask 0x$($Affinity.ToString('X')), out $out"

$exit = $null

try {
	& pwsh -NoProfile -File $Script @Arguments 2>&1 | Tee-Object -FilePath (Join-Path $out 'output.txt')
	$exit = $LASTEXITCODE
}
finally {
	if ((Test-Path $window) -and ((Get-Content $window | Select-Object -First 1) -eq "pid $PID")) { Set-Content $window @('idle', "since $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))", "why the window of pid $PID ended: $what") }
}

$report = @(
	"# $Label, $($started.ToString('yyyy-MM-dd HH:mm')) - $((Get-Date).ToString('HH:mm'))",
	'',
	"Script $Script, arguments: $($Arguments -join ' '). Machine $env:COMPUTERNAME, this process and its children on mask 0x$($Affinity.ToString('X')) at high priority (the children inherit it; whether they stayed on it was not read back).",
	$quiet
) + $state + @("Exit code: $exit.", $(if ($Note) { "Note: $Note" }))

Set-Content (Join-Path $out 'run.txt') $report

exit $(if ($exit) { $exit } else { 0 })

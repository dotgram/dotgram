<#
.SYNOPSIS
	Runs a timing as an announced window: the window lock, the wait for builds, the quiet check, the state of what is measured read and written down, and the command started on the stand's half
	of the machine (`taskset`, logical processors 0-7 and 16-23; D147).

.DESCRIPTION
	For a timing that is not BenchmarkDotNet: a run of the stand (`--stand`, `--stand-paired`, the linearity modes) or a script that starts processes of its own. `Run-Bdn.ps1` does this for BDN and
	reads the workers back; this script does not read its children back: they start on the mask because the command is started under `taskset`, and every process it starts inherits it.

	In order:
	  1. refuses (exit 3) when another window is open;
	  2. takes the window (WindowLib.ps1, Enter-Window): the lock, the announcement, and a wait of at most -WaitMinutes for the builds that hold the builds lock (benchmarks/Aside.sh) to end,
	     naming them; they do not end in time, exit 3;
	  3. the quiet check: refuses (exit 5) above 6% of the machine in use, or a build tool above 0.15 core, over three seconds (a build that did not go through Aside.sh);
	  4. reads the state of the repository the script or the command's first file sits in (HEAD, modified tracked files) and the sha256 of every file named in -Artifacts. A quotable run refuses
	     (exit 5) on modified tracked files; -Probe lets it through, stamped, and its numbers are not quoted;
	  5. runs `taskset -c <cpus> <command>` with DOTGRAM_WINDOW_HOLDER set (a stand run inside knows it is inside this window and announces nothing of its own), the output shown and written to
	     output.txt, and writes run.txt: the quiet reading, the state, the processors, the exit code. Both go to <scratch>/window/<label>-<time> (/ramdisk/build/dotgram on this machine).
	This script itself is not pinned: it waits on the other half while the command runs.

	The priority cannot be raised here (no privilege to lower a nice value); every timing runs at the default priority and run.txt says so.

.EXAMPLE
	pwsh benchmarks/Run-Announced.ps1 -Label stand -Command dotnet, benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll, --stand, --repeat, 5 -SlotMinutes 40

.EXAMPLE
	pwsh benchmarks/Run-Announced.ps1 -Label fix-first-validate -Script benchmarks/FirstCall/Fix/Run-Validate.ps1 -SlotMinutes 15 -Artifacts <harness dll>, <library dll>
#>
param(
	[Parameter(Mandatory)][string]$Label,
	[string]$Script,
	[string[]]$Command = @(),
	[string[]]$Arguments = @(),
	[double]$SlotMinutes = 15,
	[double]$WaitMinutes = 20,
	[string[]]$Artifacts = @(),
	[string]$Note = '',
	[string]$Root,
	[switch]$Probe
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

if ([bool]$Script -eq ($Command.Count -gt 0)) { Stop-Run 2 'Give -Script (a PowerShell script) or -Command (an executable and its arguments), one of the two.' }

if ($Script) {
	$Script  = (Resolve-Path $Script).Path
	$Command = @((Get-Process -Id $PID).Path, '-NoProfile', '-File', $Script) + $Arguments
	$anchor  = Split-Path $Script
}
else {
	$Command = $Command + $Arguments
	$anchor  = if ($Command.Count -gt 1 -and (Test-Path $Command[1])) { Split-Path (Resolve-Path $Command[1]).Path } else { (Get-Location).Path }
}

if (-not $Root) { $Root = Join-Path (Get-ScratchRoot) 'window' }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$out   = Join-Path $Root "$Label-$stamp"
$cpus  = Get-StandCpus

if ((Get-WindowState) -eq 'Busy') { Stop-Run 3 "Another timing window is open: $((Get-WindowLines) -join '; ')" }

$started = Get-Date
$what    = "Run-Announced.ps1 $Label$(if ($Probe) { ' [PROBE]' })$(if ($Note) { ' [' + $Note + ']' })"
$window  = Enter-Window $what $started.AddMinutes($SlotMinutes) $WaitMinutes

if ($window -is [string]) { Stop-Run 3 $window }

$exit   = $null
$report = @()

try {
	$quiet = Measure-Quiet
	$quiet.Text

	if (-not $quiet.Quiet) { Stop-Run 5 "The machine is not quiet: $($quiet.Why). $($quiet.Text) Nothing was started." }

	$root     = git -C $anchor rev-parse --show-toplevel 2>$null
	$head     = if ($root) { (git -C $root rev-parse --short HEAD) } else { 'not in a repository' }
	$modified = if ($root) { @(git -C $root status --porcelain --untracked-files=no 2>$null) } else { @() }
	$state    = @("State of the repository at $anchor, read at $((Get-Date).ToString('HH:mm:ss')): HEAD $head; $($modified.Count) tracked files modified$(if ($modified.Count -gt 0) { ' (' + (($modified | Select-Object -First 3) -join '; ') + ')' }).")

	foreach ($artifact in $Artifacts) {
		if (Test-Path $artifact) { $item = Get-Item $artifact; $state += "Artifact $($item.FullName): sha256 $((Get-FileHash $item.FullName -Algorithm SHA256).Hash.Substring(0, 16)), $($item.Length) bytes, written $($item.LastWriteTime.ToString('HH:mm:ss'))." }
		else { $state += "Artifact ${artifact}: NOT FOUND." }
	}

	$state

	if ($modified.Count -gt 0) {
		if ($Probe) { $state += 'PROBE: allowed on a repository with modified tracked files; its numbers are not to be quoted.' }
		else { Stop-Run 5 "The repository has $($modified.Count) modified tracked files ($(($modified | Select-Object -First 3) -join '; ')). A quotable run needs a clean tree (or -Probe, and the numbers are then not quoted). Nothing was started." }
	}

	New-Item -ItemType Directory -Force $out | Out-Null
	"Window open $((Get-Date).ToString('HH:mm:ss')), until $($started.AddMinutes($SlotMinutes).ToString('HH:mm:ss')): $what; processors $cpus, out $out"

	$pinned = Get-PinnedCommand $Command[0] ($Command | Select-Object -Skip 1)
	& $pinned[0] ($pinned | Select-Object -Skip 1) 2>&1 | Tee-Object -FilePath (Join-Path $out 'output.txt')
	$exit = $LASTEXITCODE

	$report = @(
		"# $Label, $($started.ToString('yyyy-MM-dd HH:mm')) - $((Get-Date).ToString('HH:mm'))",
		'',
		"Command: $($Command -join ' '). Machine $([Environment]::MachineName), started under taskset -c $cpus (every process it starts inherits the processors; not read back), default priority.",
		$quiet.Text
	) + $state + @("Exit code: $exit.", $(if ($Note) { "Note: $Note" }))

	Set-Content (Join-Path $out 'run.txt') $report
}
finally {
	Exit-Window $window "the window of pid $PID ended: $what"
}

exit $(if ($exit) { $exit } else { 0 })

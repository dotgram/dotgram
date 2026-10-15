<#
.SYNOPSIS
	Runs a timing as an announced window: the window lock, the wait for builds, the quiet check, the state of what is measured read and written down, and the command started on the stand's half
	of the machine (`taskset`, logical processors 0-7 and 16-23; D147).

.DESCRIPTION
	For a timing that is not BenchmarkDotNet: a run of the stand (`--stand`, `--stand-paired`, the linearity modes) or a script that starts processes of its own. `Run-Bdn.ps1` does this for BDN and
	reads the workers back; this script does not read its children back: they start on the mask because the command is started under `taskset`, and every process it starts inherits it.

	The window is SHARED by default: builds and tests go on beside it on the other half of the machine (benchmarks/Aside.sh pins them there), and only the stand's processors are held quiet.
	-Exclusive (or DOTGRAM_WINDOW_EXCLUSIVE=1) opens the old kind: nothing heavy runs on either half. Use it when the numbers decide something precise; the two halves share the package's power
	and boost budget and the memory controller, which a mask does not divide (benchmarks/README.md, *The window on this machine*, has what a build beside a shared window costs).

	In order:
	  1. refuses (exit 3) when another window is open;
	  2. takes the window (WindowLib.ps1, Enter-Window): the lock and the announcement, which names the kind. An exclusive window then waits at most -WaitMinutes for the builds that hold
	     the builds lock (benchmarks/Aside.sh) to end, naming them; they do not end in time, exit 3. A shared one does not wait for them;
	  3. the quiet check: waits at most -WaitMinutes for quiet, saying what is busy, and then refuses (exit 5). Exclusive: above 6% of the machine in use, or a build tool above 0.15 core,
	     over three seconds (a build that did not go through Aside.sh). Shared: the .NET processes of this user that may run on the stand's processors (a build started before the window)
	     are moved to the other half first, and then above 6% of the stand's processors in use, or a build tool that may run on them;
	  4. reads the state of the repository the script or the command's first file sits in (HEAD, modified tracked files) and the sha256 of every file named in -Artifacts. A quotable run refuses
	     (exit 5) on modified tracked files; -Probe lets it through, stamped, and its numbers are not quoted;
	  5. runs `taskset -c <cpus> <command>` with DOTGRAM_WINDOW_HOLDER set (a stand run inside knows it is inside this window and announces nothing of its own), the output shown and written to
	     output.txt, and writes run.txt: the kind of the window, the quiet reading, the state, the processors, how busy each half of the machine was while the command ran, the exit code.
	     Both go to <scratch>/window/<label>-<time> (/ramdisk/build/dotgram on this machine).
	This script itself is not pinned; it only waits while the command runs.

	The priority cannot be raised here (no privilege to lower a nice value); every timing runs at the default priority and run.txt says so.

.EXAMPLE
	pwsh benchmarks/Run-Announced.ps1 -Label stand -Command dotnet, .build/bin/DotGram.Benchmarks/release/DotGram.Benchmarks.dll, --stand, --repeat, 5 -SlotMinutes 40

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
	[switch]$Probe,
	[switch]$Exclusive
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

$Command   = @(Split-FileList $Command)
$Arguments = @(Split-FileList $Arguments)
$Artifacts = @(Split-FileList $Artifacts)

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
$window  = Enter-Window $what $started.AddMinutes($SlotMinutes) $WaitMinutes -Shared:(-not $Exclusive)

if ($window -is [string]) { Stop-Run 3 $window }

# Inside a window already (Enter-Window returned $null): the kind is that window's.
$kind = if ($window) { $window.Kind } else { Get-WindowKind }

$exit   = $null
$report = @()

try {
	$quiet = Wait-Quiet $WaitMinutes -Shared:($kind -eq 'shared')
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
	"Window open $((Get-Date).ToString('HH:mm:ss')), until $($started.AddMinutes($SlotMinutes).ToString('HH:mm:ss')): $what; $kind; processors $cpus, out $out"

	$pinned = Get-PinnedCommand $Command[0] ($Command | Select-Object -Skip 1)
	$times0 = Read-CpuTimes
	$clock  = [Diagnostics.Stopwatch]::StartNew()
	& $pinned[0] ($pinned | Select-Object -Skip 1) 2>&1 | Tee-Object -FilePath (Join-Path $out 'output.txt')
	$exit   = $LASTEXITCODE
	$times1 = Read-CpuTimes
	$during = "While the command ran ($([math]::Round($clock.Elapsed.TotalMinutes, 1)) min), on average: the stand's processors $cpus $(Get-BusyCpus $times0 $times1 $cpus) busy (the timing itself among them), the other half $(Get-AsideCpus) $(Get-BusyCpus $times0 $times1 (Get-AsideCpus)) busy."
	$during

	$report = @(
		"# $Label, $($started.ToString('yyyy-MM-dd HH:mm')) - $((Get-Date).ToString('HH:mm'))",
		'',
		"Command: $($Command -join ' '). Machine $([Environment]::MachineName), started under taskset -c $cpus (every process it starts inherits the processors; not read back), default priority.",
		$(if ($kind -eq 'shared') { "Window: SHARED. Builds and tests may have run on the other half ($(Get-AsideCpus)) while this timed; the line below says how busy it was. Not for a figure that needs an exclusive window." } else { 'Window: EXCLUSIVE. Nothing heavy was let start on either half while this timed.' }),
		$quiet.Text
	) + @($quiet.Moved | ForEach-Object { "Moved to the other half before the start: $_." }) + @($during) + $state + @("Exit code: $exit.", $(if ($Note) { "Note: $Note" }))

	Set-Content (Join-Path $out 'run.txt') $report
}
finally {
	Exit-Window $window "the window of pid $PID ended: $what"
}

exit $(if ($exit) { $exit } else { 0 })

<#
.SYNOPSIS
	Several BenchmarkDotNet runs as ONE window: `Run-Bdn.ps1` once per step, with the window taken once by this script (WindowLib.ps1, Enter-Window: the lock, the announcement, the wait for builds) and let go once, so that there is no gap between the steps.

.DESCRIPTION
	The steps are hashtables of `Label` and `BdnArgs` (and optionally `Note`, `Commit`, `LimitMinutes`), run in order under one announcement of `-SlotMinutes`. A step that fails (a worker off the stand's processors, exit 4; the machine not quiet, 5; the step's own limit, 6) stops the queue: what has not run is
	named, and it is to be run in a window of its own, never shortened. The window is let go when the queue ends, however it ends (and by the kernel if this process dies). Every step writes its own run.txt beside its artifacts.

.EXAMPLE
	pwsh benchmarks/Run-BdnQueue.ps1 -Assembly <dll> -SlotMinutes 55 -Steps @(@{ Label = 'fix-hot'; BdnArgs = '--filter','*FixAgainstQuickFix*' }, @{ Label = 'fix-cold'; BdnArgs = '--filter','*FixDictionaryFirstCall*' })
#>
param(
	[Parameter(Mandatory)][string]$Assembly,
	[Parameter(Mandatory)][hashtable[]]$Steps,
	[double]$SlotMinutes = 60,
	[string]$Commit = '',
	[string]$Note = '',
	[double]$WaitMinutes = 20
)

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

Assert-Linux

$script  = Join-Path $PSScriptRoot 'Run-Bdn.ps1'
$started = Get-Date
$labels  = ($Steps | ForEach-Object { $_.Label }) -join ' + '
$what    = "Run-BdnQueue.ps1 $labels$(if ($Note) { ' [' + $Note + ']' })"
$window  = Enter-Window $what $started.AddMinutes($SlotMinutes) $WaitMinutes

if ($window -is [string]) { Stop-Run 3 $window }

$done   = @()
$stoppedAt = $null

try {
	foreach ($step in $Steps) {
		$arguments = @{ Assembly = $Assembly; Label = $step.Label; BdnArgs = $step.BdnArgs; Within = $true; LimitMinutes = $(if ($step.LimitMinutes) { $step.LimitMinutes } else { $SlotMinutes }) }

		if ($Commit -or $step.Commit) { $arguments.Commit = $(if ($step.Commit) { $step.Commit } else { $Commit }) }

		if ($Note -or $step.Note) { $arguments.Note = $(if ($step.Note) { $step.Note } else { $Note }) }

		"=== step $($step.Label), $((Get-Date).ToString('HH:mm:ss'))"
		& $script @arguments
		$code = $LASTEXITCODE
		"=== step $($step.Label) ended, exit $code, $((Get-Date).ToString('HH:mm:ss'))"
		$done += "$($step.Label): $code"

		if ($code -ne 0) { $stoppedAt = $step.Label; break }
	}
}
finally {
	Exit-Window $window "the window of pid $PID ended: $what"
}

"Queue: $($done -join '; ')"

if ($stoppedAt) {
	$rest = @($Steps | ForEach-Object { $_.Label } | Where-Object { $done -notmatch "^$([regex]::Escape($_)):" })
	"STOPPED at step $stoppedAt; not run: $($rest -join ', '). What was not run goes to a window of its own, not shortened."
	exit 7
}

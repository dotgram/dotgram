<#
.SYNOPSIS
	Several BenchmarkDotNet runs as ONE announced window: `Run-Bdn.ps1` once per step, with the window announced once by this script and put back to idle once, so that there is no gap between the steps.

.DESCRIPTION
	The steps are hashtables of `Label` and `BdnArgs` (and optionally `Note`, `Commit`, `LimitMinutes`), run in order under one announcement of `-SlotMinutes`. A step that fails (a worker off the mask, exit 4; the machine not quiet, 5; the step's own limit, 6) stops the queue: what has not run is
	named, and it is to be run in a window of its own, never shortened. The window is put back to idle when the queue ends, however it ends. Every step writes its own run.txt beside its artifacts.

.EXAMPLE
	& .\benchmarks\Run-BdnQueue.ps1 -Assembly <dll> -SlotMinutes 55 -Steps @(@{ Label = 'fix-hot'; BdnArgs = '--filter','*FixAgainstQuickFix*' }, @{ Label = 'fix-cold'; BdnArgs = '--filter','*FixDictionaryFirstCall*' })
#>
param(
	[Parameter(Mandatory)][string]$Assembly,
	[Parameter(Mandatory)][hashtable[]]$Steps,
	[double]$SlotMinutes = 60,
	[string]$Commit = '',
	[string]$Note = ''
)

$window  = Join-Path ([IO.Path]::GetTempPath()) 'dotgram-timing-window.txt'
$script  = Join-Path $PSScriptRoot 'Run-Bdn.ps1'
$first   = (Get-Content $window -ErrorAction SilentlyContinue | Select-Object -First 1)

if ($first -like 'pid *') {
	$owner = 0

	if ([int]::TryParse($first.Substring(4), [ref]$owner) -and $owner -ne $PID -and (Get-Process -Id $owner -ErrorAction SilentlyContinue)) { Write-Error "Another timing window is announced and its process is alive: $(Get-Content $window)"; exit 3 }
}

$started = Get-Date
$labels  = ($Steps | ForEach-Object { $_.Label }) -join ' + '
Set-Content $window @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($started.AddMinutes($SlotMinutes).ToString('yyyy-MM-dd HH:mm:ss'))", "what Run-BdnQueue.ps1 $labels$(if ($Note) { ' [' + $Note + ']' })")

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
	Set-Content $window @('idle', "since $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))", "why the window of pid $PID ended: Run-BdnQueue.ps1 $labels")
}

"Queue: $($done -join '; ')"

if ($stoppedAt) {
	$rest = @($Steps | ForEach-Object { $_.Label } | Where-Object { $done -notmatch "^$([regex]::Escape($_)):" })
	"STOPPED at step $stoppedAt; not run: $($rest -join ', '). What was not run goes to a window of its own, not shortened."
	exit 7
}

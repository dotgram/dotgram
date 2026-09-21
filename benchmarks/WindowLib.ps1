# The window announcement, in one place: the file is `dotgram-timing-window.txt` in the temp directory, its first line is `idle` or `pid <n>`, and a live announcement carries the START TIME of the process that made it
# (`process-start <UTC, round-trip format>`) because Windows recycles process ids (2026-09-21: two pids were reused twice in twenty-four minutes): "the pid is alive" says nothing about whether the announcer is.
# Dot-source it: `. (Join-Path $PSScriptRoot 'WindowLib.ps1')`.

function Get-WindowFile { Join-Path ([IO.Path]::GetTempPath()) 'dotgram-timing-window.txt' }

function Get-ProcessStartText([int]$id) {
	(Get-Process -Id $id -ErrorAction Stop).StartTime.ToUniversalTime().ToString('o')
}

# The lines of an announcement for the process that makes it.
function New-WindowAnnouncement([datetime]$started, [datetime]$until, [string]$what) {
	@("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", "process-start $(Get-ProcessStartText $PID)")
}

# What the file says: Idle; Alive (the announcing process exists and started when the file says); Gone (no such process); Reused (a process holds that pid but started at another time: the announcer is gone);
# AliveByPid (the file records no start time, so only the pid could be checked); Unreadable.
function Get-WindowOwnerState([string[]]$lines) {
	$first = if ($lines.Count -gt 0) { $lines[0].Trim() } else { '' }

	if ($first -eq 'idle' -or $first -eq '') { return 'Idle' }

	if ($first -notlike 'pid *') { return 'Unreadable' }

	$owner = 0

	if (-not [int]::TryParse($first.Substring(4), [ref]$owner)) { return 'Unreadable' }

	$process = Get-Process -Id $owner -ErrorAction SilentlyContinue

	if (-not $process) { return 'Gone' }

	$recorded = @($lines | Where-Object { $_ -like 'process-start *' } | Select-Object -First 1)

	if ($recorded.Count -eq 0) { return 'AliveByPid' }

	$then = [datetime]::Parse($recorded[0].Substring(14), [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)

	if ([math]::Abs(($process.StartTime.ToUniversalTime() - $then).TotalSeconds) -lt 2) { return 'Alive' }

	return 'Reused'
}

<#
.SYNOPSIS
	Says in one line whether a timing window is open: `pwsh benchmarks/Window.ps1`. Read it before you build, test or run anything on this machine.

.DESCRIPTION
	`dotgram-timing-window.txt` in the temp directory always exists and its FIRST LINE says what the machine is doing:
	  idle         no window; the lines below say when the last one ended and what it was
	  pid <n>      a window is announced by process <n>, with `started`, `until` and `what` below it
	Whether the file exists says nothing about whether a window is open: a missing file is a defect of the stand (something deleted it) and this script says so.
	A file that names a pid which is no longer alive is STALE: the run that announced it died without ending its window. The machine is free, and the file is put back to idle.

	Output is one line and it begins with a WORD, and the word is the answer: IDLE or STALE (the machine is free), BUSY (do not build, test or run anything, on either half), MISSING (the file was missing, a defect of the stand: created idle; tell the stand session) or UNREADABLE (treat as busy). The exit code repeats it: 0 IDLE/STALE, 1 BUSY/UNREADABLE, 2 MISSING.

	**A check that printed no word did not run.** An exit code alone cannot tell "the script says the file is missing" (2) from "the script is not in this checkout" (the shell's own code): a caller reads the word, and if the script cannot be found the answer is NOT CHECKED, which is a refusal to proceed and never a pass. From a script:
	    if (-not (Test-Path benchmarks/Window.ps1)) { 'NOT CHECKED: Window.ps1 is not in this checkout'; exit 9 }
	    $word = (& pwsh -NoProfile -File benchmarks/Window.ps1) -join ' '
	    if ($word -notmatch '^(IDLE|STALE)') { "NOT FREE: $word"; exit 9 }

.EXAMPLE
	pwsh benchmarks/Window.ps1
#>
$window = Join-Path ([IO.Path]::GetTempPath()) 'dotgram-timing-window.txt'
$idle   = { param($why) Set-Content $window @('idle', "since $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))", "why $why") }

if (-not (Test-Path $window)) {
	& $idle 'the file was missing and Window.ps1 created it'
	'MISSING: the announcement file did not exist (something deleted it; a window may have been open and nobody could tell). Created it idle. Tell the stand session.'
	exit 2
}

$lines = @(Get-Content $window)
$first = if ($lines.Count -gt 0) { $lines[0].Trim() } else { '' }

if ($first -eq 'idle' -or $first -eq '') {
	'IDLE: no window is open. ' + (($lines | Select-Object -Skip 1) -join '; ')
	exit 0
}

if ($first -like 'pid *') {
	$owner = 0

	if (-not [int]::TryParse($first.Substring(4), [ref]$owner)) { "UNREADABLE first line '$first': treat the machine as busy and ask the stand session."; exit 1 }

	if (Get-Process -Id $owner -ErrorAction SilentlyContinue) {
		'BUSY: ' + ($lines -join '; ') + '. Do not build, test or run anything, on either half.'
		exit 1
	}

	& $idle "pid $owner was announced and is gone (the run died without ending its window): $($lines -join '; ')"
	"STALE: pid $owner announced a window and is gone; the machine is free, the file is idle again. Was: $($lines -join '; ')"
	exit 0
}

"UNREADABLE first line '$first': treat the machine as busy and ask the stand session."
exit 1

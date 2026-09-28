<#
.SYNOPSIS
	Says in one line whether a timing window is open: `pwsh benchmarks/Window.ps1`.

.DESCRIPTION
	A window is the lock `timing-window.lock` in the window directory (`/ramdisk/locks` on this machine; WindowLib.ps1, D147), held by the process that times for as long as it times. The kernel lets
	the lock go when that process ends, however it ends, so a window cannot outlive its holder. `timing-window.txt` beside it names the holder (pid, started, until, what).

	You do not need this before a build: `benchmarks/Aside.sh <command>` waits for a window by itself, and a window waits for what runs through it. It is for a person or a session that wants to
	know, and for scripts that must not start a timing inside somebody else's.

	Output is one line and it begins with a WORD, and the word is the answer: IDLE (no window), STALE (no window; the text named a holder that ended without saying so, and is put back to idle),
	WITHIN (a window is open and this process is inside it: DOTGRAM_WINDOW_HOLDER names its holder) or BUSY (a window is open: run heavy work through Aside.sh, which waits). The exit code repeats it:
	0 IDLE/STALE, 1 BUSY, 3 WITHIN.

	**A check that printed no word did not run.** A caller reads the word; if the script cannot be found the answer is NOT CHECKED, which is a refusal to proceed and never a pass.

.EXAMPLE
	pwsh benchmarks/Window.ps1
#>
. (Join-Path $PSScriptRoot 'WindowLib.ps1')

$state = Get-WindowState
$lines = Get-WindowLines

switch ($state) {
	'Idle' { 'IDLE: no window is open. ' + (($lines | Select-Object -Skip 1) -join '; '); exit 0 }
	'Stale' { 'STALE: no window is open; the announcement named a holder that ended without saying so, and is idle again.'; exit 0 }
	'Within' { 'WITHIN: ' + ($lines -join '; ') + '. This process is inside that window.'; exit 3 }
	default { 'BUSY: ' + ($lines -join '; ') + '. Run builds, tests and other heavy work through benchmarks/Aside.sh, which waits for the window to close.'; exit 1 }
}

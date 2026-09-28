# The timing window, the stand's half of the machine and where scratch goes, in one place (D147). Dot-source it: `. (Join-Path $PSScriptRoot 'WindowLib.ps1')`.
#
# A window is a LOCK, not the contents of a file. `timing-window.lock` in the window directory (`/ramdisk/locks` on this machine, or DOTGRAM_WINDOW_DIR) is held exclusively by the process that
# times, for as long as it times, and the kernel lets it go when that process ends, however it ends: there is no stale window and no pid to check against a start time. `timing-window.txt` beside it
# names the holder (pid, started, until, what) for whoever reads it; it is information, and the lock is the answer.
# `timing-builds.lock` is held SHARED by every build, test or other heavy run that goes through `benchmarks/Aside.sh` (or Enter-Aside below): a window waits for those to end before it starts, and
# they wait while a window is open. A process started inside a window inherits DOTGRAM_WINDOW_HOLDER, the holder's pid, and is inside it: it neither waits for the window nor announces another.
# The lock is flock(2) on Linux, taken through FileShare (FileShare.None is an exclusive flock, anything else a shared one), so `flock(1)` in a shell and this file see the same locks.

function Get-WindowDirectory {
	$directory = if ($env:DOTGRAM_WINDOW_DIR) { $env:DOTGRAM_WINDOW_DIR } elseif (Test-Path '/ramdisk') { '/ramdisk/locks' } else { [IO.Path]::GetTempPath() }

	[IO.Directory]::CreateDirectory($directory) | Out-Null
	$directory
}

function Get-WindowFile { Join-Path (Get-WindowDirectory) 'timing-window.txt' }
function Get-WindowLockFile { Join-Path (Get-WindowDirectory) 'timing-window.lock' }
function Get-BuildsLockFile { Join-Path (Get-WindowDirectory) 'timing-builds.lock' }

# Where a run's disposable output goes: never the SSD. DOTGRAM_SCRATCH, or /ramdisk/build/dotgram, or `dotgram` in the temp directory (the same rule as Stand.DefaultDirectory).
function Get-ScratchRoot {
	if ($env:DOTGRAM_SCRATCH) { return $env:DOTGRAM_SCRATCH }

	if (Test-Path '/ramdisk') { return '/ramdisk/build/dotgram' }

	Join-Path ([IO.Path]::GetTempPath()) 'dotgram'
}

# The logical processors a timing runs on: the first CCD with its SMT siblings (cores 0-7 are processors 0-7 and 16-23 here), which is also where the preferred cores are. DOTGRAM_STAND_CPUS overrides.
function Get-StandCpus { if ($env:DOTGRAM_STAND_CPUS) { $env:DOTGRAM_STAND_CPUS } else { '0-7,16-23' } }

# A taskset list ("0-7,16-23") as a sorted set of processor numbers, so that two spellings of one set compare equal.
function ConvertTo-CpuSet([string]$list) {
	$set = foreach ($part in $list.Trim().Split(',', [StringSplitOptions]::RemoveEmptyEntries)) {
		if ($part -match '^(\d+)-(\d+)$') { [int]$Matches[1]..[int]$Matches[2] } else { [int]$part }
	}

	(@($set) | Sort-Object -Unique) -join ','
}

# The processors a process may run on, as the kernel says (`Cpus_allowed_list` of its main thread; a child started under taskset has it on every thread). $null when the process is gone.
function Get-ProcessCpus([int]$id) {
	$line = Get-Content "/proc/$id/status" -ErrorAction SilentlyContinue | Where-Object { $_ -like 'Cpus_allowed_list:*' } | Select-Object -First 1

	if ($line) { $line.Substring($line.IndexOf(':') + 1).Trim() } else { $null }
}

# A command line that starts on the stand's half. On Linux the mask must be set on the child as it starts: `Process.ProcessorAffinity` pins only the main thread of the process that sets it, and the
# children a PowerShell pipeline starts do not inherit it (checked 2026-09-28: pwsh set to 0-7,16-23, its child read 0-31).
function Get-PinnedCommand([string]$exe, [string[]]$arguments) {
	@('taskset', '-c', (Get-StandCpus), $exe) + $arguments
}

# A refusal with its exit code. Write-Error under ErrorActionPreference Stop ends the script with 1 before an `exit` after it runs, which is how the documented codes were lost; this writes
# the message to the error stream and exits with the code (a `finally` of the caller still runs).
function Stop-Run([int]$code, [string]$message) {
	[Console]::Error.WriteLine($message)
	exit $code
}

# `pwsh -File script.ps1 -List 'a','b'` from bash hands the list over as ONE string, "a,b" (2026-09-19, a keep-list read that way deleted a queue's trees). A list parameter that arrives as a single
# element with commas in it is split; an argument that needs a comma of its own is passed from PowerShell, not through -File.
function Split-FileList([string[]]$list) {
	if ($list.Count -eq 1 -and $list[0].Contains(',')) { return $list[0].Split(',') }

	$list
}

function Assert-Linux {
	if (-not $IsLinux) { Stop-Run 2 'The stand times on Linux (D147): pinning is taskset and the window is flock. On another system a timing is not pinned and is not quotable.' }
}

# A lock taken without waiting: the open FileStream (hold it to hold the lock), or $null when somebody holds it in a way that conflicts.
function Open-Lock([string]$path, [switch]$Exclusive) {
	try {
		if ($Exclusive) { [IO.File]::Open($path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None) }
		else { [IO.File]::Open($path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite) }
	}
	catch [IO.IOException] { $null }
	catch [Management.Automation.MethodInvocationException] { if ($_.Exception.InnerException -is [IO.IOException]) { $null } else { throw } }
}

# Who holds a lock file, from /proc/locks: "pid <n> <command line>" for each holder (Linux; nothing elsewhere).
function Get-LockHolders([string]$path) {
	if (-not $IsLinux -or -not (Test-Path $path)) { return @() }

	$inode = (& stat -c '%i' $path).Trim()

	foreach ($line in Get-Content '/proc/locks' -ErrorAction SilentlyContinue) {
		$fields = $line -split '\s+'

		if ($fields.Count -ge 6 -and $fields[5] -like "*:$inode") {
			$holder = $fields[4]
			$command = (Get-Content "/proc/$holder/cmdline" -Raw -ErrorAction SilentlyContinue) -replace "`0", ' '
			"pid $holder $(if ($command) { $command.Trim() } else { '(gone)' })"
		}
	}
}

function Get-WindowLines { @(Get-Content (Get-WindowFile) -ErrorAction SilentlyContinue) }

function Set-WindowIdle([string]$why) {
	Set-Content (Get-WindowFile) @('idle', "since $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))", "why $why")
}

# Idle (nobody holds the window), Stale (nobody holds it, but the text still names a holder: it died without saying so; the text is put back to idle), Within (this process is inside the
# window that is open: DOTGRAM_WINDOW_HOLDER names its holder) or Busy.
function Get-WindowState {
	$probe = Open-Lock (Get-WindowLockFile)

	if ($probe) {
		$probe.Dispose()
		$lines = Get-WindowLines

		if ($lines.Count -gt 0 -and $lines[0] -like 'pid *') { Set-WindowIdle "the holder ended without saying so: $($lines -join '; ')"; return 'Stale' }

		return 'Idle'
	}

	$lines = Get-WindowLines

	if ($env:DOTGRAM_WINDOW_HOLDER -and $lines.Count -gt 0 -and $lines[0] -eq "pid $env:DOTGRAM_WINDOW_HOLDER") { return 'Within' }

	'Busy'
}

# Opens a window: takes the window lock, writes the announcement, then waits (at most -WaitMinutes) for the builds that hold the builds lock to end, naming them. Returns the handle to pass to
# Exit-Window, $null when this process is already inside a window (whose holder lets it go), or a string that begins with REFUSED.
function Enter-Window([string]$what, [datetime]$until, [double]$WaitMinutes = 20) {
	if ((Get-WindowState) -eq 'Within') { return $null }

	if ($env:DOTGRAM_ASIDE) { return 'REFUSED: this timing was started through Aside.sh, whose builds lock the window would wait for: run it directly, not through Aside.sh.' }

	# A shared probe (Window.ps1, Aside.sh) holds the lock for milliseconds: a few seconds of retries ride over it.
	$gate = $null

	for ($try = 0; $try -lt 25 -and -not $gate; $try++) {
		$gate = Open-Lock (Get-WindowLockFile) -Exclusive

		if (-not $gate) { Start-Sleep -Milliseconds 200 }
	}

	if (-not $gate) { return "REFUSED: another timing window is open: $((Get-WindowLines) -join '; ')" }

	$started = Get-Date
	Set-Content (Get-WindowFile) @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", 'state waiting for the builds that hold timing-builds.lock')
	$env:DOTGRAM_WINDOW_HOLDER = "$PID"

	$builds = $null
	$deadline = (Get-Date).AddMinutes($WaitMinutes)
	$said = [datetime]::MinValue

	while (-not ($builds = Open-Lock (Get-BuildsLockFile) -Exclusive)) {
		if ((Get-Date) -gt $deadline) {
			$holders = @(Get-LockHolders (Get-BuildsLockFile))
			Exit-Window @{ Gate = $gate } "it waited $WaitMinutes minutes for builds that did not end: $($holders -join '; ')"
			return "REFUSED: builds held the machine for $WaitMinutes minutes: $($holders -join '; ')"
		}

		if (((Get-Date) - $said).TotalSeconds -ge 60) { "Waiting for builds to end before the window opens: $((@(Get-LockHolders (Get-BuildsLockFile))) -join '; ')" | Write-Host; $said = Get-Date }

		Start-Sleep -Seconds 2
	}

	Set-Content (Get-WindowFile) @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", "state open since $((Get-Date).ToString('HH:mm:ss'))")

	@{ Gate = $gate; Builds = $builds; What = $what }
}

function Exit-Window($handle, [string]$why) {
	if ($handle -isnot [hashtable]) { return }

	Set-WindowIdle $why
	Remove-Item Env:DOTGRAM_WINDOW_HOLDER -ErrorAction SilentlyContinue

	if ($handle.Builds) { $handle.Builds.Dispose() }

	$handle.Gate.Dispose()
}

# For a heavy run that times nothing (a build, a test run, an allocation run): waits while a window is open, then holds the builds lock shared until Exit-Aside. $null inside a window.
function Enter-Aside {
	if ((Get-WindowState) -eq 'Within') { return $null }

	$said = $false

	while ($true) {
		$probe = Open-Lock (Get-WindowLockFile)

		if ($probe) {
			$probe.Dispose()
			$builds = Open-Lock (Get-BuildsLockFile)

			if ($builds) { return $builds }
		}

		if (-not $said) { "Waiting: a timing window is open: $((Get-WindowLines) -join '; ')" | Write-Host; $said = $true }

		Start-Sleep -Seconds 2
	}
}

function Exit-Aside($handle) { if ($handle) { $handle.Dispose() } }

# The quiet check: the CPU time of every readable process over three seconds, outside this one. Refuses above 6% of the machine, or when a build tool is above 0.15 core, which a small compile alone
# would not lift the total to 6% for. What it saw is returned either way, for run.txt.
function Measure-Quiet([double]$Percent = 6) {
	$tools = 'dotnet', 'MSBuild', 'VBCSCompiler', 'csc', 'vbcscompiler', 'cl', 'link', 'git', 'nuget', 'node', 'msbuild', 'cc1', 'cc1plus', 'ld', 'make', 'cargo', 'rustc', 'java'

	$sample = {
		$taken = @{}

		foreach ($process in Get-Process) {
			# Keyed by pid AND start time: a pid recycled inside the three seconds is another process.
			try { if ($process.Id -ne $PID -and $process.Id -ne 0) { $taken["$($process.Id)@$($process.StartTime.Ticks)"] = @($process.ProcessName, $process.TotalProcessorTime.TotalSeconds, $process.Id) } } catch { }
		}

		$taken
	}

	$before  = & $sample
	Start-Sleep -Seconds 3
	$after   = & $sample
	$cores   = [Environment]::ProcessorCount
	$busy    = @(foreach ($key in $after.Keys) { if ($before.ContainsKey($key)) { [pscustomobject]@{ Id = $after[$key][2]; Name = $after[$key][0]; Cores = [math]::Round(($after[$key][1] - $before[$key][1]) / 3, 3) } } })
	$total   = ($busy | Measure-Object Cores -Sum).Sum
	$share   = [math]::Round(100 * $total / $cores, 1)
	$top     = ($busy | Sort-Object Cores -Descending | Select-Object -First 3 | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores)" }) -join '; '
	$builders = @($busy | Where-Object { $_.Name -in $tools -and $_.Cores -gt 0.15 })
	$text    = "Quiet check before the start: $share% of the machine ($([math]::Round($total, 2)) of $cores logical processors) in use outside this script, three seconds; busiest: $top."
	$why     = if ($builders.Count -gt 0) { "a build tool is running: $(($builders | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores) cores" }) -join '; ')" } elseif ($share -gt $Percent) { "more than $Percent% of the machine is in use" } else { $null }

	[pscustomobject]@{ Quiet = -not $why; Why = $why; Text = $text }
}

# The timing window, the stand's half of the machine and where scratch goes, in one place (D147). Dot-source it: `. (Join-Path $PSScriptRoot 'WindowLib.ps1')`.
#
# A window is a LOCK, not the contents of a file. `timing-window.lock` in the window directory (`/ramdisk/locks` on this machine, or DOTGRAM_WINDOW_DIR) is held exclusively by the process that
# times, for as long as it times, and the kernel lets it go when that process ends, however it ends: there is no stale window and no pid to check against a start time. `timing-window.txt` beside it
# names the holder (pid, started, until, what) for whoever reads it; it is information, and the lock is the answer.
# `timing-builds.lock` is held SHARED by every build, test or other heavy run that goes through `benchmarks/Aside.sh` (or Enter-Aside below). A window is one of two kinds, named by the line `kind`
# of the announcement. An EXCLUSIVE window waits for those builds to end before it starts, and they wait while it is open: nothing heavy runs on either half. A SHARED window does not wait for them:
# the builds that are running are moved to the other half (Move-AsideProcesses), new ones started through Aside.sh run there at once, and the quiet check reads only the stand's processors. An
# announcement that names no kind (a stand run started by hand, StandWindow.cs) is exclusive. A process started inside a window inherits DOTGRAM_WINDOW_HOLDER, the holder's pid, and is inside it: it
# neither waits for the window nor announces another.
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

# The processors the stand does not use, where builds run beside a shared window: DOTGRAM_ASIDE_CPUS, or every online processor outside Get-StandCpus (the second CCD with its siblings here).
function Get-AsideCpus {
	if ($env:DOTGRAM_ASIDE_CPUS) { return $env:DOTGRAM_ASIDE_CPUS }

	$stand  = @((ConvertTo-CpuSet (Get-StandCpus)) -split ',')
	$online = @((ConvertTo-CpuSet (Get-Content '/sys/devices/system/cpu/online' -Raw)) -split ',')

	# Written as ranges ("8-15,24-31"), as taskset and a reader take it.
	$ranges = [Collections.Generic.List[string]]::new()
	$first  = $last = -2

	foreach ($cpu in @($online | Where-Object { $_ -notin $stand } | ForEach-Object { [int]$_ })) {
		if ($cpu -ne $last + 1) {
			if ($first -ge 0) { $ranges.Add($(if ($first -eq $last) { "$first" } else { "$first-$last" })) }

			$first = $cpu
		}

		$last = $cpu
	}

	if ($first -ge 0) { $ranges.Add($(if ($first -eq $last) { "$first" } else { "$first-$last" })) }

	$ranges -join ','
}

# The kind of the window that is open, from its announcement: 'shared', or 'exclusive' (also when the announcement names none).
function Get-WindowKind {
	$line = Get-WindowLines | Where-Object { $_ -like 'kind *' } | Select-Object -First 1

	if ($line -eq 'kind shared') { 'shared' } else { 'exclusive' }
}

# Moves this user's .NET processes that may run on every processor (a build, a test host, a compiler server or an MSBuild node started before a shared window opened) to Get-AsideCpus, every
# thread (`taskset -a`). A process already pinned is left alone: a timing on the stand's processors, or work already aside. So are pwsh and shells (what they start is matched on its own),
# everything that does not load the .NET runtime, and this process. Returns "pid <n> (<name>)" for each process moved, and keeps it (with its start time) for Restore-AsideProcesses.
function Move-AsideProcesses {
	$online = ConvertTo-CpuSet (Get-Content '/sys/devices/system/cpu/online' -Raw)
	$aside  = Get-AsideCpus
	$uid    = (& id -u).Trim()

	foreach ($directory in [IO.Directory]::GetDirectories('/proc')) {
		$id = [IO.Path]::GetFileName($directory)

		if ($id -notmatch '^\d+$' -or [int]$id -eq $PID) { continue }

		try { $status = [IO.File]::ReadAllLines("$directory/status") } catch { continue }

		$name    = (($status | Where-Object { $_ -like 'Name:*' } | Select-Object -First 1) -replace '^Name:\s*', '')
		$owner   = (($status | Where-Object { $_ -like 'Uid:*' } | Select-Object -First 1) -split '\s+')[1]
		$allowed = (($status | Where-Object { $_ -like 'Cpus_allowed_list:*' } | Select-Object -First 1) -replace '^Cpus_allowed_list:\s*', '')

		if ($owner -ne $uid -or -not $allowed -or (ConvertTo-CpuSet $allowed) -ne $online -or $name -in 'pwsh', 'bash', 'sh') { continue }

		try { if (-not [IO.File]::ReadAllText("$directory/maps").Contains('libcoreclr')) { continue } } catch { continue }

		& taskset -a -cp $aside $id *> $null

		if ($LASTEXITCODE -eq 0) {
			$script:MovedAside += , @($id, (Get-StartTime $id))
			"pid $id ($name)"
		}
	}
}

$script:MovedAside = @()

# The start time of a process (field 22 of /proc/<pid>/stat), so that a recycled pid is not taken for it; $null when it is gone.
function Get-StartTime([string]$id) {
	try { $stat = [IO.File]::ReadAllText("/proc/$id/stat") } catch { return $null }

	($stat.Substring($stat.LastIndexOf(')') + 2) -split ' ')[19]
}

# At the end of a shared window, puts back on every processor (a) the processes Move-AsideProcesses moved, if they are still the same processes and still exactly on the other half, and
# (b) this user's compiler servers (VBCSCompiler) that are exactly on the other half: a server outlives the build that started it there, and every build after the window would compile on half
# the machine until it idles out.
function Restore-AsideProcesses {
	$online = ConvertTo-CpuSet (Get-Content '/sys/devices/system/cpu/online' -Raw)
	$aside  = ConvertTo-CpuSet (Get-AsideCpus)
	$uid    = (& id -u).Trim()
	$put    = @($script:MovedAside | Where-Object { $_[1] -and (Get-StartTime $_[0]) -eq $_[1] } | ForEach-Object { $_[0] })

	foreach ($directory in [IO.Directory]::GetDirectories('/proc')) {
		$id = [IO.Path]::GetFileName($directory)

		if ($id -notmatch '^\d+$') { continue }

		try { if ([IO.File]::ReadAllText("$directory/comm").Trim() -ne 'VBCSCompiler') { continue } } catch { continue }

		$owner = ((Get-Content "$directory/status" -ErrorAction SilentlyContinue | Where-Object { $_ -like 'Uid:*' } | Select-Object -First 1) -split '\s+')[1]

		if ($owner -eq $uid) { $put += $id }
	}

	foreach ($id in $put | Select-Object -Unique) {
		$allowed = Get-ProcessCpus ([int]$id)

		if ($allowed -and (ConvertTo-CpuSet $allowed) -eq $aside) { & taskset -a -cp $online $id *> $null }
	}

	$script:MovedAside = @()
}

# The busy and total time of each logical processor, from /proc/stat: processor number -> @(total, idle and iowait).
function Read-CpuTimes {
	$times = @{}

	foreach ($line in [IO.File]::ReadAllLines('/proc/stat')) {
		if ($line -match '^cpu(\d+) ') {
			$values = @(($line -split '\s+') | Select-Object -Skip 1 | ForEach-Object { [long]$_ })
			$times[[int]$Matches[1]] = @(($values | Measure-Object -Sum).Sum, ($values[3] + $values[4]))
		}
	}

	$times
}

# How many of the processors in a taskset list were busy between two Read-CpuTimes, summed (0.5 is half of one processor's time).
function Get-BusyCpus($before, $after, [string]$list) {
	$busy = 0.0

	foreach ($cpu in (ConvertTo-CpuSet $list) -split ',') {
		$cpu   = [int]$cpu
		$total = $after[$cpu][0] - $before[$cpu][0]

		if ($total -gt 0) { $busy += ($total - ($after[$cpu][1] - $before[$cpu][1])) / $total }
	}

	[math]::Round($busy, 2)
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
# element with commas in it is split back into its parts. One of those parts can itself be a comma list of its own (`--only a,b`, `--aa-only a,b`): write its comma TWICE, `a,,b`, which survives
# bash untouched (bash gives no meaning to a comma) and tells the element's own comma apart from the one separating elements (2026-09-29, `--only a,b` sent through Run-Announced.ps1 from bash read
# as `--only a` plus a stray element `b`, taken as the output directory, and silently changed what was timed).
function Split-FileList([string[]]$list) {
	if ($list.Count -ne 1 -or -not $list[0].Contains(',')) { return $list }

	$marker = [char]1

	($list[0] -replace ',,', $marker) -split ',' | ForEach-Object { $_ -replace $marker, ',' }
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
		# Read, and put back to idle, while the probe still holds the lock: once it is let go a new window may write its announcement, and this must not overwrite it.
		try {
			$lines = Get-WindowLines

			if ($lines.Count -gt 0 -and $lines[0] -like 'pid *') { Set-WindowIdle "the holder ended without saying so: $($lines -join '; ')"; return 'Stale' }

			return 'Idle'
		}
		finally {
			$probe.Dispose()
		}
	}

	$lines = Get-WindowLines

	if ($env:DOTGRAM_WINDOW_HOLDER -and $lines.Count -gt 0 -and $lines[0] -eq "pid $env:DOTGRAM_WINDOW_HOLDER") { return 'Within' }

	'Busy'
}

# Opens a window: takes the window lock, writes the announcement, then waits (at most -WaitMinutes) for the builds that hold the builds lock to end, naming them. Returns the handle to pass to
# Exit-Window, $null when this process is already inside a window (whose holder lets it go), or a string that begins with REFUSED.
# -Shared opens a shared window (it does not wait for the builds; the quiet check, Wait-Quiet -Shared, moves them aside). DOTGRAM_WINDOW_EXCLUSIVE=1 makes every window exclusive.
function Enter-Window([string]$what, [datetime]$until, [double]$WaitMinutes = 20, [switch]$Shared) {
	if ((Get-WindowState) -eq 'Within') { return $null }

	if ($env:DOTGRAM_WINDOW_EXCLUSIVE -eq '1') { $Shared = $false }

	$kind = if ($Shared) { 'kind shared' } else { 'kind exclusive' }

	if ($env:DOTGRAM_ASIDE) { return 'REFUSED: this timing was started through Aside.sh, whose builds lock the window would wait for: run it directly, not through Aside.sh.' }

	# A shared probe (Window.ps1, Aside.sh) holds the lock for milliseconds: a few seconds of retries ride over it.
	$gate = $null

	for ($try = 0; $try -lt 25 -and -not $gate; $try++) {
		$gate = Open-Lock (Get-WindowLockFile) -Exclusive

		if (-not $gate) { Start-Sleep -Milliseconds 200 }
	}

	if (-not $gate) { return "REFUSED: another timing window is open: $((Get-WindowLines) -join '; ')" }

	$started = Get-Date
	$env:DOTGRAM_WINDOW_HOLDER = "$PID"

	if ($Shared) {
		Set-Content (Get-WindowFile) @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", $kind, "state open since $((Get-Date).ToString('HH:mm:ss')); builds run on processors $(Get-AsideCpus)")

		return @{ Gate = $gate; Builds = $null; What = $what; Kind = 'shared' }
	}

	Set-Content (Get-WindowFile) @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", $kind, 'state waiting for the builds that hold timing-builds.lock')

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

	Set-Content (Get-WindowFile) @("pid $PID", "started $($started.ToString('yyyy-MM-dd HH:mm:ss'))", "until $($until.ToString('yyyy-MM-dd HH:mm:ss'))", "what $what", $kind, "state open since $((Get-Date).ToString('HH:mm:ss'))")

	@{ Gate = $gate; Builds = $builds; What = $what; Kind = 'exclusive' }
}

function Exit-Window($handle, [string]$why) {
	if ($handle -isnot [hashtable]) { return }

	# While the gate is still held: a window that opens next moves what it needs itself.
	if ($handle.Kind -eq 'shared') { Restore-AsideProcesses }

	Set-WindowIdle $why
	Remove-Item Env:DOTGRAM_WINDOW_HOLDER -ErrorAction SilentlyContinue

	if ($handle.Builds) { $handle.Builds.Dispose() }

	$handle.Gate.Dispose()
}

# For a heavy run that times nothing (a build, a test run, an allocation run): waits while an exclusive window is open, then holds the builds lock shared until Exit-Aside. $null inside a window.
# Beside a shared window it does not wait: the caller starts its processes through Get-AsideCommand, which pins them to the other half (a pwsh process cannot pin what it starts by itself:
# Get-PinnedCommand's note).
function Enter-Aside {
	$script:AsidePin = $null

	# Inside an Aside already: the outer one holds the builds lock, and waiting at the gate here would hold a window that is waiting for the outer one. An outer Aside.sh that pinned its command
	# pinned this process too, and what it starts inherits that.
	if ($env:DOTGRAM_ASIDE) { return $null }

	if ((Get-WindowState) -eq 'Within') { return $null }

	$said = $false

	while ($true) {
		$probe = Open-Lock (Get-WindowLockFile)

		$shared = -not $probe -and (Get-WindowKind) -eq 'shared'

		if ($probe) { $probe.Dispose() }

		if ($probe -or $shared) {
			$builds = Open-Lock (Get-BuildsLockFile)

			if ($builds) {
				$env:DOTGRAM_ASIDE = '1'

				if ($shared) { $script:AsidePin = Get-AsideCpus; "A shared timing window is open: this run's processes go on processors $script:AsidePin." | Write-Host }

				return $builds
			}
		}

		if (-not $said) { "Waiting: an exclusive timing window is open: $((Get-WindowLines) -join '; ')" | Write-Host; $said = $true }

		Start-Sleep -Seconds 2
	}
}

function Exit-Aside($handle) {
	if ($handle) { $handle.Dispose(); Remove-Item Env:DOTGRAM_ASIDE -ErrorAction SilentlyContinue }

	$script:AsidePin = $null
}

# A command line for a process an Aside starts: under `taskset` on the other half when Enter-Aside found a shared window open, as it is otherwise.
function Get-AsideCommand([string]$exe, [string[]]$arguments) {
	if ($script:AsidePin) { @('taskset', '-c', $script:AsidePin, $exe) + $arguments } else { @($exe) + $arguments }
}

# The quiet check: the CPU time of every readable process over three seconds, outside this one. Refuses above 6% of the machine, or when a build tool is above 0.15 core, which a small compile alone
# would not lift the total to 6% for. -Shared (a shared window): the 6% is of the stand's processors, read from /proc/stat, and a build tool counts only when it may run on them (one pinned to
# the other half is what a shared window lets run). What it saw is returned either way, for run.txt, with how busy each half was.
function Measure-Quiet([double]$Percent = 6, [switch]$Shared) {
	$tools = 'dotnet', 'MSBuild', 'VBCSCompiler', 'csc', 'vbcscompiler', 'cl', 'link', 'git', 'nuget', 'node', 'msbuild', 'cc1', 'cc1plus', 'ld', 'make', 'cargo', 'rustc', 'java'

	$sample = {
		$taken = @{}

		foreach ($process in Get-Process) {
			# Keyed by pid AND start time: a pid recycled inside the three seconds is another process.
			try { if ($process.Id -ne $PID -and $process.Id -ne 0) { $taken["$($process.Id)@$($process.StartTime.Ticks)"] = @($process.ProcessName, $process.TotalProcessorTime.TotalSeconds, $process.Id) } } catch { }
		}

		$taken
	}

	$stand   = Get-StandCpus
	$aside   = Get-AsideCpus
	$before  = & $sample
	$cpus0   = Read-CpuTimes
	Start-Sleep -Seconds 3
	$cpus1   = Read-CpuTimes
	$after   = & $sample
	$cores   = [Environment]::ProcessorCount
	$busy    = @(foreach ($key in $after.Keys) { if ($before.ContainsKey($key)) { [pscustomobject]@{ Id = $after[$key][2]; Name = $after[$key][0]; Cores = [math]::Round(($after[$key][1] - $before[$key][1]) / 3, 3) } } })
	$total   = ($busy | Measure-Object Cores -Sum).Sum
	$standBusy = Get-BusyCpus $cpus0 $cpus1 $stand
	$asideBusy = Get-BusyCpus $cpus0 $cpus1 $aside
	$standCount = @((ConvertTo-CpuSet $stand) -split ',').Count
	$asideCount = @((ConvertTo-CpuSet $aside) -split ',').Count
	$top     = ($busy | Sort-Object Cores -Descending | Select-Object -First 3 | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores)" }) -join '; '
	$builders = @($busy | Where-Object { $_.Name -in $tools -and $_.Cores -gt 0.15 })
	$halves  = "the stand's processors $stand $standBusy of $standCount busy, the other half $asideBusy of $asideCount"

	if ($Shared) {
		$standSet = @((ConvertTo-CpuSet $stand) -split ',')
		$builders = @($builders | Where-Object { $allowed = Get-ProcessCpus $_.Id; -not $allowed -or @((ConvertTo-CpuSet $allowed) -split ',' | Where-Object { $_ -in $standSet }).Count -gt 0 })
		$share    = [math]::Round(100 * $standBusy / $standCount, 1)
		$text     = "Quiet check before the start (shared window): $share% of the stand's processors in use, three seconds ($halves); busiest process: $top."
		$why      = if ($builders.Count -gt 0) { "a build tool may run on the stand's processors: $(($builders | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores) cores" }) -join '; ')" } elseif ($share -gt $Percent) { "more than $Percent% of the stand's processors are in use" } else { $null }
	}
	else {
		$share    = [math]::Round(100 * $total / $cores, 1)
		$text     = "Quiet check before the start (exclusive window): $share% of the machine ($([math]::Round($total, 2)) of $cores logical processors) in use outside this script, three seconds ($halves); busiest: $top."
		$why      = if ($builders.Count -gt 0) { "a build tool is running: $(($builders | ForEach-Object { "$($_.Name) (pid $($_.Id)) $($_.Cores) cores" }) -join '; ')" } elseif ($share -gt $Percent) { "more than $Percent% of the machine is in use" } else { $null }
	}

	[pscustomobject]@{ Quiet = -not $why; Why = $why; Text = $text; StandBusy = $standBusy; AsideBusy = $asideBusy }
}

# The quiet check, repeated until it passes or -WaitMinutes have gone by, saying what is busy each time it fails: a build somebody started by hand ends by itself, and a window that waits
# for it costs less than one refused and asked for again. -Shared moves the .NET processes that may run on the stand's processors to the other half before every reading (Move-AsideProcesses),
# and the reading carries the list (Moved). Returns the last reading; the caller refuses when it is still not quiet.
function Wait-Quiet([double]$WaitMinutes = 20, [switch]$Shared) {
	$deadline = (Get-Date).AddMinutes($WaitMinutes)
	$moved    = @()

	while ($true) {
		if ($Shared) {
			$now = @(Move-AsideProcesses)

			foreach ($line in $now) { Write-Host "Moved to processors $(Get-AsideCpus): $line" }

			$moved += $now
		}

		$quiet = Measure-Quiet -Shared:$Shared
		$quiet | Add-Member Moved $moved

		if ($quiet.Quiet -or (Get-Date) -gt $deadline) { return $quiet }

		Write-Host "Waiting for the machine to be quiet ($((Get-Date).ToString('HH:mm:ss')), until $($deadline.ToString('HH:mm:ss'))): $($quiet.Why). $($quiet.Text)"
		Start-Sleep -Seconds 10
	}
}

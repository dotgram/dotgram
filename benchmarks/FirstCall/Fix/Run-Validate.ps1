<#
.SYNOPSIS
	What the first call of validation costs, the walk against the generated rule, one fresh process
	per sample.

.DESCRIPTION
	A first call is a property of a process, not of a loop: the runtime compiles a method the first
	time it is reached, and the second call in the same process is no longer a first call. So every
	sample here is its own `fixfirst` process, and the roads are asked in alternation within each
	launch so that whatever drifts over the run drifts through all of them.

	Three cells a message type, not two. The third is one road asked a second time under another
	name, interleaved like the rest: the difference between those two cells is what this run can
	resolve, and a difference between the roads that is not several times it is not a difference.

	Per cell it prints the least, the median and the greatest of every launch, for the wall time of
	the call and for the time the runtime spent compiling during it, plus how many methods it
	compiled and their IL. The wall time answers "what does a consumer wait for"; the compiling
	time answers "how much of that is the method being large". Printing all three of least, median
	and greatest is what makes one bad launch visible instead of averaged in.

	The message body is empty for every type, so the work at run time is as equal as it can be made
	and what differs is the size of the method reached. What validation then reports is a list of
	what the type requires: the right size to print and the wrong thing to read anything into.

	It pins nothing itself. A child inherits the affinity and priority of whoever starts it, and the
	first milliseconds of a fresh process -- the runtime starting, the assembly loading, the first
	method compiling -- ARE the figure here, so a mask applied after Start-Process has returned
	arrives after the thing it was meant to govern. This is run under Run-Announced.ps1, which sets
	the mask on itself before anything starts; this script only checks what it inherited and refuses
	what it does not recognise.

.EXAMPLE
	& ./benchmarks/Run-Announced.ps1 -Label fix-first-validate -SlotMinutes 15 -Script ./benchmarks/FirstCall/Fix/Run-Validate.ps1 -Artifacts ./benchmarks/FirstCall/Fix/bin/Release/net10.0/fixfirst.exe, ./src/DotGram.Finance/bin/Release/net10.0/DotGram.Finance.dll
#>
param(
	# A directory holding the DotGram.Finance.dll under test, built Release.
	[string]   $Library  = 'src/DotGram.Finance/bin/Release/net10.0',
	[string[]] $Types    = @('Heartbeat', 'NewOrderSingle', 'ExecutionReport', 'TradeCaptureReport'),
	[int]      $Launches = 11,
	[string]   $Harness  = 'benchmarks/FirstCall/Fix/bin/Release/net10.0/fixfirst.exe',

	# One process that validates one message of EVERY type, instead of one type a process. It is
	# the other question: not what the first message costs, but what a process pays to have met
	# them all -- and it is a reading where multiplying the per-type row would be a product.
	[switch]   $All
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path benchmarks/Window.ps1)) { 'NOT CHECKED: Window.ps1 is not in this checkout'; exit 9 }

$window = (& pwsh -NoProfile -File benchmarks/Window.ps1) -join ' '

if ($window -notmatch '^(BUSY)') {
	"NOT ANNOUNCED: this is a timing run and the machine says: $window"
	'Announce a window first; a first-call figure taken on an unannounced machine is not a figure.'
	exit 9
}

foreach ($needed in @($Harness, (Join-Path $Library 'DotGram.Finance.dll'))) {
	if (-not (Test-Path $needed)) { "MISSING: $needed"; exit 9 }
}

# Inherited, never set here. Printing it means a run started without the announcer says so in its
# own output rather than looking like every other run.
$mask = [int64](Get-Process -Id $PID).ProcessorAffinity
$rank = (Get-Process -Id $PID).PriorityClass

'affinity 0x{0:X} priority {1}' -f $mask, $rank

if ($mask -ne 0xFFFF) {
	'NOT THE TIMING HALF: this inherited a mask that is not 0xFFFF, the cores the stand times on.'
	'Run it through Run-Announced.ps1, which sets the mask on itself before anything starts.'
	exit 9
}

# The third cell is the second road again, under another name: an A/A whose spread is this run's
# resolution.
$roads = if ($All) {
	[ordered]@{
		'the walk'      = 'validate-all'
		'generated'     = 'validate-all-generated'
		'generated A/A' = 'validate-all-generated'
	}
} else {
	[ordered]@{
		'the walk'      = 'validate'
		'generated'     = 'validate-generated'
		'generated A/A' = 'validate-generated'
	}
}

# The all-types modes take no message type and report one line for the ninety-three together.
if ($All) { $Types = @('all 93 types') }

function Sample([string] $mode, [string] $type) {
	$out = New-TemporaryFile
	$err = New-TemporaryFile

	try {
		# No mask is set on the child on purpose: it inherits this process's, and by the time
		# Start-Process has returned, the part of a first call that matters has already happened.
		$arguments = if ($All) { @($Library, $mode) } else { @($Library, $mode, $type) }

		$run = Start-Process -FilePath $Harness -ArgumentList $arguments -PassThru -NoNewWindow `
			-RedirectStandardOutput $out -RedirectStandardError $err

		$run.WaitForExit()

		if ($run.ExitCode -ne 0) {
			throw "fixfirst $mode $type exited $($run.ExitCode): $(Get-Content $err -Raw)"
		}

		$line = Get-Content $out | Where-Object { $_ -match '^first validate' }
		$line = @($line)[0]

		if (-not $line) { throw "fixfirst $mode $type printed no first-validate phase" }

		# first validate        12.34 ms   jit   57 methods    98765 bytes IL   9.99 ms compiling   ...
		if ($line -notmatch '^first validate(?: all)?\s+([0-9.]+) ms\s+jit\s+(\d+) methods\s+(\d+) bytes IL\s+([0-9.]+) ms compiling') {
			throw "fixfirst $mode $type printed a phase line this script cannot read: $line"
		}

		[pscustomobject]@{
			Wall      = [double]$Matches[1]
			Methods   = [int]$Matches[2]
			Il        = [int]$Matches[3]
			Compiling = [double]$Matches[4]
		}
	}
	finally {
		Remove-Item $out, $err -ErrorAction SilentlyContinue
	}
}

function Median([double[]] $values) {
	$sorted = @($values | Sort-Object)
	$middle = [int][math]::Floor($sorted.Count / 2)

	if ($sorted.Count % 2) { $sorted[$middle] } else { ($sorted[$middle - 1] + $sorted[$middle]) / 2 }
}

$taken = @{}

foreach ($type in $Types) {
	foreach ($road in $roads.Keys) { $taken["$type/$road"] = @() }
}

for ($launch = 1; $launch -le $Launches; $launch++) {
	foreach ($type in $Types) {
		# The order of the three roads rotates by launch, so that none of them is always the one
		# that goes first on a machine that is still settling.
		$names = @($roads.Keys)
		$order = @(0..($names.Count - 1) | ForEach-Object { $names[($_ + $launch) % $names.Count] })

		foreach ($road in $order) {
			$taken["$type/$road"] += ,(Sample $roads[$road] $type)
		}
	}

	Write-Host ("launch {0} of {1}   " -f $launch, $Launches) -NoNewline
	Write-Host "`r" -NoNewline
}

Write-Host ''
'{0,-20} {1,-15} {2,26} {3,26} {4,9} {5,10}' -f 'message', 'road', 'wall ms (min/med/max)', 'compiling ms (min/med/max)', 'methods', 'IL bytes'
'{0,-20} {1,-15} {2,26} {3,26} {4,9} {5,10}' -f '-------', '----', '---------------------', '--------------------------', '-------', '--------'

foreach ($type in $Types) {
	foreach ($road in $roads.Keys) {
		$rows      = $taken["$type/$road"]
		$wall      = @($rows | ForEach-Object { $_.Wall })
		$compiling = @($rows | ForEach-Object { $_.Compiling })

		'{0,-20} {1,-15} {2,26} {3,26} {4,9} {5,10}' -f `
			$type, $road,
			('{0:0.00} / {1:0.00} / {2:0.00}' -f ($wall | Measure-Object -Minimum).Minimum, (Median $wall), ($wall | Measure-Object -Maximum).Maximum),
			('{0:0.00} / {1:0.00} / {2:0.00}' -f ($compiling | Measure-Object -Minimum).Minimum, (Median $compiling), ($compiling | Measure-Object -Maximum).Maximum),
			(Median @($rows | ForEach-Object { [double]$_.Methods })),
			(Median @($rows | ForEach-Object { [double]$_.Il }))
	}
}

''
"$Launches launches a cell, one fresh process each, on the mask this script inherited."
'Wall is the call; compiling is the part of it the runtime spent on the JIT.'
'Read every difference against the two generated cells of the same message: that pair is the same'
'road twice, so what separates them is what this run cannot tell apart.'

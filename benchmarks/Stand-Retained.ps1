<#
.SYNOPSIS
	What each side's parser still holds after a row's input, ONE ROW PER PROCESS.

.DESCRIPTION
	`DotGram.Benchmarks --stand-retained before after --only row` prints the managed heap that stays reachable after a row's reading has run twice
	(and after eight further small parses of the same parser), beside the bytes a call allocates. The pools of a generated parser are thread-static and shared by every row of a process, so what a row
	retains depends on the rows that ran before it in that process (performance-ff, 2026-09-20: four rows "kept less at the same allocation" in a multi-row run, and were identical alone). This script
	runs each row in a process of its own, which is the only way its figure means the row. Times nothing; it needs no window, but the large rows allocate gigabytes a call.

.EXAMPLE
	./benchmarks/Stand-Retained.ps1 -Sides T:\TEMP\dotgram-stand\side\r1-1, T:\TEMP\dotgram-stand\side\r2-1 -Rows sql/worst-columns-100k, el/terms1000 -Exe T:\TEMP\dotgram-stand\stand-bin70\DotGram.Benchmarks.exe
#>
param(
	[Parameter(Mandatory)][string[]]$Sides,
	[Parameter(Mandatory)][string[]]$Rows,
	[Parameter(Mandatory)][string]$Exe,
	[UInt64]$Affinity = 4294901760
)

$ErrorActionPreference = 'Stop'
$me = Get-Process -Id $PID

$me.ProcessorAffinity = [IntPtr]$Affinity
$me.PriorityClass = 'BelowNormal'

$lines = foreach ($row in $Rows) {
	# The row's id is a substring filter: an exact id names one row, and a process holds nothing else.
	& $Exe --stand-retained @Sides --only $row 2>&1 | Where-Object { $_ -match '^\| (el|sql|fix|web|feeds|tsql|fixmsg|config)/' -or $_ -match '^Retained after' -or $_ -match '^    pools after' }
}

"| row | side | after one parse KB | after two KB | two / one | bytes a call | after eight small parses KB | after sixteen KB | after eight more, a collection after each KB |"
"| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |"
$lines

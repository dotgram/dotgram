<#
.SYNOPSIS
	What each side's parser still holds after a row's input, ONE ROW PER PROCESS.

.DESCRIPTION
	`DotGram.Benchmarks --stand-retained before after --only row` prints the managed heap that stays reachable after a row's reading has run twice
	(and after eight further small parses of the same parser), beside the bytes a call allocates. The pools of a generated parser are thread-static and shared by every row of a process, so what a row
	retains depends on the rows that ran before it in that process (performance-ff, 2026-09-20: four rows "kept less at the same allocation" in a multi-row run, and were identical alone). This script
	runs each row in a process of its own, which is the only way its figure means the row. Times nothing, but the large rows allocate gigabytes a call, so it runs as a build does: it waits while a
	timing window is open and holds the builds lock while it runs (WindowLib.ps1, Enter-Aside; D147).

.EXAMPLE
	pwsh benchmarks/Stand-Retained.ps1 -Sides /ramdisk/build/dotgram/stand/side/r1-1, /ramdisk/build/dotgram/stand/side/r2-1 -Rows sql/worst-columns-100k, el/terms1000 -Exe /ramdisk/build/dotgram/stand/bin/DotGram.Benchmarks
#>
param(
	[Parameter(Mandatory)][string[]]$Sides,
	[Parameter(Mandatory)][string[]]$Rows,
	[Parameter(Mandatory)][string]$Exe
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'WindowLib.ps1')

$Sides = @(Split-FileList $Sides)
$Rows  = @(Split-FileList $Rows)

$aside = Enter-Aside

$lines = try {
	foreach ($row in $Rows) {
		# The row's id is a substring filter: an exact id names one row, and a process holds nothing else.
		& $Exe --stand-retained @Sides --only $row 2>&1 | Where-Object { $_ -match '^\| (el|sql|fix|web|feeds|tsql|fixmsg|config)/' -or $_ -match '^Retained after' -or $_ -match '^    pools after' -or $_ -match '^    parses \(' -or $_ -match '^    pools at the first' -or $_ -match '^        parse ' }
	}
}
finally {
	Exit-Aside $aside
}

"| row | side | after one parse KB | after two KB | two / one | bytes a call | after eight small parses KB | after sixteen KB | after eight more, a collection after each KB | twenty of the same parse, lowest KB | highest KB |"
"| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |"
$lines

param([string]$Tag = 'a', [string]$Runs = '1,2,3,4,5')
# Per series over the runs: the exponent's min and max, the classes it took, and whether the class flipped.
$used = ($Runs -split ',' | ForEach-Object { [int]$_ }) | Where-Object { Test-Path "T:\TEMP\dotgram-stand\spread-$Tag-$_.txt" }
$data = @{}
foreach ($i in $used) {
	foreach ($line in Get-Content "T:\TEMP\dotgram-stand\spread-$Tag-$i.txt") {
		if ($line -match '^\| (?<parser>[^|]+) \| (?<shape>[^|]+) \| (?<unit>[^|]+) \| (?<sizes>[^|]+) \| (?<points>[^|]+) \| (?<chars>[^|]+) \| (?<time>[^|]+) \| (?<exp>[^|]+) \|') {
			$id = ($Matches.parser.Trim() + ' | ' + $Matches.shape.Trim())
			$e = 0.0
			if ([double]::TryParse($Matches.exp.Trim(), [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$e)) {
				if (-not $data.ContainsKey($id)) { $data[$id] = @{ Exp = @(); Class = @() } }
				$data[$id].Exp += $e
			}
		}
		elseif ($line -match '^(?<id>.+) = (?<cls>Linear|Superlinear|Quadratic)$') {
			$id = $Matches.id.Trim()
			if (-not $data.ContainsKey($id)) { $data[$id] = @{ Exp = @(); Class = @() } }
			$data[$id].Class += $Matches.cls
		}
	}
}
$rows = foreach ($id in $data.Keys) {
	$d = $data[$id]
	if ($d.Exp.Count -eq 0) { continue }
	$classes = @($d.Class | Select-Object -Unique)
	[pscustomobject]@{ Id = $id; Runs = $d.Exp.Count; Min = [math]::Round(($d.Exp | Measure-Object -Minimum).Minimum, 2); Max = [math]::Round(($d.Exp | Measure-Object -Maximum).Maximum, 2); Classes = ($classes -join '/'); Flips = ($classes.Count -gt 1) }
}
"runs read: $($used.Count); series: $(@($rows).Count); series whose class changed between runs: $(@($rows | Where-Object Flips).Count)"
"widest exponent range (max-min): {0:N2}; median range: {1:N2}" -f (($rows | ForEach-Object { $_.Max - $_.Min } | Measure-Object -Maximum).Maximum), (($rows | ForEach-Object { $_.Max - $_.Min } | Sort-Object)[[int](@($rows).Count / 2)])
''
'Series whose class changed, or whose exponent moved by 0.15 or more:'
$rows | Where-Object { $_.Flips -or ($_.Max - $_.Min) -ge 0.15 } | Sort-Object { $_.Max - $_.Min } -Descending | Format-Table -AutoSize -Wrap | Out-String -Width 220

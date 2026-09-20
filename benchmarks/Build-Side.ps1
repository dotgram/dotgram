<#
.SYNOPSIS
	Builds one side of a paired stand: the five libraries of a commit, with the build properties it is given, into a folder of DLLs.

.DESCRIPTION
	`DotGram.Benchmarks --stand-paired beforeDir afterDir` loads two folders of DLLs into one process and
	reads them round-robin. A folder is what this script makes. Two folders of two commits are a pair
	of commits; two folders of ONE commit built with a property flipped (`-Property DotGramSearchValues=off`)
	are a pair of two branches of the emitted code, on one platform, in one process, with nothing else
	different: that is the pair a platform-dependent branch has to carry.

	The build runs in a worktree of its own for each side (`-Tree`), rebuilt from scratch when a property is
	given, so that the generator runs again with it. It is pinned to logical processors 16-31 by default (a
	build times nothing but must not share the timing window's half of the machine) and it writes
	`build.txt` into the folder: the commit, the properties and a hash of the code the generator emitted (the
	generated files with the worktree's path taken out of them, since `#line` carries it), which the report
	of a pair prints in its header. **A property no generator reads changes nothing**: the property reaches the
	generator only as `build_property.Name` in the analyzer config and only where a
	`<CompilerVisibleProperty>` names it, so a side built with a property nobody consumes emits the same
	code as one built without, and a pair of the two would read as "the branch costs nothing". The hash is
	how the report tells: two sides given different properties and the same hash are printed as not a pair.
	The generation reports (`*.DotGramReport.g.cs`, which carry timings) are left out of the hash. The check
	applies only to two sides of ONE commit given different properties: its silence says nothing about two commits.

.EXAMPLE
	./benchmarks/Build-Side.ps1 -Name on  -Commit HEAD
	./benchmarks/Build-Side.ps1 -Name off -Commit HEAD -Property DotGramSearchValues=off
#>
param(
	[Parameter(Mandatory)][string]$Name,
	[string]$Commit = 'HEAD',
	[string[]]$Property = @(),
	[string]$Root = 'T:\TEMP\dotgram-stand',
	[string]$Tree,
	[string]$Framework = 'net10.0',
	[UInt64]$Affinity = 4294901760
)

$ErrorActionPreference = 'Stop'

$repo = (git rev-parse --show-toplevel).Trim()
$sha  = (git -C $repo rev-parse --short $Commit).Trim()
$me   = Get-Process -Id $PID

$me.ProcessorAffinity = [IntPtr]$Affinity
$env:MSBUILDDISABLENODEREUSE = '1'

if (-not $Tree) { $Tree = Join-Path $Root "wt\$Name-$sha" }

if (-not (Test-Path $Tree)) { git -C $repo worktree add --detach $Tree $sha | Out-Null }

$projects = 'src\DotGram.Finance\DotGram.Finance.csproj', 'src\DotGram.ExpressionLanguage\DotGram.ExpressionLanguage.csproj',
	'src\DotGram.Web\DotGram.Web.csproj', 'examples\DotGram.Examples\DotGram.Examples.csproj', 'src\DotGram.Sql\DotGram.Sql.csproj'
$flags    = @($Property | ForEach-Object { "-p:$_" })
$rebuild  = @(if ($Property.Count -gt 0) { '-t:Rebuild' })
$watch    = [Diagnostics.Stopwatch]::StartNew()

foreach ($project in $projects) {
	$log = dotnet build (Join-Path $Tree $project) -c Release -f $Framework -v:quiet -m:1 -nodeReuse:false -p:UseSharedCompilation=false @rebuild @flags 2>&1

	if ($LASTEXITCODE -ne 0) { throw "$Name`: $project did not build:`n$(($log | Select-Object -Last 15) -join "`n")" }
}

$out = Join-Path $Root "side\$Name"

Remove-Item -Recurse -Force $out -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $out | Out-Null

foreach ($project in $projects) {
	$directory = Split-Path (Join-Path $Tree $project)

	Copy-Item (Join-Path $directory "bin\Release\$Framework\$([IO.Path]::GetFileNameWithoutExtension($project)).dll") $out
}

# The emitted code, hashed without the worktree's path (`#line` directives carry it), file by file in name order.
$sha256 = [Security.Cryptography.SHA256]::Create()
$bytes  = [Text.Encoding]::UTF8.GetBytes(($projects | ForEach-Object {
	$generated = Join-Path (Split-Path (Join-Path $Tree $_)) 'obj\GeneratedFiles'

	if (Test-Path $generated) {
		Get-ChildItem $generated -Recurse -Filter *.g.cs | Where-Object { $_.Name -notlike '*DotGramReport*' } | Sort-Object FullName | ForEach-Object { (Get-Content $_.FullName -Raw).Replace($Tree, '') }
	}
}) -join "`n")
$emitted = if ($bytes.Length -gt 0) { -join ($sha256.ComputeHash($bytes)[0..7] | ForEach-Object { $_.ToString('x2') }) } else { 'none: no generated files were found' }

@("commit $sha", "framework $Framework", $(if ($Property.Count -gt 0) { "properties $($Property -join ' ')" } else { 'no properties' }), "emitted $emitted") | Set-Content (Join-Path $out 'build.txt')

"{0}: {1:N0} s, {2} dlls, {3}" -f $Name, $watch.Elapsed.TotalSeconds, (Get-ChildItem $out -Filter *.dll).Count, (Get-Date -Format HH:mm:ss)

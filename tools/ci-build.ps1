param(
	[Parameter(Mandatory)][string] $Configuration
)

$ErrorActionPreference = 'Stop'
$directory = Join-Path $PWD 'artifacts/build-diagnostics'
New-Item -ItemType Directory -Force $directory | Out-Null
$arguments = @(
	'build', 'DotGram.slnx', '--no-restore', '--configuration', $Configuration,
	'-warnaserror', '-m:1', '-nodeReuse:false', '-p:UseSharedCompilation=false',
	'-p:ReportAnalyzer=true', "-bl:$directory/build.binlog;ProjectImports=None"
)
$parameters = @{
	FilePath = 'dotnet'
	ArgumentList = $arguments
	PassThru = $true
	RedirectStandardOutput = Join-Path $directory 'build.stdout.log'
	RedirectStandardError = Join-Path $directory 'build.stderr.log'
}
if ($IsWindows) { $parameters.WindowStyle = 'Hidden' }
$watch = [Diagnostics.Stopwatch]::StartNew()
$process = Start-Process @parameters
try
{
	do
	{
		$compilers = @(Get-Process -Name dotnet, csc, vbc, VBCSCompiler, MSBuild -ErrorAction SilentlyContinue)
		$workingSet = ($compilers | Measure-Object WorkingSet64 -Sum).Sum
		$private = ($compilers | Measure-Object PrivateMemorySize64 -Sum).Sum
		$available = $null
		if ($IsWindows)
		{
			$available = (Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory * 1KB
		}
		elseif (Test-Path /proc/meminfo)
		{
			$memory = Get-Content /proc/meminfo -Raw
			if ($memory -match 'MemAvailable:\s+(\d+)') { $available = [long]$Matches[1] * 1KB }
		}
		$row = [pscustomobject]@{
			Utc = [DateTime]::UtcNow.ToString('o')
			ElapsedSeconds = [math]::Round($watch.Elapsed.TotalSeconds, 1)
			CompilerProcesses = $compilers.Count
			WorkingSetMB = [math]::Round($workingSet / 1MB)
			PrivateMB = [math]::Round($private / 1MB)
			AvailableMB = if ($null -ne $available) { [math]::Round($available / 1MB) } else { $null }
		}
		$row | Export-Csv (Join-Path $directory 'memory.csv') -Append -NoTypeInformation
		Write-Host ($row | ConvertTo-Json -Compress)
		$finished = $process.WaitForExit(15000)
	} while (-not $finished)
	$process.WaitForExit()
	$code = $process.ExitCode
}
finally
{
	Get-Content (Join-Path $directory 'build.stdout.log')
	Get-Content (Join-Path $directory 'build.stderr.log')
}
exit $code

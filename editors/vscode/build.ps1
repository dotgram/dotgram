# Builds the .Gram extension: the language server published into server/, the client bundled, and the
# whole packaged as a .vsix in artifacts/ at the repository root. Never publishes.
#
#   editors/vscode/build.ps1 [-SelfContained <rid>]
#
# -SelfContained <rid> publishes the server for one platform with its own runtime, and packages a
# platform-specific .vsix (vsce package --target); without it the server is framework-dependent and
# the .vsix runs anywhere .NET 10 does (docs/vscode.md).
param([string] $SelfContained = "")

$ErrorActionPreference = "Stop"

$here = $PSScriptRoot
$root = Resolve-Path (Join-Path $here "../..")

$targets = @{
	"win-x64"     = "win32-x64"
	"win-arm64"   = "win32-arm64"
	"linux-x64"   = "linux-x64"
	"linux-arm64" = "linux-arm64"
	"osx-x64"     = "darwin-x64"
	"osx-arm64"   = "darwin-arm64"
}

function Invoke-Checked([string] $command, [string[]] $arguments) {
	& $command @arguments
	if ($LASTEXITCODE -ne 0) { throw "$command failed with exit code $LASTEXITCODE." }
}

Remove-Item -Recurse -Force (Join-Path $here "server") -ErrorAction SilentlyContinue

$publish = @("publish", (Join-Path $root "src/DotGram.LanguageServer/DotGram.LanguageServer.csproj"),
	"-c", "Release", "-o", (Join-Path $here "server"), "-nodeReuse:false", "-warnaserror")
$target  = @()

if ($SelfContained) {
	if (-not $targets.ContainsKey($SelfContained)) { throw "No VS Code target for runtime $SelfContained." }

	$publish += @("--runtime", $SelfContained, "--self-contained", "true")
	$target   = @("--target", $targets[$SelfContained])
}
else {
	$publish += @("--no-self-contained", "-p:UseAppHost=false")
}

Invoke-Checked "dotnet" $publish

Push-Location $here
try {
	Invoke-Checked "npm" @("ci")
	Invoke-Checked "npm" @("run", "build")
	New-Item -ItemType Directory -Force (Join-Path $root "artifacts") | Out-Null
	Invoke-Checked "npx" (@("vsce", "package") + $target + @("--out", (Join-Path $root "artifacts/")))
}
finally {
	Pop-Location
}

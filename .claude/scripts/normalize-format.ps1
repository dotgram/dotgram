<#
.SYNOPSIS
	Applies the repository's file-format rules: CRLF, final newline, no trailing
	whitespace, and the right BOM per extension.

.DESCRIPTION
	Replaces the ad-hoc loop this used to be done with. That loop recursed with
	-Force and walked into .vs, rewriting Visual Studio's binary caches as text and
	corrupting them. Hence the two hard rules here:

	  * an explicit allowlist of extensions — never "every file";
	  * an explicit denylist of directories, checked before a file is opened.

	Indentation is deliberately not touched: converting spaces to tabs mechanically
	mishandles XML (two-space base indent) and alignment continuations. That stays a
	matter of writing files correctly in the first place.

.PARAMETER Path
	Repository root. Defaults to the parent of this script's own directory.

.PARAMETER WhatIf
	Report what would change without writing.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
	[string] $Path = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
)

$ErrorActionPreference = 'Stop'

$textExtensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.md', '.json', '.gram', '.ps1', '.editorconfig', '.gitattributes', '.gitignore')
$bomExtensions  = @('.cs', '.csproj', '.props', '.targets')
$excludedDirs   = @('.vs', '.git', 'bin', 'obj', 'node_modules', 'artifacts')

$changed = 0

foreach ($file in Get-ChildItem -LiteralPath $Path -Recurse -File -Force)
{
	$relative = $file.FullName.Substring($Path.Length).TrimStart('\')

	if ($excludedDirs | Where-Object { $relative -split '\\' -contains $_ })
		{ continue }

	$extension = if ($file.Name -like '.*') { $file.Name } else { $file.Extension }

	if ($textExtensions -notcontains $extension)
		{ continue }

	$original = [IO.File]::ReadAllText($file.FullName)
	$lines    = $original -split "`r`n|`n|`r" | ForEach-Object { $_.TrimEnd() }

	while ($lines.Count -gt 0 -and $lines[-1] -eq '')
		{ $lines = $lines[0..($lines.Count - 2)] }

	$text     = ($lines -join "`r`n") + "`r`n"
	$encoding = [Text.UTF8Encoding]::new($bomExtensions -contains $extension)
	$expected = $encoding.GetPreamble() + $encoding.GetBytes($text)
	$actual   = [IO.File]::ReadAllBytes($file.FullName)

	if (-not [Linq.Enumerable]::SequenceEqual([byte[]] $expected, $actual))
	{
		if ($PSCmdlet.ShouldProcess($relative, 'normalize'))
			{ [IO.File]::WriteAllText($file.FullName, $text, $encoding) }

		Write-Output $relative
		$changed++
	}
}

Write-Output "$changed file(s) normalized."

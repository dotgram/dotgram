using System;

namespace DotGram.Cli;

/// <summary>How the file system compares names.</summary>
static class FileSystem
{
	/// <summary>
	/// Names as the file system tells them apart: without case on Windows and macOS, whose file systems
	/// ignore it by default, with case elsewhere. Two spellings this calls equal are one file.
	/// </summary>
	public static readonly StringComparer Names =
		OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}

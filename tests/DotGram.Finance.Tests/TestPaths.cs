using System.IO;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Tests;

/// <summary>
/// Where the checked-in files the tests read are. Found from this source file and not from the
/// test binary, which is built into a folder of its own outside the project's.
/// </summary>
static class TestPaths
{
	/// <summary>The repository's <c>tests</c> folder.</summary>
	public static string Tests { get; } = Path.GetFullPath(Path.Combine(SourceFolder(), ".."));

	static string SourceFolder([CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}
}

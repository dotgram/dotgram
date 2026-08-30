using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every identifier the compiler can report is in the reference, and nothing else is.
/// </summary>
/// <remarks>
/// A message carries an identifier so that it can be looked up, and a reference that has
/// fallen behind the code is worse than none: it answers, and answers wrong. So the two are
/// compared here rather than by anyone remembering. A number that is retired stays in the
/// reference and leaves the code, which is the one direction the comparison allows.
/// </remarks>
public sealed class DiagnosticsReferenceTests
{
	[Fact]
	public void Every_identifier_the_compiler_reports_is_documented()
	{
		var missing = Reported().Except(Documented()).OrderBy(one => one, StringComparer.Ordinal).ToArray();

		Assert.True(
			missing.Length == 0,
			"Not in docs/diagnostics.md: " + string.Join(", ", missing));
	}

	/// <summary>And a documented one is either reported or listed as retired.</summary>
	[Fact]
	public void And_a_documented_identifier_is_reported_or_retired()
	{
		var reference = File.ReadAllText(Reference);
		var retired   = reference[reference.IndexOf("## Retired numbers", StringComparison.Ordinal)..];

		var stale = Documented()
			.Except(Reported())
			.Where(one => !retired.Contains(one, StringComparison.Ordinal))
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToArray();

		Assert.True(
			stale.Length == 0,
			"Documented, reported by nothing, and not listed as retired: " + string.Join(", ", stale));
	}

	static HashSet<string> Reported()
	{
		var found = new HashSet<string>(StringComparer.Ordinal);

		foreach (var file in Directory.GetFiles(Compiler, "*.cs", SearchOption.AllDirectories))
		{
			if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
				file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
				continue;

			foreach (Match match in Identifier.Matches(File.ReadAllText(file)))
				found.Add(match.Value);
		}

		return found;
	}

	static HashSet<string> Documented() =>
		[.. Identifier.Matches(File.ReadAllText(Reference)).Select(match => match.Value)];

	static readonly Regex Identifier = new("GRAM[0-9]{4}", RegexOptions.Compiled);

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string Compiler  => Path.Combine(Root, "src", "DotGram");
	static string Reference => Path.Combine(Root, "docs", "diagnostics.md");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}

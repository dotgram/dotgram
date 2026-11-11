using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every identifier the compiler or the Visual Studio extension can report is in the
/// reference, and nothing else is.
/// </summary>
/// <remarks>
/// A message carries an identifier so that it can be looked up, and a reference that has
/// fallen behind the code is worse than none: it answers, and answers wrong. So the two are
/// compared here rather than by anyone remembering. A number that is retired stays in the
/// reference and leaves the code, which is the one direction the comparison allows. The
/// extension reports in the editor what the generator does not, and a number means one thing
/// whichever of the two reports it.
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

	/// <summary>No number is reported by both the generator and the extension.</summary>
	[Fact]
	public void The_generator_and_the_extension_report_different_numbers()
	{
		var shared = Reported(Compiler)
			.Intersect(Reported(Extension))
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToArray();

		Assert.True(
			shared.Length == 0,
			"Reported by both src/DotGram and src/DotGram.VisualStudio: " + string.Join(", ", shared));
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

	/// <summary>
	/// The catalog the generator and the editor report from is the reference's table: every identifier
	/// in use, its title the "What it says" column, its severity the Severity column, and each row an
	/// anchor a help link can point at.
	/// </summary>
	[Fact]
	public void The_catalog_is_the_reference()
	{
		var reference = File.ReadAllText(Reference);
		var active    = reference[..reference.IndexOf("## Retired numbers", StringComparison.Ordinal)];
		var rows      = Row.Matches(active)
			.ToDictionary(
				static match => match.Groups["id"].Value,
				static match => (Severity: match.Groups["severity"].Value, Title: match.Groups["title"].Value.Replace("`", "", StringComparison.Ordinal).TrimEnd('.')),
				StringComparer.Ordinal);

		Assert.Equal(
			DiagnosticCatalog.Entries.Keys.Order(StringComparer.Ordinal),
			rows.Keys.Order(StringComparer.Ordinal));

		foreach (var (id, entry) in DiagnosticCatalog.Entries)
		{
			Assert.Equal((entry.Severity.ToString(), entry.Title), rows[id]);
			Assert.Contains($"<a id=\"{id.ToLowerInvariant()}\"></a>", active, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// The severity a call site passes is the catalog's: the catalog decides it, and a call site that
	/// says otherwise would mislead whoever reads it.
	/// </summary>
	[Fact]
	public void Every_call_site_passes_the_catalog_s_severity()
	{
		var constants = new Dictionary<string, string>(StringComparer.Ordinal);
		var sources   = Directory.GetFiles(Path.Combine(Compiler, "Grammar"), "*.cs", SearchOption.AllDirectories)
			.Select(File.ReadAllText)
			.ToArray();

		foreach (var source in sources)
			foreach (Match match in Constant.Matches(source))
				constants[match.Groups["name"].Value] = match.Groups["id"].Value;

		var wrong = new List<string>();

		foreach (var source in sources)
		{
			foreach (Match match in Created.Matches(source))
				Check(match.Groups["id"].Value, match.Groups["severity"].Value);

			foreach (Match match in Helper.Matches(source))
				Check(match.Groups["id"].Value, match.Groups["helper"].Value == "Report" ? "Error" : "Warning");
		}

		Assert.True(wrong.Count == 0, "Not the catalog's severity: " + string.Join(", ", wrong));

		void Check(string written, string severity)
		{
			var id = written.StartsWith('"') ? written.Trim('"') : constants.GetValueOrDefault(written.Split('.')[^1]);

			if (id is not null && DiagnosticCatalog.Entries.TryGetValue(id, out var entry) && entry.Severity.ToString() != severity)
				wrong.Add($"{id} passed as {severity}, listed as {entry.Severity}");
		}
	}

	/// <summary>A row of the reference: its identifier, severity and what it says.</summary>
	static readonly Regex Row = new(@"^\| <a id=""gram[0-9]{4}""></a>`(?<id>GRAM[0-9]{4})` \| (?<severity>\w+) \| (?<title>.*?) \| ", RegexOptions.Compiled | RegexOptions.Multiline);

	/// <summary>An identifier's constant.</summary>
	static readonly Regex Constant = new(@"const string (?<name>\w+)\s*=\s*""(?<id>GRAM[0-9]{4})""", RegexOptions.Compiled);

	/// <summary>A diagnostic made with its severity: the identifier first, the severity last.</summary>
	static readonly Regex Created = new(@"new GramDiagnostic\(\s*(?<id>""GRAM[0-9]{4}""|[\w.]+)\s*,[^;]*?GramSeverity\.(?<severity>\w+)\)", RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>A diagnostic made through a helper whose name is its severity.</summary>
	static readonly Regex Helper = new(@"\b(?<helper>Report|Warn|Remark)\(\s*(?<id>""GRAM[0-9]{4}""|[\w.]+)\s*,", RegexOptions.Compiled);

	static HashSet<string> Reported()
	{
		return [.. Reported(Compiler), .. Reported(Extension)];
	}

	static HashSet<string> Reported(string directory)
	{
		var found = new HashSet<string>(StringComparer.Ordinal);

		foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
		{
			if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
				file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
				continue;

			// The catalog lists every identifier in use, which is what is compared against it above;
			// here what counts is what the code itself reports.
			if (Path.GetFileName(file) == "DiagnosticCatalog.cs")
				continue;

			foreach (Match match in Identifier.Matches(File.ReadAllText(file)))
				found.Add(match.Value);
		}

		return found;
	}

	static HashSet<string> Documented()
	{
		return [.. Identifier.Matches(File.ReadAllText(Reference)).Select(match => match.Value)];
	}

	static readonly Regex Identifier = new("GRAM[0-9]{4}", RegexOptions.Compiled);

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string Compiler  => Path.Combine(Root, "src", "DotGram");
	static string Extension => Path.Combine(Root, "src", "DotGram.VisualStudio");
	static string Reference => Path.Combine(Root, "docs", "diagnostics.md");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}

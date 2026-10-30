using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every diagnostic the generator can report is shown by the editor with the generator's own
/// text and severity, or is listed with the reason the editor cannot reach it.
/// </summary>
/// <remarks>
/// The editor does not copy the generator's messages: it compiles the grammar with the
/// generator's own front end and shows what comes out. What it cannot show is what needs more
/// than the grammar's text and <c>Lexical</c> — the host class, a C# symbol resolver, an attribute
/// option. EditorDiagnostics.txt holds a grammar for every number the editor does reach, and
/// the reason for every one it does not; a number the generator starts to report fails here until
/// it is put in one list or the other.
/// </remarks>
public sealed class EditorDiagnosticsTests
{
	public static TheoryData<string> Samples()
	{
		var data = new TheoryData<string>();

		foreach (var sample in Read().Samples)
			data.Add(sample.Id);

		return data;
	}

	[Theory]
	[MemberData(nameof(Samples))]
	public void The_editor_shows_what_the_generator_reports(string id)
	{
		var sample   = Read().Samples.Single(sample => sample.Id == id);
		var compiled = GramCompiler.Compile(sample.Text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = sample.Lexical,
		});
		var analysed = GramLanguageService.Analyze(sample.Text, new GramAnalysisOptions { Lexical = sample.Lexical });

		var reported = compiled.Diagnostics.Where(one => one.Id == id).Select(static one => (one.Message, one.Severity)).ToArray();
		var shown    = analysed.Diagnostics.Where(one => one.Id == id).Select(static one => (one.Message, one.Severity)).ToArray();

		Assert.True(reported.Length > 0, $"The sample for {id} no longer makes the generator report it:\n{sample.Text}");
		Assert.Equal(reported, shown);
	}

	[Fact]
	public void Every_number_the_generator_names_is_a_sample_or_an_exclusion()
	{
		var (samples, excluded) = Read();
		var listed = samples.Select(static sample => sample.Id).Concat(excluded.Keys).ToArray();
		var named  = Named();

		Assert.Equal(listed.Length, listed.Distinct(StringComparer.Ordinal).Count());
		Assert.True(
			named.SetEquals(listed),
			"Named in src/DotGram and in neither list: " + string.Join(", ", named.Except(listed).Order(StringComparer.Ordinal)) +
			"; listed and named nowhere: " + string.Join(", ", listed.Except(named).Order(StringComparer.Ordinal)));
		Assert.All(excluded, static pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value), $"{pair.Key} is excluded without a reason"));
	}

	static (IReadOnlyList<Sample> Samples, IReadOnlyDictionary<string, string> Excluded) Read()
	{
		var samples  = new List<Sample>();
		var excluded = new Dictionary<string, string>(StringComparer.Ordinal);
		var lines    = File.ReadAllText(Path.Combine(Root, "tests", "DotGram.Tests", "EditorDiagnostics.txt"))
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Split('\n');

		(string Id, bool Lexical)? current = null;
		var body = new List<string>();

		void Close()
		{
			if (current is { } open)
				samples.Add(new Sample(open.Id, open.Lexical, string.Join("\n", body).TrimEnd('\n')));

			current = null;
			body.Clear();
		}

		foreach (var line in lines)
		{
			if (Header.Match(line) is { Success: true } header)
			{
				Close();
				current = (header.Groups[1].Value, header.Groups[2].Success);
			}
			else if (Exclusion.Match(line) is { Success: true } exclusion)
			{
				Close();
				excluded.Add(exclusion.Groups[1].Value, exclusion.Groups[2].Value);
			}
			else if (current is not null)
				body.Add(line);
		}

		Close();

		return (samples, excluded);
	}

	static HashSet<string> Named()
	{
		var found = new HashSet<string>(StringComparer.Ordinal);

		foreach (var file in Directory.EnumerateFiles(Path.Combine(Root, "src", "DotGram"), "*.cs", SearchOption.AllDirectories))
		{
			var relative = Path.GetRelativePath(Root, file).Replace('\\', '/');

			if (relative.Split('/').Any(static part => part is "bin" or "obj"))
				continue;

			foreach (Match match in Identifier.Matches(File.ReadAllText(file)))
				found.Add(match.Value);
		}

		return found;
	}

	sealed record Sample(string Id, bool Lexical, string Text);

	static readonly Regex Header     = new(@"^=== (GRAM[0-9]{4})( lexical)?\s*$", RegexOptions.Compiled);
	static readonly Regex Exclusion  = new(@"^--- (GRAM[0-9]{4}) (.*)$", RegexOptions.Compiled);
	static readonly Regex Identifier = new("GRAM[0-9]{4}", RegexOptions.Compiled);

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}

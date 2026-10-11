using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the rows mean, asked of the reference semantics, and the documentation held to the rows.
/// </summary>
/// <remarks>
/// These need no generated parser: the grammar's own stages and <see cref="ReferenceInterpreter"/>
/// answer them. They are what the emitted parsers are later held to, so a row whose expectation
/// is wrong fails here first, where the argument is short.
/// </remarks>
public sealed partial class BackReferenceTests
{
	[Theory]
	[MemberData(nameof(RowNames))]
	public void Every_row_passes_every_check(string name)
	{
		var row = Named(name);

		Assert.Empty(Errors(row.Published()));
	}

	[Theory]
	[MemberData(nameof(RowNames))]
	public void The_semantics_decide_every_row_as_written(string name)
	{
		var row   = Named(name);
		var graph = Graph(row.Grammar);
		var rule  = graph.Rules.First(one => one.Name == row.Rule);

		foreach (var input in row.Accepted)
			Assert.True(ReferenceInterpreter.Parses(graph, rule, input), $"{row.Name} refuses \"{Escaped(input)}\"");

		foreach (var input in row.Refused)
			Assert.False(ReferenceInterpreter.Parses(graph, rule, input), $"{row.Name} reads \"{Escaped(input)}\"");
	}

	[Theory]
	[MemberData(nameof(FindRowNames))]
	public void The_semantics_find_every_occurrence_as_written(string name)
	{
		var row   = Named(name);
		var graph = Graph(row.Grammar);
		var rule  = graph.Rules.First(one => one.Name == row.Rule);

		Assert.Equal(row.Found, Occurrences(graph, rule, row.Text!));
	}

	[Fact]
	public void A_binding_records_the_text_it_matched_on_the_path_taken()
	{
		// The give-back the section describes: the binding is the shorter text on the path that
		// succeeds, and a reference reads that, not the longest text the binding ever held.
		var graph = Graph("R = $q: 'a'+ & 'a' & $q");
		var rule  = graph.Rules.Single(one => one.Name == "R");

		Assert.Equal(3, ReferenceInterpreter.Reads(graph, rule, "aaa", 0));
		Assert.Equal(5, ReferenceInterpreter.Reads(graph, rule, "aaaaa", 0));
		Assert.Equal(3, ReferenceInterpreter.Reads(graph, rule, "aaaa", 0));
	}

	[Fact]
	public void A_called_rule_binds_for_itself_alone()
	{
		// The inner element binds `b` and returns; the outer reference still reads `a`.
		var graph = Graph(Named("XmlElement").Grammar);
		var rule  = graph.Rules.Single(one => one.Name == "Element");

		Assert.Equal(14, ReferenceInterpreter.Reads(graph, rule, "<a><b></b></a>", 0));
		Assert.Equal(-1, ReferenceInterpreter.Reads(graph, rule, "<a><b></a></b>", 0));
	}

	// ── The documentation ───────────────────────────────────────────────────────

	/// <summary>
	/// Every grammar §3.5.1 shows is one of the rows, character for character, so the section
	/// shows nothing that is not run.
	/// </summary>
	[Fact]
	public void Every_example_of_the_section_is_a_row()
	{
		var shown = Examples();

		Assert.True(shown.Length >= Documented.Length, $"Only {shown.Length} examples were found in §3.5.1.");

		foreach (var example in shown)
			Assert.True(
				Documented.Any(row => Same(row, example)),
				$"§3.5.1 shows a grammar that no row runs:\n{example}");

		foreach (var row in Documented)
			Assert.True(
				shown.Any(example => Same(row, example)),
				$"A row says it is an example of §3.5.1 and the section does not show it:\n{row}");
	}

	[Fact]
	public void Every_example_of_the_section_is_a_complete_grammar()
	{
		foreach (var example in Examples())
		{
			var first = Regex.Match(example, @"^\w+").Value;

			Assert.Empty(Errors(example + $"\nparse {first}"));
		}
	}

	/// <summary>
	/// The blocks of §3.5.1 that are grammars: those whose first line declares a rule. The rest
	/// list the forms (<c>$q: X   bind: …</c>) and are not meant to be compiled.
	/// </summary>
	static string[] Examples()
	{
		var text  = File.ReadAllText(Path.Combine(Root, "docs", "syntax.md")).Replace("\r\n", "\n");
		var from  = text.IndexOf("#### 3.5.1", StringComparison.Ordinal);
		var to    = text.IndexOf("\n### 3.6", from, StringComparison.Ordinal);
		var block = new Regex(@"```dotgram\n(.*?)```", RegexOptions.Singleline);

		return
		[
			.. block.Matches(text[from..to])
				.Select(static one => one.Groups[1].Value)
				.Where(static one => Regex.IsMatch(one, @"^\w+\s*="))
		];
	}

	/// <summary>The same text, up to the line endings and the indentation a raw string strips.</summary>
	static bool Same(string row, string example)
	{
		return row.Replace("\r\n", "\n").Trim() == example.Replace("\r\n", "\n").Trim();
	}

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>The grammar as the generator reads it, the standard library included.</summary>
	static RecognitionGraph Graph(string grammar)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
		var graph  = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(parsed.File, own: grammar.Length),
			scanner: RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return graph;
	}

	/// <summary>
	/// Where a <c>find</c> would find each occurrence, by the semantics: the first reading from each
	/// position, the search going on after it.
	/// </summary>
	static int[] Occurrences(RecognitionGraph graph, RuleSymbol rule, string text)
	{
		var found = new List<int>();
		var at    = 0;

		while (at < text.Length)
		{
			var end = ReferenceInterpreter.Reads(graph, rule, text, at);

			if (end > at)
			{
				found.Add(at);
				at = end;
			}
			else
			{
				at++;
			}
		}

		return [.. found];
	}

	static string Escaped(string input)
	{
		return input.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
	}
}

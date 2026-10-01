using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Ignore-case literals over characters whose cases are not a pair: every rendering of one
/// random grammar against the reference semantics, on an alphabet made of them.
/// </summary>
public sealed partial class CaseFoldAttackTests
{
	/// <summary>
	/// One seed of the fuzz over unpaired cases; DotGram.Tests.Slow runs the others.
	/// </summary>
	[Fact]
	public void Every_rendering_agrees_with_the_semantics_on_unpaired_cases()
	{
		Fuzz(1, Unpaired);
	}

	/// <summary>The same over ASCII pairs alone, one seed: what is wrong here is not about the fold.</summary>
	[Fact]
	public void Every_rendering_agrees_with_the_semantics_on_ascii_pairs()
	{
		Fuzz(1, "aAbBkK1");
	}

	/// <summary>
	/// The titlecase letter is a case of each of its pair and they are not cases of it: the
	/// relation is the literal's. Asked of every rendering, single and inside a keyword.
	/// </summary>
	[Theory]
	[InlineData("ǅ",  "Ǆ", true)]
	[InlineData("ǅ",  "ǆ", true)]
	[InlineData("Ǆ",  "ǅ", false)]
	[InlineData("ǆ",  "ǅ", false)]
	[InlineData("Ǆ",  "ǆ", true)]
	[InlineData("aǅ", "AǄ", true)]
	[InlineData("aǄ", "Aǅ", false)]
	[InlineData("ǅa", "ǆA", true)]
	[InlineData("Ǆa", "ǅA", false)]
	[InlineData("ßs", "ẞS", false)]
	[InlineData("ẞs", "ßS", true)]
	[InlineData("ςσ", "ΣΣ", true)]
	[InlineData("ΣΣ", "ςσ", false)]
	[InlineData("sς", "ſΣ", false)]
	[InlineData("k\u03BC", "K\u039C", true)]
	[InlineData("k\u03BC", "K\u00B5", false)]
	public void A_literal_reads_what_its_own_characters_fold_to_in_every_rendering(string literal, string input, bool expected)
	{
		var grammar = $"trivia = {{ ' '* }}\nStart = \"{literal}\"i | '(' & \"{literal}\"i & ')' | \"{literal}\"i & \"{literal}\"i\nparse Start\n";
		var failures = new List<string>();

		foreach (var (name, options) in Modes())
		{
			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text);

			foreach (var text in new[] { input, "(" + input + ")", input + input, " " + input + " " })
			{
				var answer = EmittedCode.Match(assembly, "Grammar", "TryParseStart", text).IsSuccess;

				if (answer != expected)
					failures.Add($"{name}: \"{literal}\"i against \"{text}\" read {answer}, wanted {expected}");

				if (options.BufferedInput && Streamed(assembly, text) != expected)
					failures.Add($"{name}, TextReader: \"{literal}\"i against \"{text}\" read {!expected}, wanted {expected}");
			}
		}

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	/// <summary>
	/// What an ignore-case literal reads is decided where the parser is generated, so it must
	/// not hang on which casing tables the generating process happened to find.
	/// </summary>
	/// <remarks>
	/// <c>char.ToUpperInvariant</c> reads the system ICU where there is one and the runtime's
	/// own tables in invariant globalization mode (and NLS under .NET Framework, which is what
	/// an editor may host the generator on). On one machine, .NET 10 over ICU 78 pairs U+A7CE
	/// with U+A7CF (Unicode 17) and the same .NET 10 with
	/// <c>DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1</c> does not; .NET 8 and 9 in invariant mode
	/// do not pair U+0264 with U+A7CB (Unicode 16) and .NET 10 does. So one grammar generates
	/// two parsers on two build machines. The expected answers are one fixed table's (Unicode
	/// 16, what .NET 10's own tables say); any fixed table would do, as long as it is the
	/// generator's own.
	/// </remarks>
	[Theory]
	[InlineData('\uA7CE', null)]
	[InlineData('\uA7CF', null)]
	[InlineData('\uA7D2', null)]
	[InlineData('\uA7D4', null)]
	[InlineData('\uA7CB', '\u0264')]
	[InlineData('\u0264', '\uA7CB')]
	[InlineData('\u019B', '\uA7DC')]
	public void What_a_literal_reads_does_not_depend_on_the_host_casing_tables(char letter, char? partner)
	{
		var expected = partner is { } other ? new[] { letter, other }.OrderBy(static one => one).ToArray() : [letter];

		Assert.Equal(expected, CaseFold.Of(letter));
	}

	/// <summary>
	/// Keywords beyond ASCII under recovery and <c>find</c>, over a string and over a reader
	/// that hands one character a call: every rendering reads the same rows, refuses at the same
	/// place with the same message, and finds the same rows.
	/// </summary>
	[Theory]
	[InlineData("\u03A3\u0395\u039B \u01C4AB;")]
	[InlineData("\u03C2\u03B5\u03BB;")]
	[InlineData("\u03C3\u03B5\u03BB \u01C5ab;\u03C2\u03B5\u03BB;sel;")]
	[InlineData("\u01C6ab;\u01C5AB;\u01C4ab;")]
	[InlineData("\u00DFS;\u1E9Es;")]
	[InlineData("\u017Fel;SEL;")]
	[InlineData("K\u00B5;k\u039C;K\u03BC;")]
	[InlineData("\u03C3\u03B5")]
	[InlineData("sel \u03C3\u03B5\u03BB ; \u01C5a")]
	[InlineData("")]
	public void Recovery_and_find_agree_in_every_rendering(string input)
	{
		const string grammar = """
			trivia = { ' '* }
			Kw = "\u03C3\u03B5\u03BB"i | "\u01C5ab"i | "\u1E9Es"i | "sel"i | "k\u03BC"i
			Row = Kw & Kw? & ';'
			Start = Row* recover ';' & eof
			parse Start
			parse Row as Rows
			find Row
			""";

		var graph    = Normalized(grammar);
		var row      = graph.Rules.First(static rule => rule.Name == "Row");
		var answers  = new List<(string Name, string Answer)>();

		foreach (var (name, options) in Modes())
		{
			if (options.Lexical)
				continue;

			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text);
			var whole    = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
			var rows     = EmittedCode.Match(assembly, "Grammar", "TryRows", input);
			var found    = string.Join(",", EmittedCode.FoundAt(assembly, "Grammar", "FindRow", input));
			var answer   = $"start {whole.IsSuccess} at {whole.Position}: {whole.Error}; rows {rows.IsSuccess} at {rows.Position}: {rows.Error}; found {found}";

			Assert.Equal(ReferenceInterpreter.Parses(graph, row, input), rows.IsSuccess);

			if (options.BufferedInput)
			{
				var streamed = string.Join(",", FoundAtReader(assembly, input));

				answers.Add((name + ", TextReader find", streamed == found ? "same" : $"found {streamed}, over the string {found}"));
			}

			answers.Add((name, answer));
		}

		var expected = answers[0].Answer;

		Assert.All(answers, one => Assert.True(
			one.Answer == expected || one.Answer == "same", $"{one.Name}: {one.Answer}\nengine: {expected}"));
	}
}

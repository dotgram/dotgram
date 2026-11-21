using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Braces inside a recursion, in a file written in several parts: the parts that seal are read with
/// the pointers that let a seal step over what the seals inside it spent, and the reading is linear in
/// its depth, as it is in one file.
/// </summary>
/// <remarks>
/// Whether the support carries the pointers is decided from what the readers wrote. A split file
/// moves its readers into parts of their own, and a decision read off the first part alone left the
/// pointers out where every seal was in another part: the parse was right, and quadratic again.
/// </remarks>
public sealed class SealedSplitTests
{
	const string Braced = """
		trivia = none
		Doc  = Nest & eof
		Nest = '(' & { Item* } & ')' | '(' & '!' & ')'
		Item = Nest | Word
		Word = ['a'..'z']+ | ' '
		parse Doc
		""";

	[Theory]
	[InlineData("(a(a)a)")]
	[InlineData("(a(a(!)a)a)")]
	[InlineData("(a(a)a")]
	[InlineData("(!)")]
	[InlineData("((a a)(a)!)")]
	public void A_split_file_answers_as_one_file_does(string input)
	{
		Assert.Equal(
			EmittedCode.Match(Compiled(split: false, counts: false).Assembly, "Grammar", "TryParseDoc", input),
			EmittedCode.Match(Compiled(split: true, counts: false).Assembly, "Grammar", "TryParseDoc", input));
	}

	/// <summary>The ways the seals look at, at four depths a doubling apart: each doubling adds about twice what the one before did.</summary>
	[Fact]
	public void A_split_file_seals_nested_braces_in_time_linear_in_their_depth()
	{
		var parser = Compiled(split: true, counts: true);
		var steps  = parser.GetNestedType("Ways_DotGram", BindingFlags.NonPublic)!.GetField("CountSealSteps", BindingFlags.NonPublic | BindingFlags.Static);

		Assert.True(steps is not null, "the split parser carries no skip pointers: its support was written without them.");

		var counts = new List<long>();

		foreach (var depth in new[] { 1000, 2000, 4000, 8000 })
		{
			steps!.SetValue(null, 0L);

			var text = string.Concat(Enumerable.Repeat("(a", depth)) + string.Concat(Enumerable.Repeat("a)", depth));

			Assert.True(EmittedCode.Match(parser.Assembly, "Grammar", "TryParseDoc", text).IsSuccess);

			counts.Add((long)steps.GetValue(null)!);
		}

		Assert.True(
			counts[0] > 0 && counts[3] - counts[2] <= 2.2 * (counts[2] - counts[1]) && counts[2] - counts[1] <= 2.2 * (counts[1] - counts[0]),
			$"{string.Join(", ", counts)} ways looked at by the seals at 1000, 2000, 4000 and 8000 levels.");
	}

	static readonly Dictionary<(bool, bool), Type> _compiled = [];

	static Type Compiled(bool split, bool counts)
	{
		lock (_compiled)
		{
			if (_compiled.TryGetValue((split, counts), out var known))
				return known;

			var result = GramCompiler.Compile(Braced, new GramCompilerOptions
			{
				ClassName      = "Grammar",
				CSharpScanner  = RoslynCSharpScanner.Instance,
				Direct         = true,
				SourceFileSize = split ? 2000 : 0,
			});

			EmittedCode.Quiet(result.Diagnostics);

			if (split)
			{
				Assert.True(result.Sources.Count > 1, "the grammar was written in one file.");
				Assert.DoesNotContain("ways.Seal(", result.Sources[0].Text, StringComparison.Ordinal);
				Assert.Contains(result.Sources.Skip(1), static part => part.Text.Contains("ways.Seal(", StringComparison.Ordinal));
			}

			var parser = EmittedCode.Compile(
				result.Sources[0].Text,
				sourceParts: result.Sources.Skip(1).Select(static one => one.Text),
				symbols: counts ? ["DOTGRAM_COUNTS"] : null).GetType("Grammar")!;

			_compiled[(split, counts)] = parser;

			return parser;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// <see cref="HandExpression"/> held to <see cref="ExpressionParser"/>: the same accepted and
/// refused text, the same tree, a refusal at the same place (docs/design/architecture-decisions.md,
/// D1 and D8).
/// </summary>
/// <remarks>
/// Every text <see cref="ExpressionParserTests"/> reads goes through <see cref="Both"/> as well;
/// this adds the shared corpus the benchmark agrees on before it times anything, and the corpus
/// taken apart one character at a time, which is where two readers stop in different places.
/// </remarks>
public sealed class ExpressionHandTests
{
	public static TheoryData<string> Shapes => [.. ExpressionCorpus.Shapes];

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Every_shape_of_the_language_is_answered_alike(string text)
	{
		Both.Agree(text);
	}

	[Theory]
	[MemberData(nameof(Shapes))]
	public void And_alike_where_a_name_is_spelled_in_ASCII(string text)
	{
		Both.Agree(text, typeof(Both).Assembly, ascii: true);
	}

	[Theory]
	[InlineData("(int caf\u00e9) => caf\u00e9")]
	[InlineData("(int x\u00e9) => 1")]
	[InlineData("(int x) => $\"{\u00e9}\"")]
	[InlineData("using System.Linq; (int[] a) => a.Select(\u00e9 => \u00e9).Sum()")]
	public void A_word_outside_ASCII_is_refused_alike_by_the_narrower_reading(string text)
	{
		Both.Agree(text, typeof(Both).Assembly, ascii: true);
	}

	[Fact]
	public void And_alike_on_every_text_one_character_short_of_a_shape()
	{
		var told = new List<string>();

		foreach (var shape in ExpressionCorpus.Shapes)
			foreach (var text in Mutations(shape))
				try
				{
					Both.Agree(text);
				}
				catch (Exception disagreed) when (disagreed is not OutOfMemoryException)
				{
					told.Add(disagreed.Message);
				}

		Assert.True(told.Count == 0, $"{told.Count} texts told the two apart; the first:\n\n" + string.Join("\n\n", told.Take(8)));
	}

	/// <summary>A shape cut short at every character, and with every character taken out once.</summary>
	static IEnumerable<string> Mutations(string shape)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal) { shape };

		for (var length = 0; length < shape.Length; length++)
			if (seen.Add(shape.Substring(0, length)))
				yield return shape.Substring(0, length);

		for (var at = 0; at < shape.Length; at++)
			if (seen.Add(shape.Remove(at, 1)))
				yield return shape.Remove(at, 1);
	}

	/// <summary>
	/// Texts that read and that no tree can be built for: every construction here throws where it
	/// is made, so a reader that builds anything where it only reads throws on them.
	/// </summary>
	/// <remarks>
	/// It catches only a construction that throws. What a guard is handed is built all the same,
	/// as the grammar builds it for the guard: a declaration's type, the operand a member is
	/// asked for when it is assigned to, what `var` and `foreach (var …)` take their type from,
	/// whether a `new` names an array, and the parameters of a lambda that says no types.
	/// </remarks>
	[Theory]
	[InlineData("(int x) => x + \"a\" * true")]
	[InlineData("(int x) => x.NoSuchMember")]
	[InlineData("(int x) => x.NoSuchMethod(1, 2)")]
	[InlineData("(string s) => s.Substring(true)")]
	[InlineData("(int x) => (string)x")]
	[InlineData("(int x) => -\"a\"")]
	[InlineData("(int x) => x ? 1 : 2")]
	[InlineData("(int x) => x ?? 1")]
	[InlineData("(int x) => { }")]
	[InlineData("(int x) => { break; }")]
	[InlineData("(int x) => { continue; }")]
	[InlineData("(int x) => 99999999999999999999")]
	[InlineData("(int x) => 1e400m")]
	[InlineData("(int x) => \"\\U0011FFFF\"")]
	[InlineData("(int x) => $\"{x +}\"")]
	[InlineData("(int x) => $\"{nothing}\"")]
	[InlineData("(int x) => \"\"\"\n  a\n   \"\"\"")]
	[InlineData("(int x) => x[0]")]
	[InlineData("(int x) => new int(1, 2, 3)")]
	[InlineData("(int x) => new int[] { \"a\" }")]
	[InlineData("(int x) => new System.Text.StringBuilder() { NoSuchMember = 1 }")]
	[InlineData("(int x) => { var f = n => n; return x; }")]
	[InlineData("(int x) => { switch (x) { case \"a\": break; } x }")]
	[InlineData("(int x) => { while (x) { } x }")]
	[InlineData("(int x) => { if (x) x = 1; x }")]
	[InlineData("(int x) => { return \"a\"; return 1; }")]
	[InlineData("(int x) => { foreach (int n in x) { } x }")]
	[InlineData("(int x) => typeof(int).NoSuchMember")]
	[InlineData("(int x) => checked(x * \"a\")")]
	public void Reading_without_building_builds_nothing(string text)
	{
		Assert.ThrowsAny<Exception>(
			() => ExpressionParser.TryParseLambda(text, new ExpressionParser.State(typeof(Both).Assembly) { Text = text }));

		Assert.True(HandExpression.Recognizes(text, new ExpressionParser.State(typeof(Both).Assembly) { Text = text }));
	}
}

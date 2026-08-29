using System;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The lowered grammar, printed as a tree.
/// </summary>
/// <remarks>
/// A node prints itself as the notation it came from, which reads well and is ambiguous
/// about the one thing a lowering question usually turns on: <c>c: Call => (c)</c> is what
/// both a construction around a capture and a capture around a construction print, and which
/// of them it is decides where a factory belongs. An afternoon went into that ambiguity
/// before this existed.
/// </remarks>
public sealed class DumpTests
{
	[Fact]
	public void A_lowered_grammar_prints_as_a_tree() =>
		Assert.Equal(
			"""
			Word:
			  Construct => (t)
			    Capture 't'
			      Repeat 1..*
			        Element ['a'..'z']
			Start:
			  Construct => (w)
			    Sequence
			      Capture 'w'
			        Call Word
			      Repeat 0..1
			        Literal '!'

			""".Replace("\r\n", "\n"),
			Graph(
				"Word : @string = t: ['a'..'z']+ => @(t)\n" +
				"Start : @string = w: Word & '!'? => @(w)\n" +
				"parse Start")
				.Dump()
				.Replace("\r\n", "\n"));

	/// <summary>And one rule of it on its own, which is what a report about one can carry.</summary>
	[Fact]
	public void And_one_rule_prints_on_its_own()
	{
		var graph = Graph("Word : @string = t: ['a'..'z']+ => @(t)\nparse Word");

		Assert.StartsWith("Word:", graph.Dump(graph.Rules[0]), StringComparison.Ordinal);
	}

	static RecognitionGraph Graph(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
}

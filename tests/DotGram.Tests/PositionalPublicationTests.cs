using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The forms read from a position and inside a window are written where a directive says
/// <c>positional</c>, and nowhere else: whether the rules are read by methods, by the engine or
/// by one flat method does not decide it (§6.3).
/// </summary>
public sealed class PositionalPublicationTests
{
	/// <summary>A rule a flat method reads, and one that reaches itself, which none can.</summary>
	static string Rules(string rule)
	{
		return rule == "Pair"
			? "Word = ['a'..'z']+\nPair = Word & ',' & Word\n"
			: "Word = ['a'..'z']+\nNested = '(' & Nested & ')' | Word\n";
	}

	/// <summary>Every rendering, and each with and without the word.</summary>
	public static TheoryData<string, bool, bool> Renderings()
	{
		var data = new TheoryData<string, bool, bool>();

		foreach (var rule in new[] { "Pair", "Nested" })
			foreach (var direct in new[] { true, false })
				foreach (var positional in new[] { true, false })
					data.Add(rule, direct, positional);

		return data;
	}

	[Theory]
	[MemberData(nameof(Renderings))]
	public void The_forms_from_a_position_are_there_exactly_where_they_are_asked_for(string rule, bool direct, bool positional)
	{
		var (source, assembly) = Compiled($"{(positional ? "positional " : "")}parse {rule}", direct);
		var forms              = Positional(assembly, "TryParse" + rule);

		Assert.Equal(positional ? 4 : 0, forms);

		// The whole forms are there either way.
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParse" + rule, rule == "Pair" ? "ab,cd" : "((ab))").IsSuccess);
		Assert.Contains("TryParse" + rule + "(string input)", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A publication lowered to one flat method keeps it for the whole input, and a machine of its
	/// own reads the forms the directive asked for: the request does not cost the whole parse its
	/// speed.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_flat_publication_keeps_its_flat_method_and_reads_from_a_position_beside_it(bool direct)
	{
		var (plain, _)          = Compiled("parse Pair", direct);
		var (asked, assembly)   = Compiled("positional parse Pair", direct);

		// Flat without the word: neither a reader of methods nor an engine is written, so the one
		// method the whole input is read by is the flat one. Were the rule ever not lowered, this
		// fails rather than the test going on to hold the engine against itself.
		Assert.False(Rendered(plain), "Pair is no longer lowered to a flat method.");
		Assert.DoesNotContain("RecAt_DotGram_Pair(", plain, StringComparison.Ordinal);

		// And still flat with it: the whole method is written alike, and the forms from a position
		// enter the machine beside it, by methods or by the engine, under its tag.
		Assert.Contains(FlatWhole(plain), asked, StringComparison.Ordinal);
		Assert.True(Rendered(asked), "No machine beside the flat method reads from a position.");
		Assert.Contains("RecAt_DotGram_Pair(", asked, StringComparison.Ordinal);

		var read = EmittedCode.Positioned(assembly, "Grammar", "TryParsePair", "##ab,cd##", 2);

		Assert.True(read.IsSuccess, read.Error);
		Assert.Equal((2L, 5L), (read.Position, read.Length));

		var window = EmittedCode.Positioned(assembly, "Grammar", "TryParsePair", "##ab,cd##", 2, 4);

		Assert.True(window.IsSuccess, window.Error);
		Assert.Equal((2L, 4L), (window.Position, window.Length));

		var answered = EmittedCode.Answered(assembly, "Grammar", "TryParsePair", "ab,cd;", 0);

		Assert.True(answered.Read);
		Assert.Equal(5, answered.At);

		Assert.False(EmittedCode.Positioned(assembly, "Grammar", "TryParsePair", "##ab;cd", 2).IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParsePair", "ab,cd#").IsSuccess);
	}

	/// <summary>
	/// A <c>find</c> of the rule the flat machine also reads from a position under the plain name
	/// does not meet the entry the forms from a position are read by.
	/// </summary>
	[Fact]
	public void A_find_of_the_same_flat_rule_stands_beside_the_forms_from_a_position()
	{
		var (_, assembly) = Compiled("positional parse Pair\nfind Pair", direct: true);

		Assert.Equal(4, Positional(assembly, "TryParsePair"));
		Assert.True(EmittedCode.Positioned(assembly, "Grammar", "TryParsePair", "x ab,cd", 2).IsSuccess);

		var found = assembly.GetType("Grammar")!.GetMethod("FindPair", [typeof(string)])!.Invoke(null, ["x ab,cd y ef,gh"]);

		Assert.Equal(2, ((System.Collections.IEnumerable)found!).Cast<object>().Count());
	}

	/// <summary>
	/// Two directives of one rule, one of them positional and written second: the rule is entered
	/// from a position for the one that asks, whichever is written first.
	/// </summary>
	[Theory]
	[InlineData("Pair", true)]
	[InlineData("Pair", false)]
	[InlineData("Nested", true)]
	[InlineData("Nested", false)]
	public void A_rule_published_twice_is_read_from_a_position_for_the_directive_that_asks(string rule, bool direct)
	{
		var (_, assembly) = Compiled($"parse {rule} as Whole\npositional parse {rule} as Placed", direct);
		var text          = rule == "Pair" ? "##ab,cd" : "##((ab))";

		Assert.Equal(0, Positional(assembly, "TryWhole"));
		Assert.Equal(4, Positional(assembly, "TryPlaced"));
		Assert.True(EmittedCode.Positioned(assembly, "Grammar", "TryPlaced", text, 2).IsSuccess);
	}

	/// <summary>Over segments a position is the parser's to make: asking for one is refused.</summary>
	[Fact]
	public void A_grammar_over_segments_refuses_the_word()
	{
		const string Grammar = "Start : @int = (any* & eos)* => @(0)\npositional parse Start";

		var result = GramCompiler.Compile(
			Grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		var refused = result.Diagnostics.SingleOrDefault(static one => one.Id == GrammarNormalizer.PositionalOverSegments);

		Assert.True(refused is not null, string.Join("; ", result.Diagnostics));

		Assert.Equal(GramSeverity.Error, refused.Severity);
		Assert.Contains("'eos'", refused.Message, StringComparison.Ordinal);

		// At the word, as the parser places what it refuses before the verb.
		Assert.Equal("positional", Grammar.Substring(refused.Position, refused.Length));
	}

	static (string Source, Assembly Assembly) Compiled(string directives, bool direct)
	{
		var result = GramCompiler.Compile(
			Rules(directives.Contains("Pair", StringComparison.Ordinal) ? "Pair" : "Nested") + directives,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Direct = direct });

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		return (source, EmittedCode.Compile(source));
	}

	/// <summary>How many forms of <paramref name="method"/> take a position: never vacuous where it is asked about.</summary>
	static int Positional(Assembly assembly, string method)
	{
		var all = assembly.GetType("Grammar")!.GetMethods().Where(one => one.Name == method).ToArray();

		Assert.NotEmpty(all);

		return all.Count(static one => one.GetParameters().Skip(1).FirstOrDefault()?.Name == "at");
	}

	/// <summary>Whether a reader of methods or an engine is written: anything but a flat method.</summary>
	static bool Rendered(string source)
	{
		// `Reader_` for a group's reader, `ReaderAt_` for the one beside a flat method.
		return source.Contains("ref struct Reader", StringComparison.Ordinal) ||
			source.Contains("entries.Add(new ParserEntry(", StringComparison.Ordinal);
	}

	/// <summary>The flat method that reads the whole input, from its declaration to the next member's.</summary>
	static string FlatWhole(string source)
	{
		var start = source.IndexOf("RecWhole_DotGram_Pair(", StringComparison.Ordinal);

		Assert.True(start >= 0, "No flat whole method.");

		var line = source.LastIndexOf('\n', start) + 1;
		var end  = source.IndexOf("\n\t\t}", start, StringComparison.Ordinal);

		return source.Substring(line, end - line);
	}
}

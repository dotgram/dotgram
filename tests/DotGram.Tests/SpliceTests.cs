using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Joining grammars into one text, and taking a position in it back apart.
/// </summary>
/// <remarks>
/// The half of parser inheritance that has to be right before anything else can be built:
/// a grammar included in another is compiled as one text, and every diagnostic and every
/// <c>#line</c> has to find its way home from a position in the whole (docs/next.md,
/// "Considered: parser inheritance").
/// </remarks>
public sealed class SpliceTests
{
	const string Own      = "using Base;\nStart = a: Base.Word\n";
	const string Included = "trivia = none\nWord = ['a'..'z']+\n";

	static (string Text, SplicedLineMap Map) Joined() =>
		GrammarSplice.Join(
			new GrammarSplice.Part(Own, null, new GrammarLineMap(Own, "Derived.gram")),
			[new GrammarSplice.Part(Included, "Base", new GrammarLineMap(Included, "Base.gram"))]);

	/// <summary>
	/// The including grammar keeps the positions it had, which is the whole reason it goes
	/// first.
	/// </summary>
	[Fact]
	public void What_does_the_including_is_not_moved()
	{
		var (text, _) = Joined();

		Assert.StartsWith(Own, text, StringComparison.Ordinal);
	}

	/// <summary>What is included is copied in byte for byte, so translation stays exact.</summary>
	/// <remarks>
	/// Indenting it inside the wrapper would read better and would shift every position on
	/// every line. The wrapper's braces stand on lines of their own instead.
	/// </remarks>
	[Fact]
	public void And_what_is_included_is_copied_and_not_reformatted()
	{
		var (text, map) = Joined();
		var segment     = map.Segments[1];

		Assert.Equal(Included, text.Substring(segment.Start, segment.Length));
		Assert.Contains("namespace Base\n{\n", text, StringComparison.Ordinal);
	}

	/// <summary>A position on either side of the join finds its own file.</summary>
	/// <remarks>
	/// The point of the exercise, written as one test over both halves rather than two:
	/// what it must show is that the same call answers differently, and a pair of tests
	/// that each look at one half can both pass while the boundary between them is wrong.
	/// </remarks>
	[Fact]
	public void A_position_finds_the_grammar_it_came_from()
	{
		var (text, map) = Joined();

		// `Start` in the including grammar, on its second line.
		var own = Own.IndexOf("Start", StringComparison.Ordinal);

		Assert.True(map.TryMap(own, out var ownFile, out var ownLine, out var ownColumn));
		Assert.Equal("Derived.gram", ownFile);
		Assert.Equal(2, ownLine);
		Assert.Equal(1, ownColumn);

		// `Word` in the included one, on its second line — at a position in the joined text
		// that is nowhere near it.
		var joined = text.IndexOf("Word = ", StringComparison.Ordinal);

		Assert.True(map.TryMap(joined, out var file, out var line, out var column));
		Assert.Equal("Base.gram", file);
		Assert.Equal(2, line);
		Assert.Equal(1, column);
	}

	/// <summary>The wrapper belongs to nobody, and says so rather than guessing.</summary>
	[Fact]
	public void A_position_in_the_wrapper_belongs_to_no_grammar()
	{
		var (text, map) = Joined();
		var wrapper     = text.IndexOf("namespace Base", StringComparison.Ordinal);

		Assert.Equal(-1, map.SegmentAt(wrapper));
		Assert.False(map.TryMap(wrapper, out _, out _, out _));
	}

	/// <summary>
	/// The position one past the last character is a position a diagnostic may name.
	/// </summary>
	/// <remarks>
	/// A rule that wanted one more character fails where that character would have gone,
	/// which is the end of the text and not inside it.
	/// </remarks>
	[Fact]
	public void And_the_end_of_a_grammar_is_a_place()
	{
		var (_, map) = Joined();
		var segment  = map.Segments[1];

		Assert.Equal(1, map.SegmentAt(segment.End));
		Assert.True(map.TryMap(segment.End, out var file, out _, out _));
		Assert.Equal("Base.gram", file);
	}

	/// <summary>Joining one grammar has to be that grammar, to the character.</summary>
	/// <remarks>
	/// The second case is what the first version of this got wrong and this test did not
	/// catch: the newline that keeps a grammar from running into the wrapper was appended
	/// whether or not there was a wrapper, and the example here happened to end with one
	/// already. One character makes the end of the text a different place, and a rule that
	/// failed at the end of the input is reported there — so a host inheriting nothing was
	/// quietly told its last token ran one character longer.
	/// </remarks>
	[Theory]
	[InlineData("using Base;\nStart = a: Base.Word\n")]
	[InlineData("Start = Missing")]
	public void And_including_nothing_changes_nothing(string alone)
	{
		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(alone, null, new GrammarLineMap(alone, "Derived.gram")), []);

		Assert.Equal(alone, text);
		Assert.Equal(alone.Length, Assert.Single(map.Segments).Length);
	}

	[Fact]
	public void And_a_grammar_ending_without_a_newline_does_not_run_into_the_wrapper()
	{
		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part("Start = 'a'", null, null),
			[new GrammarSplice.Part("Word = 'b'", "Base", null)]);

		// Appended, so the first segment is still exactly as long as what went in.
		Assert.Equal("Start = 'a'".Length, map.Segments[0].Length);
		Assert.Contains("'a'\n", text, StringComparison.Ordinal);
		Assert.Contains("'b'\n}\n", text, StringComparison.Ordinal);
	}

	/// <summary>An included grammar has to be named, because it is wrapped in one.</summary>
	[Fact]
	public void And_what_is_included_must_be_named() =>
		Assert.Throws<ArgumentException>(
			() => GrammarSplice.Join(
				new GrammarSplice.Part("Start = 'a'", null, null),
				[new GrammarSplice.Part("Word = 'b'", null, null)]));

	// ── The joined text as the compiler sees it ─────────────────────────────────

	/// <summary>A grammar that says nothing itself and reaches into what it included.</summary>
	const string Deriving = "using Base;\nStart = w: Word\nparse Start\n";

	static GramCompilation Compiled(string text, SplicedLineMap map) =>
		GramCompiler.Compile(text, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			LineMap       = map,
		});

	/// <summary>
	/// A grammar including another compiles, and the `using` reaches into the wrapper.
	/// </summary>
	/// <remarks>
	/// The map is only worth having if the text it maps is a grammar. This is the whole
	/// arrangement end to end: the including grammar's `using Base;` stands above
	/// declarations that come after it in the file, which works because the binder declares
	/// everything before it resolves anything.
	/// </remarks>
	[Fact]
	public void A_grammar_including_another_compiles()
	{
		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(Deriving, null, null),
			[new GrammarSplice.Part("Word = ['a'..'z']+\n", "Base", null)]);

		var result = Compiled(text, map);

		Assert.Empty(result.Diagnostics);
		Assert.NotEmpty(result.Sources);
	}

	/// <summary>An error in the included grammar is placed in the included grammar.</summary>
	/// <remarks>
	/// The one that would catch a boundary off by anything at all: the diagnostic's position
	/// is in the joined text, and what it has to come back as is a line in the file the rule
	/// was actually written in.
	/// </remarks>
	[Fact]
	public void And_an_error_inside_what_was_included_is_placed_there()
	{
		const string Broken = "Word = Missing\n";

		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(Deriving, null, new GrammarLineMap(Deriving, "Derived.gram")),
			[new GrammarSplice.Part(Broken, "Base", new GrammarLineMap(Broken, "Base.gram"))]);

		var diagnostic = Assert.Single(Compiled(text, map).Diagnostics);

		Assert.Equal(GrammarBinder.UndefinedName, diagnostic.Id);

		// Not "somewhere in the joined text": the first line of Base.gram, where `Missing`
		// is written.
		Assert.True(map.TryMap(diagnostic.Position, out var file, out var line, out var column));
		Assert.Equal("Base.gram", file);
		Assert.Equal(1, line);
		Assert.Equal(8, column);
	}

	// ── The grammar's own state, through every way a parser is rendered ──────────

	/// <summary>
	/// A supplied name has to reach every rendering, not only the one it was built in.
	/// </summary>
	/// <remarks>
	/// <para>
	/// `context` was threaded through the general engine and nowhere else, and the two
	/// halves disagreed in silence: a publication passed a context the flat recognizer did
	/// not take, and that recognizer called a factory without the one it did. A valid
	/// grammar produced C# that does not compile — which is the worst shape a generator
	/// defect takes, because the error lands in a file nobody wrote.
	/// </para>
	/// <para>
	/// Written as a corpus over the shapes that pick a rendering, rather than as one test
	/// per rendering: what has to hold is that the choice of rendering is invisible, and a
	/// test that names one rendering stops being about that.
	/// </para>
	/// </remarks>
	[Theory]
	// Flat-valued: one construction over a text capture, no recursion, no recovery.
	[InlineData(
		"context : @C\nValue : @int = d: ['0'..'9']+ => @(context.Count + d.Length)\nparse Value",
		"out int value, C context)")]
	// Flat and valueless, with a guard that names it — no factory anywhere. Written with
	// no capture in it, because a capture is what `Silent` refuses outside the
	// values-in-locals rendering, and with one this is the engine again.
	[InlineData(
		"context : @C\nValue = ['0'..'9']+ & when @(context.Ok())\nparse Value",
		"ref Failure failure, C context)")]
	// Left recursion, read by methods: the fold step's factory is handed it.
	[InlineData(
		"context : @C\nSum : @int = l: Sum & '+' & r: D => @(context.Add(l, r)) | d: D => @(d)\n" +
			"D : @int = t: ['0'..'9'] => @(t.Length)\nparse Sum",
		"Construct_Sum_1(context, ")]
	// A captured call to a flat-valued rule, compiled where the call was.
	[InlineData(
		"context : @C\nOuter : @int = v: Value & '!' => @(v)\n" +
			"Value : @int = d: ['0'..'9']+ => @(context.Count + d.Length)\nparse Outer",
		"Construct_Value(context,")]
	// A publication that also streams, which is a second recognizer over a window.
	[InlineData(
		"context : @C\nStart = ['0'..'9']+ & when @(context.Ok())\nfind Start",
		"global::System.IO.TextReader")]
	public void The_grammar_own_state_reaches_every_rendering(string grammar, string shape)
	{
		var emitted = Emitted(grammar);

		// That the corpus still covers what it says it covers: four grammars that all
		// compile prove nothing if three of them quietly took the same path.
		Assert.Contains(shape, emitted, StringComparison.Ordinal);

		// And that it compiles, which is the whole of what the defect broke —
		// `EmittedCode.Compile` fails on a warning, let alone on a missing argument.
		EmittedCode.Compile(emitted, className: "Grammar");
	}

	static string Emitted(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Empty(result.Diagnostics);

		// The host half a grammar with a `context` needs, written here because the
		// generated code names it and nothing else supplies it.
		return "public class C { public int Count; public bool Ok() => true; public bool Ok(string s) => true; " +
			"public int Add(int a, int b) => a + b; }\n" + result.Sources[0].Text;
	}
}

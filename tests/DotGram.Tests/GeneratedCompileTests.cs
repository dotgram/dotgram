using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Grammars the generator took and wrote C# for that did not compile, or threw on: each is now a
/// parser that compiles under warnings as errors and answers as its other forms do, or a GRAM
/// diagnostic that names what cannot be.
/// </summary>
/// <remarks>
/// The host's C# is compiled with the grammar and asked by a real symbol resolver, as a build asks
/// it: a recognizer that takes the context, one called through the input view and one with type
/// arguments are told apart only by their signatures. <c>CompileOracleTests</c> in the slow suite
/// finds such grammars at random; these are the shapes it and the reviews found, kept by name.
/// </remarks>
public sealed class GeneratedCompileTests
{
	/// <summary>What every grammar here may call in the host.</summary>
	const string Host = """
		public sealed class Ctx { public int N; }
		static bool M(System.ReadOnlySpan<char> input, ref int pos) { if (pos < input.Length && input[pos] == '!') { pos++; return true; } return false; }
		public static bool F<T>(System.ReadOnlySpan<char> input, ref int pos, out int value) { value = System.Runtime.InteropServices.Marshal.SizeOf<T>(); return true; }
		static string Text(System.ReadOnlySpan<byte> text) { return System.Text.Encoding.ASCII.GetString(text.ToArray()); }
		static string Text(string text) { return text; }
		""";

	/// <summary>The same, and recognizers over the input view, which is a type only a grammar that calls one is given.</summary>
	const string Viewing = Host + """
		static bool V(ParserInput<char> input, ref int p, Ctx context) { context.N++; while (input.Peek(p, out var c) && c != '!') p++; return input.TryAdvance(ref p, 1); }
		""";

	/// <summary>
	/// <c>@F&lt;int&gt;</c> is called as written, <c>F&lt;int&gt;(…)</c>: C# cannot infer a type argument
	/// that no parameter carries, and dropping it was CS0411. <c>@F&lt;long&gt;</c> is another call, and
	/// another rule.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recognizer_with_type_arguments_is_called_with_them(bool direct)
	{
		var parser = Built(
			"""
			Narrow : @int = @F<int>
			Wide   : @int = @F<long>
			Both   : @int = a: Narrow & b: Wide & 'x' => @(a * 10 + b)
			parse Both
			""",
			Host, direct);

		Assert.Equal((true, (object?)48), Read(parser, "TryParseBoth", "x"));
	}

	/// <summary>
	/// A rule that takes parameters is a rule only once a call gives it its arguments, and a
	/// publication passes none: refused at the directive (GRAM4013), where it was an exception in
	/// the emitter.
	/// </summary>
	[Fact]
	public void Publishing_a_parameterized_rule_is_refused_at_the_directive()
	{
		var result = Compiled(
			"""
			Word = ['a'..'z']+
			Padded(item, pad: char) : @string = t: item => @(t + pad)
			S : @string = v: Padded(Word, '#') & eof => @(v)
			parse S
			parse Padded as P
			""",
			Host, direct: true);

		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltCall, said.Id);
		Assert.Contains("'Padded' takes 2 parameters", said.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A binding is no capture and no value of C# (§3.5.1): <c>$q: 'a' &amp; $q =&gt; @(q)</c> handed the
	/// C# nothing called <c>q</c>, and the consumer's build said CS0103. A capture of the same text is
	/// what the C# can read.
	/// </summary>
	[Theory]
	[InlineData("T : @string = $q: 'a' & $q => @(q)", true)]
	[InlineData("T : @string = $q: 'a'+ & $q & when @(q.Length > 0) => @(\"x\")", true)]
	[InlineData("T : @string = q: ($q: 'a') & $q => @(q)", false)]
	[InlineData("T : @string = $q: 'a' & $q => @(parserText)", false)]
	public void A_binding_read_as_csharp_is_refused(string rule, bool refused)
	{
		var result = Compiled(rule + "\nparse T", Host, direct: true);

		Assert.Equal(refused, result.Diagnostics.Any(static one => one.Id == GrammarNormalizer.MisplacedBinding));

		if (!refused)
			Assert.Equal((true, (object?)(rule.Contains("parserText", StringComparison.Ordinal) ? "aa" : "a")), Read(Build(result, Host), "TryParseT", "aa"));
	}

	/// <summary>
	/// A guard inside a lookahead that reads a capture made before it is refused (GRAM4006), as a
	/// capture inside one is: a lookahead is a reading of its own and is not handed what the rule
	/// captured, so its guard was handed nothing of that name (CS0103). Written beside the lookahead,
	/// the guard says the same and compiles.
	/// </summary>
	[Theory]
	[InlineData("T = 'a' & c: 'b'* & ?=(when @(c.Length == 0)) | 'a' & 'b'", true)]
	[InlineData("T : @string = 'a' & c: 'b'* & ?!('x' & when @(c.Length > 1)) & any => @(c)", true)]
	[InlineData("T = 'a' & c: 'b'* & when @(c.Length == 0) | 'a' & 'b'", false)]
	[InlineData("T = 'a' & c: 'b'* & ?=('b' & when @(true)) & any", false)]
	public void A_guard_in_a_lookahead_that_reads_a_capture_is_refused(string rule, bool refused)
	{
		var result = Compiled(rule + "\nparse T", Host, direct: true);

		Assert.Equal(refused, result.Diagnostics.Any(static one => one.Id == GrammarNormalizer.UnbuiltCapture));

		if (refused)
			return;

		foreach (var direct in new[] { true, false })
			Build(Compiled(rule + "\nparse T", Host, direct), Host);
	}

	/// <summary>
	/// A recognizer of the host over a span, met only in a recovery's synchronization, is read by
	/// the rule as surely as one in its body: a buffered form is refused for it (GRAM4026), where the
	/// emitter handed the buffer to a method taking a span (CS1503).
	/// </summary>
	[Fact]
	public void A_span_recognizer_in_a_synchronization_refuses_the_buffered_form()
	{
		var result = Compiled(
			"""
			Feed : @string[] = Row* recover @M
			Row  : @string   = w: ['a'..'z']+ & eol => @(w)
			parse Feed stream
			""",
			Host, direct: true);

		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains("external recognizers require contiguous input", said.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A recognizer that takes the context, met only in a synchronization: the machine and the readers
	/// carry the context for it, where they asked only the rule's body whether anything did (CS0103).
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recognizer_taking_the_context_in_a_synchronization_is_handed_it(bool direct)
	{
		var parser = Built(
			"""
			context : @Ctx
			Feed : @string[] = Row* recover @V & eof
			Row  : @string   = w: ['a'..'z']+ & eol => @(w)
			parse Feed
			""",
			Viewing, direct);

		var type    = parser.GetType("Grammar")!;
		var context = Activator.CreateInstance(type.GetNestedType("Ctx")!)!;
		var match   = type.GetMethod("TryParseFeed", [typeof(string), context.GetType()])!.Invoke(null, ["ab\n1x!cd\n", context])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.Equal("ab,cd", string.Join(",", (string[])match.GetType().GetProperty("Value")!.GetValue(match)!));
		Assert.True((int)context.GetType().GetField("N")!.GetValue(context)! > 0);
	}

	/// <summary>
	/// A repetition marked <c>recover</c> and captured whole is its text (§7.3), so the rejected
	/// element its <c>=&gt;</c> builds has no sequence to go in (GRAM4010) — where the readers were
	/// written for a sequence and named a capture they never declared (CS0103), or the streamed form
	/// threw looking for a table of values for the text.
	/// </summary>
	[Theory]
	[InlineData("Rows : @string = rows: (Row* recover ';' => @(\"bad\")) & eof => @(rows)\nRow : @string = t: ['a'..'z']+ & eol => @(t)\nparse Rows")]
	[InlineData("Rows : @int = rows: (Row* recover eol => @(0)) & eof => @(rows.Length)\nRow : @int = t: ['a'..'z']+ & eol => @(t.Length)\nparse Rows stream")]
	public void A_recovery_that_builds_inside_a_capture_of_its_text_is_refused(string grammar)
	{
		var result = Compiled(grammar, Host, direct: true);

		Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.UnbuiltRecovery);
	}

	/// <summary>
	/// The same repetition captured whole with no <c>=&gt;</c> is a capture of its text like any other,
	/// and reads as one in every form.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recovering_repetition_captured_whole_is_its_text(bool direct)
	{
		var parser = Built(
			"Rows : @string = rows: (Row* recover ';') & eof => @(rows)\nRow : @int = t: ['a'..'z']+ & eol => @(t.Length)\nparse Rows",
			Host, direct);

		Assert.Equal((true, (object?)"ab\n1;cd\n"), Read(parser, "TryParseRows", "ab\n1;cd\n"));
	}

	/// <summary>
	/// Over bytes a raw capture is a <c>ReadOnlySpan&lt;byte&gt;</c> that nothing decodes (§6.3): a rule
	/// declared a string and built from one, <c>=&gt; @(t)</c>, is refused the byte form (GRAM4026),
	/// where it was CS0029. Handed to a method of the host's, the same capture builds one.
	/// </summary>
	[Fact]
	public void A_string_built_from_a_raw_capture_is_refused_the_byte_form()
	{
		var refused = Compiled("Item : @string = t: ['a'..'z']+ & ';' => @(t)\nItems : @string[] = Item* & eof\nparse Items stream bytes", Host, direct: true);
		var said    = Assert.Single(refused.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains("'=> @(t)'", said.Message, StringComparison.Ordinal);

		var parser = Built("Item : @string = t: ['a'..'z']+ & ';' => @(Text(t))\nItems : @string[] = Item* & eof\nparse Items stream bytes", Host, direct: true);
		var type   = parser.GetType("Grammar")!;
		var match  = type.GetMethods().Single(one => one.Name == "TryParseItems" && one.GetParameters()[0].ParameterType == typeof(Stream))
			.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes("ab;cd;")), null, null])!;

		Assert.Equal("ab,cd", string.Join(",", (string[])match.GetType().GetProperty("Value")!.GetValue(match)!));
	}

	/// <summary>
	/// A negative lookahead over a choice of literals, in a method compiled flat: the choice's last run
	/// failed to the arena's <c>Fail</c> and not to the lookahead's door, so the lookahead refused
	/// whatever the body did and what followed it was never reached (CS0162, CS0164).
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_negative_lookahead_over_a_choice_of_literals_goes_on_where_it_refuses(bool direct)
	{
		var parser = Built("R = ?!{ eol } & 'o'\nparse R", Host, direct);

		Assert.True(Read(parser, "TryParseR", "o").IsSuccess);
		Assert.False(Read(parser, "TryParseR", "\n").IsSuccess);
		Assert.False(Read(parser, "TryParseR", "x").IsSuccess);
	}

	/// <summary>
	/// The hook a recovery with no <c>=&gt;</c> reports to declares the text a string, and under
	/// <c>SpanCaptures</c> a cut of the input is a span: it is handed the string (CS1503).
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void The_recovery_hook_is_handed_a_string_where_captures_are_spans(bool direct)
	{
		const string hook = "internal static string Told = \"\"; static partial void OnRecovered(string rule, string text, long position, int line, int column, int ordinal, string message) { Told += text + \"|\"; }";

		var parser = Built("R = \"ab\" & ('o' & ';')* recover ';' & eof\nparse R", Host + hook, direct, spans: true);

		Assert.True(Read(parser, "TryParseR", "abo;x;o;").IsSuccess);
		Assert.Equal("x|", parser.GetType("Grammar")!.GetField("Told", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null));
	}

	/// <summary>
	/// A rule that builds a value and is called only from a synchronization: the call graph, and the
	/// commit pass that asks who calls a rule, see it called, as the reading does.
	/// </summary>
	[Theory]
	[InlineData(true, CarrierKind.Auto)]
	[InlineData(true, CarrierKind.Tape)]
	[InlineData(false, CarrierKind.Auto)]
	public void A_rule_that_builds_and_is_called_only_from_a_synchronization_reads_as_written(bool direct, CarrierKind carrier)
	{
		var parser = Built(
			"""
			Rows : @int[] = Row* recover Sync => @(-parserText.Length) & eof
			Row  : @int   = t: ['a'..'z']+ & eol => @(t.Length)
			Sync : @int   = n: ['0'..'9']+ & ';' => @(int.Parse(n))
			parse Rows
			""",
			Host, direct, carrier: carrier);

		var (read, value) = Read(parser, "TryParseRows", "ab\nx12;cde\n");

		Assert.True(read);
		Assert.Equal("2,-1,3", string.Join(",", (int[])value!));
	}

	/// <summary>
	/// A recognizer over the input view, met only in a synchronization, keeps a grammar cut in two
	/// over characters as one in a body does (TerminalInventory), and it compiles.
	/// </summary>
	[Fact]
	public void A_view_recognizer_in_a_synchronization_keeps_a_lexical_grammar_over_characters()
	{
		var result = Compiled(
			"""
			context : @Ctx
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Word = { ['a'..'z']+ }
			}
			Row  : @string   = w: Lexical.Word & ';' => @(w)
			Rows : @string[] = Row* recover @V & eof
			parse Rows
			""",
			Viewing, direct: true, lexical: true);

		Assert.Contains(result.Diagnostics, static one => one.Message.Contains("input-view recognizers require character parsing", StringComparison.Ordinal));
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		EmittedCode.Compile(result.Sources[0].Text, declarationMembers: Viewing, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// A stage of a streamed reading over segments reads its rule under the plain name of the machine's
	/// own wrapper only where that rule is published by <c>find</c>: one published by <c>parse</c> is
	/// rendered under the name that also insists the input ended, and the stage called a method nobody
	/// wrote (CS0103).
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_streamed_stage_of_a_rule_published_by_parse_reads_it_by_a_name_that_exists(bool direct)
	{
		var parser = Built(
			"""
			Rows : @int[] = Row+ recover eos
			Row  : @int   = '(' & t: ['a'..'z']+ & ')' & Std.Quoted('"') & eos => @(t.Length)
			parse Rows
			parse Row as One
			""",
			Host, direct);

		var type   = parser.GetType("Grammar")!;
		var listed = (int[])type.GetMethod("ParseRows", [typeof(System.Collections.Generic.IReadOnlyList<string>)])!.Invoke(null, [new[] { "(ab)\"x\"", "(abc)\"\"" }])!;
		var read   = (System.Collections.IEnumerable)type.GetMethod("ParseRows", [typeof(TextReader)])!.Invoke(null, [new StringReader("(ab)\"x\"\n(abc)\"\"\n")])!;

		Assert.Equal("2,3", string.Join(",", listed));
		Assert.Equal("2,3", string.Join(",", read.Cast<object>()));
	}

	/// <summary>The same, where the publication's first machine is read by methods and its stages by the engine.</summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_streamed_stage_of_a_rule_read_by_methods_reads_it_by_a_name_that_exists(bool direct)
	{
		var parser = Built(
			"""
			Rows : @int[] = Row+
			Some : @int[] = Row*
			Row  : @int   = c: { ['a'..'c']+ } & when @(c.Length > 0) & ';' & eos => @(c.Length)
			parse Rows
			parse Some as Each yield
			parse Row as One
			""",
			Host, direct);

		var type   = parser.GetType("Grammar")!;
		var listed = (int[])type.GetMethod("ParseRows", [typeof(System.Collections.Generic.IReadOnlyList<string>)])!.Invoke(null, [new[] { "ab;", "c;" }])!;
		var read   = (System.Collections.IEnumerable)type.GetMethod("ParseRows", [typeof(TextReader)])!.Invoke(null, [new StringReader("ab;\nc;\n")])!;

		Assert.Equal("2,1", string.Join(",", listed));
		Assert.Equal("2,1", string.Join(",", read.Cast<object>()));
	}

	static GramCompilation Compiled(string grammar, string members, bool direct, bool lexical = false, bool spans = false, CarrierKind carrier = CarrierKind.Auto)
	{
		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			Direct         = direct,
			Lexical        = lexical,
			SpanCaptures   = spans,
			Carrier        = carrier,
		});
	}

	static Assembly Build(GramCompilation result, string members)
	{
		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static Assembly Built(string grammar, string members, bool direct, bool spans = false, CarrierKind carrier = CarrierKind.Auto)
	{
		return Build(Compiled(grammar, members, direct, spans: spans, carrier: carrier), members);
	}

	static (bool IsSuccess, object? Value) Read(Assembly parser, string method, string input)
	{
		var (success, value, _, _) = EmittedCode.Match(parser, "Grammar", method, input);

		return (success, success ? value : null);
	}
}

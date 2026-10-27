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
	/// A binding's name the host's C# answers to is that, and compiles as it always did: refused only
	/// where nothing in the C# around the call does.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_binding_named_as_a_member_of_the_host_reads_the_member(bool direct)
	{
		const string members = Host + "const string q = \"host\";";

		var result = Compiled("T : @string = $q: 'a' & $q => @(q)\nparse T", members, direct);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.MisplacedBinding);
		Assert.Equal((true, (object?)"host"), Read(Build(result, members), "TryParseT", "aa"));
	}

	/// <summary>The same through the generator itself, whose questions to the host are asked before the grammar is read.</summary>
	[Fact]
	public void A_binding_named_as_a_member_of_the_host_reads_the_member_in_a_build()
	{
		var type = GeneratorDriverTests.Build("""
			[DotGram.Gram("T : @string = $q: 'a' & $q => @(q)\nparse T")]
			public partial class Probe
			{
				const string q = "host";
			}
			""").GetType("Probe")!;

		var match = type.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, ["aa"])!;

		Assert.Equal("host", match.GetType().GetProperty("Value")!.GetValue(match));
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
	[InlineData("T : @string = ?=(when @(c)) & c: 'a' => @(c)", false)]
	[InlineData("T : @string = c: ('a' & ?=(when @(c))) => @(c)", false)]
	public void A_guard_in_a_lookahead_that_reads_a_capture_is_refused(string rule, bool refused)
	{
		// A member of the host of the capture's name: a guard that reads it before the capture is
		// bound reads the member, as it always did.
		const string members = Host + "public static bool c = true;";

		var result = Compiled(rule + "\nparse T", members, direct: true);

		Assert.Equal(refused, result.Diagnostics.Any(static one => one.Id == GrammarNormalizer.UnbuiltCapture));

		if (refused)
			return;

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(Compiled(rule + "\nparse T", members, direct), members);

			if (rule.Contains("=> @(c)", StringComparison.Ordinal))
				Assert.Equal((true, (object?)"a"), Read(parser, "TryParseT", "a"));
		}
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

	/// <summary>
	/// A rule published and also read in a repetition, on the engine: the streamed reading's stage calls
	/// the wrapper the engine wrote, the plain one its publication's positional overload has, where
	/// the stage name had become the machine's own, which nobody wrote (CS0103).
	/// </summary>
	[Theory]
	[InlineData("R0 : @int[] = R1*\nR1 : @int = 'a' & when @(true) => @(1)\nparse R0\nparse R1 as P1", "aa", "1,1")]
	[InlineData("R0 : @int[] = R1*\nR1 : @int = c0: ['a'..'c'] & ?!(['a'..'c']+) => @(c0.Length)\nparse R0\nparse R1 as P1", "a", "1")]
	[InlineData("R0 : @int[] = R1+ recover eol\nR1 : @int = 'd' & when @(true) => @(0)\nparse R0\nparse R1 as P1", "dd", "0,0")]
	public void A_published_rule_read_in_a_streamed_repetition_compiles_on_the_engine(string grammar, string input, string values)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Built(grammar, Host, direct);
			var type   = parser.GetType("Grammar")!;
			var read   = (System.Collections.IEnumerable)type.GetMethod("ParseR0", [typeof(TextReader)])!.Invoke(null, [new StringReader(input)])!;

			Assert.Equal(values, string.Join(",", (int[])Read(parser, "TryParseR0", input).Value!));
			Assert.Equal(values, string.Join(",", read.Cast<object>()));
		}
	}

	/// <summary>
	/// What a parse answers is the same over a string, a reader and a stream of bytes, by methods and on
	/// the engine: the outcome, the position, the value and what the refusal says — a rule's own
	/// <c>on fail</c> where it was entered and read nothing (§7.5), and nothing a negative lookahead's body
	/// wanted. A rule that says its own refusal is entered on the engine rather than inlined, and every
	/// analysis that asks whether a call is inlined is told so: a lookahead, a repetition or an atomic
	/// group around it goes on where it did, where its failure was sent through the engine's own.
	/// Two shapes are held to the outcome and the position only: a rule with <c>on fail</c> refused at
	/// its first character where another alternative, or what follows a repetition of it, refuses at
	/// the same place. The engine says the rule's sentence there, as §7.5 has it; the methods and the
	/// reader say what was expected.
	/// </summary>
	[Theory]
	[InlineData("Inner on fail \"Expected an inner.\" = 'a' | 'b' & 'c'\nStart = Inner & Inner & eof", true, "", "a", "x", "ab", "bx", "aa")]
	[InlineData("Inner on fail \"Expected an inner.\" = \"ab\" | \"a\"\nStart = Inner & Inner & eof", true, "", "a", "x", "ab", "abx", "aab")]
	[InlineData("Inner on fail \"Expected an inner.\" = ['0'..'9']+\nStart = Inner & ',' & Inner & eof", true, "", "1,", "x", "1,2")]
	[InlineData("Start = 'a' & ?!({ eol }) & ('a')? & eof", true, "ab", "a\n", "a", "")]
	[InlineData("Inner on fail \"Expected inner.\" = \"ab\"\nStart = ?!Inner & any* & eof", true, "x", "ab", "abx", "", "a")]
	[InlineData("Inner on fail \"Expected inner.\" = \"ab\"\nStart = ?=Inner & any* & eof", true, "ac", "ab", "abx", "", "x")]
	[InlineData("Inner on fail \"Expected inner.\" = \"ab\"\nStart = { Inner | \"ac\" } & eof", false, "ac", "ab", "ax", "", "abc")]
	[InlineData("Inner on fail \"Expected inner.\" = 'a'\nStart = Inner* & 'b' & eof", false, "ab", "b", "aab", "x", "")]
	[InlineData("Start on fail \"Expected start.\" = 'a' & when @(true)", true, "x", "a", "", "ab")]
	[InlineData("Start on fail \"Expected start.\" = 'a'? & when @(true)", true, "x", "a", "", "ab")]
	[InlineData("Inner on fail \"Expected inner.\" = \"ab\"\nStart = Inner", true, "a", "x", "ab", "")]
	[InlineData("Inner : @int on fail \"Expected inner.\" = t: ['a'..'z']+ => @(t.Length)\nStart : @int = i: Inner & '!' => @(i)", true, "!", "ab!", "ab", "")]
	[InlineData("Inner on fail \"Expected inner.\" = ['0'..'9']+\nOuter = Inner & '!'\nStart = Outer & eof", true, "x", "1!", "1x", "")]
	public void A_parse_answers_the_same_in_every_form(string grammar, bool messages, params string[] inputs)
	{
		var said = new System.Collections.Generic.Dictionary<string, (string Form, string Answer)>();

		foreach (var direct in new[] { true, false })
		{
			var type = Build(Compiled(grammar + "\nparse Start stream", Host, direct, bytes: true), Host).GetType("Grammar")!;

			foreach (var input in inputs)
			{
				foreach (var (form, argument) in new (string, object)[]
				{
					("string", input),
					("reader", new StringReader(input)),
					("stream", new MemoryStream(Encoding.ASCII.GetBytes(input))),
				})
				{
					var method = argument is string
						? type.GetMethod("TryParseStart", [typeof(string)])!
						: type.GetMethods().Single(one => one.Name == "TryParseStart" && one.GetParameters()[0].ParameterType == (argument is TextReader ? typeof(TextReader) : typeof(Stream)));
					var answer = Answer(method.Invoke(null, argument is string ? [argument] : [argument, null, null])!);
					var here   = $"{form}, direct {direct}";

					if (!messages)
						answer = string.Join(" ", answer.Split(' ').Take(3));

					if (said.TryGetValue(input, out var first))
						Assert.True(first.Answer == answer, $"\"{input}\": {first.Form} {first.Answer}, {here} {answer}");
					else
						said[input] = (here, answer);
				}
			}
		}

		if (messages && grammar.Contains("on fail", StringComparison.Ordinal) && !grammar.Contains("?", StringComparison.Ordinal))
			Assert.Contains(said.Values, static one => one.Answer.Contains("Expected inner", StringComparison.Ordinal) || one.Answer.Contains("Expected an inner", StringComparison.Ordinal) || one.Answer.Contains("Expected start", StringComparison.Ordinal));

		static string Answer(object match)
		{
			var success = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
			var value   = success ? match.GetType().GetProperty("Value")!.GetValue(match) : null;

			return $"{success} {match.GetType().GetProperty("Position")!.GetValue(match)} {match.GetType().GetProperty("Outcome")!.GetValue(match)} " +
				$"{match.GetType().GetProperty("Error")!.GetValue(match)} {(value is byte[] bytes ? Encoding.ASCII.GetString(bytes) : value)}";
		}
	}

	/// <summary>
	/// What a refusal says where the forms do not yet agree, kept red for the change that makes them:
	/// a root after leading trivia, read on the engine, names what it expected where the methods say
	/// its sentence; a flat method compiling a root says what it expected where the reader and the
	/// stream say the sentence; and the reader's call of a rule read whole inside braces says what it
	/// expected where the engine says the sentence. Outcome and position agree in every one.
	/// </summary>
	[Theory(Skip = "Message parity still open: the root after leading trivia on the engine, the flat root, and the reader's call inside braces")]
	[InlineData("trivia = ' '*\nStart on fail \"Expected start.\" = 'a' & when @(true)", "  x")]
	[InlineData("Start on fail \"Expected start.\" = 'a'", "x")]
	[InlineData("Inner on fail \"Expected inner.\" = 'a'\nStart = { Inner } & when @(true)", "x")]
	public void A_refusal_says_the_same_sentence_in_every_form(string grammar, string input)
	{
		A_parse_answers_the_same_in_every_form(grammar, messages: true, input);
	}

	/// <summary>
	/// A literal past 255 met only in a synchronization is read by the byte form as surely as one in
	/// the rule's body: refused (GRAM4026) rather than written <c>(byte)'Ā'</c> (CS0221). A recovery
	/// that builds nothing is refused the byte form before that, for its hook's line and column.
	/// </summary>
	[Theory]
	[InlineData(true, "Root : @int[] = Row* recover 'Ā' => @(-1)")]
	[InlineData(false, "Root : @int[] = Row* recover 'Ā' => @(-1)")]
	[InlineData(true, "Root = Rows & eof\nRows = Row* recover 'Ā'")]
	[InlineData(true, "Root = (Row* recover 'Ā') & eof")]
	public void A_wide_literal_in_a_synchronization_refuses_the_byte_form(bool direct, string root)
	{
		var result = Compiled(
			$"""
			Row : @int = 'z' & ';' => @(1)
			{root}
			parse Root stream bytes
			""",
			Host, direct, bytes: true);

		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
	}

	/// <summary>
	/// A keyword synchronization under <c>wordboundary</c> looks behind where it begins, and the buffer
	/// keeps that character for it as it keeps one a body looks behind at: read in a buffer of two,
	/// the look read a position already released and threw.
	/// </summary>
	[Theory]
	[InlineData(true, CarrierKind.Immediate, Rows)]
	[InlineData(true, CarrierKind.Tape, Rows)]
	[InlineData(false, CarrierKind.Auto, Rows)]
	[InlineData(true, CarrierKind.Auto, Run)]
	[InlineData(false, CarrierKind.Auto, Run)]
	public void A_keyword_synchronization_keeps_what_it_looks_behind_at(bool direct, CarrierKind carrier, string root)
	{
		var parser = Build(Compiled(
			$"""
			wordboundary = ['a'..'z']
			{root}
			parse Root stream
			""",
			Host, direct, carrier: carrier), Host);

		var type = parser.GetType("Grammar")!;

		foreach (var input in new[] { "z;end", "z;z;xend", "z;q;z;" })
		{
			var whole = Read(parser, "TryParseRoot", input);

			foreach (var size in new[] { 1, 2, 3 })
			{
				var match   = type.GetMethod("TryParseRoot", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), size, null])!;
				var success = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
				var value   = success ? match.GetType().GetProperty("Value")!.GetValue(match) : null;

				Assert.True(
					whole.IsSuccess == success && Equals(whole.Value, value),
					$"\"{input}\" in a buffer of {size}: {success} {value}, over the string {whole.IsSuccess}");
			}
		}
	}

	const string Rows = "Row : @int = ['z'] & ';' => @(1)\nRoot : @string = r: Row* recover \"end\" => @(-1) & eof => @(string.Join(\",\", r))";

	const string Run = "Root : @string = (['z'] & ';')* recover \"end\"";

	/// <summary>
	/// A recognizer that takes the context, met only in a synchronization of a grammar that declares
	/// none, is refused as one in a body is (GRAM4025), where the call was handed a <c>context</c>
	/// nothing declared (CS0103).
	/// </summary>
	[Fact]
	public void A_context_recognizer_in_a_synchronization_needs_a_context()
	{
		const string members = """
			public sealed class Context { public int N; }
			static bool Odd(ParserInput<char> input, ref int p, Context context) { return input.TryAdvance(ref p, 1); }
			""";

		var result = Compiled("Row : @int = 'z' & ';' => @(1)\nRoot : @int[] = Row* recover @Odd\nparse Root", members, direct: true);

		Assert.Contains(result.Diagnostics, static one => one.Severity == GramSeverity.Error && one.Message.Contains("requires a 'context : @T' declaration", StringComparison.Ordinal));
	}

	/// <summary>
	/// A recognizer that takes the context, met only in the synchronization of a grammar read as a
	/// stream whose rules build nothing: every reading carries the context for it (CS0103).
	/// </summary>
	[Theory]
	[InlineData(true, CarrierKind.Auto, "Root = Row* recover @Odd")]
	[InlineData(true, CarrierKind.Tape, "Root = Row* recover @Odd")]
	[InlineData(true, CarrierKind.Immediate, "Root = Row* recover @Odd")]
	[InlineData(false, CarrierKind.Auto, "Root = Row* recover @Odd")]
	[InlineData(true, CarrierKind.Auto, "Root = Rows & eof\nRows = Row* recover @Odd")]
	[InlineData(false, CarrierKind.Auto, "Root = Rows & eof\nRows = Row* recover @Odd")]
	[InlineData(true, CarrierKind.Auto, "Root : @int = r: Row* recover @Odd => @(-1) & eof => @(r.Length)")]
	[InlineData(true, CarrierKind.Immediate, "Root : @int = r: Row* recover @Odd => @(-1) & eof => @(r.Length)")]
	[InlineData(false, CarrierKind.Auto, "Root : @int = r: Row* recover @Odd => @(-1) & eof => @(r.Length)")]
	public void A_context_recognizer_only_in_a_synchronization_is_handed_the_context(bool direct, CarrierKind carrier, string root)
	{
		const string members = """
			public sealed class Context { public int N; }
			static bool Odd(ParserInput<char> input, ref int p, Context context) { context.N++; if (input.Peek(p, out var c) && c % 2 == 1) { p++; return true; } return false; }
			""";

		var parser = Build(Compiled(
			$"""
			context : @Context
			Row : @int = 'z' & ';' => @(1)
			{root}
			parse Root stream
			""",
			members, direct, carrier: carrier), members);

		var type    = parser.GetType("Grammar")!;
		var context = Activator.CreateInstance(type.GetNestedType("Context")!)!;
		var match   = type.GetMethod("TryParseRoot", [typeof(string), context.GetType()])!.Invoke(null, ["z;az;", context])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.True((int)context.GetType().GetField("N")!.GetValue(context)! > 0);
	}

	/// <summary>
	/// What the generated code may not warn of, in a consumer's build with warnings as errors: a trivia
	/// that only skips a nested comment, read with the direct machine over a string, wrote an empty
	/// <c>switch</c> (CS1522) and a field of the recursion's frame nobody assigned (CS0649). Both were
	/// one defect: the cycle through the trivia had no back edge, so its stack was never probed, and a
	/// comment nested a hundred thousand deep ran the stack out.
	/// </summary>
	[Fact]
	public void A_nested_comment_in_trivia_compiles_without_warnings()
	{
		var parser = Built(
			"""
			trivia = { (' ' | Std.Nested("/*", "*/"))* }
			Root : @string = ':' => @(parserText)
			parse Root
			""",
			Host, direct: true);

		Assert.Equal((true, (object?)":"), Read(parser, "TryParseRoot", " /* a /* b */ */ :"));

		const int deep = 100_000;

		Assert.Equal((true, (object?)":"), Read(parser, "TryParseRoot", string.Concat(Enumerable.Repeat("/*", deep)) + string.Concat(Enumerable.Repeat("*/", deep)) + ":"));
	}

	/// <summary>
	/// A token whose value is told where it was written, read through the located door of a grammar cut
	/// into tokens, is told so: the value is read again over the token's text with the door's own
	/// locating, where that reading was told it located nothing and every token said -1.
	/// </summary>
	[Theory]
	[InlineData(true, CarrierKind.Immediate, BuiltToken)]
	[InlineData(true, CarrierKind.Tape, BuiltToken)]
	[InlineData(false, CarrierKind.Auto, BuiltToken)]
	[InlineData(true, CarrierKind.Immediate, CapturedToken)]
	[InlineData(true, CarrierKind.Tape, CapturedToken)]
	[InlineData(false, CarrierKind.Auto, CapturedToken)]
	public void A_located_token_is_located_through_the_per_call_door(bool direct, CarrierKind carrier, string token)
	{
		const string members = """
			public interface ILoc { void Locate(int at, int length); void Locate(int at, int length, int gapStart); }
			public sealed class Loc : ILoc { public Loc() { } public Loc(string text) { } public int At = -1; public void Locate(int at, int length) { At = at; } public void Locate(int at, int length, int gapStart) { At = at; } }
			""";

		var grammar = $$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				{{token}}
			}
			Root : @Loc[] = Lexical.Token*
			parse Root
			""";

		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			Direct         = direct,
			Lexical        = true,
			Carrier        = carrier,
			LocationType   = "Grammar.ILoc",
			LocatedFacade  = "Located",
		});

		var parser = Build(result, members).GetType("Grammar")!;

		int[] At(Type door)
		{
			var read = (System.Array)door.GetMethod("ParseRoot", [typeof(string)])!.Invoke(null, [" ab cd"])!;

			return [.. read.Cast<object>().Select(static one => (int)one.GetType().GetField("At")!.GetValue(one)!)];
		}

		Assert.Equal([1, 4], At(parser.GetNestedType("Located")!));
		Assert.Equal([-1, -1], At(parser));
	}

	/// <summary>
	/// A per-call located grammar large enough that its source is split, whose walk that builds the values
	/// is itself larger than a part: it stays inside the generic class it is written in, where the split
	/// moved it out to the host's class and nothing found it (CS0117, CS0246).
	/// </summary>
	[Fact]
	public void A_per_call_walk_larger_than_a_part_stays_in_its_class()
	{
		const string members = """
			public interface ILoc { void Locate(int at, int length); void Locate(int at, int length, int gapStart); }
			public sealed class Loc : ILoc { public int At = -1; public void Locate(int at, int length) { At = at; } public void Locate(int at, int length, int gapStart) { At = at; } }
			""";

		var grammar = "Root : @Loc = " + string.Join(" | ", Enumerable.Range(0, 25).Select(static i => $"\"k{i}\" => @(new Loc())")) + "\nparse Root";
		var host    = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			Direct         = true,
			Carrier        = CarrierKind.Tape,
			SourceFileSize = 4000,
			LocationType   = "Grammar.ILoc",
			LocatedFacade  = "Located",
		});

		var parser = Build(result, members).GetType("Grammar")!;
		var read   = parser.GetNestedType("Located")!.GetMethod("ParseRoot", [typeof(string)])!.Invoke(null, ["k24"])!;

		Assert.Equal(0, (int)read.GetType().GetField("At")!.GetValue(read)!);
	}

	/// <summary>A token whose value is built by a construction that reads nothing of it.</summary>
	const string BuiltToken = "Token : @Loc = { ['a'..'z']+ } => @(new Loc())";

	/// <summary>A token whose value is built from its whole text alone, which is otherwise not read again.</summary>
	const string CapturedToken = "Token : @Loc = t: { ['a'..'z']+ } => @(new Loc(t))";

	/// <summary>
	/// A grammar whose located door is decided per call compiles where its source is split in parts,
	/// whose readers call the walk that builds the values from outside its generic class (CS0122,
	/// where only the calls of the first part made it more than private), and where it is cut into
	/// tokens whose values are read again over their text (CS7036, where that reading was not told
	/// whether it locates).
	/// </summary>
	[Theory]
	[InlineData("R0 : @string = Twice(eol) => @(S(parserText))\nTwice(item) = item & item", false, 4000)]
	[InlineData("R0 : @int = ?=(R1) => @(parserText.Length) | R1 & any & eol => @(parserText.Length)\nR1 on fail \"Expected R1.\" = ['a'..'c']\nparse R1 as P1", false, 4000)]
	[InlineData("trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tDigits : @int = { ['0'..'9']+ } => @(parserText.Length)\n}\nR0 : @int = (Lexical.Digits)* => @(0)", true, 0)]
	public void A_per_call_door_compiles_split_and_over_tokens(string rules, bool lexical, int split)
	{
		const string members = """
			public interface ILoc { void Locate(int at, int length); void Locate(int at, int length, int gapStart); }
			public sealed class Loc : ILoc { public int At = -1; public void Locate(int at, int length) { At = at; } public void Locate(int at, int length, int gapStart) { At = at; } }
			static string S(string text) { return text; }
			""";

		var grammar = rules + "\nL0 : @Loc[] = L1*\nL1 : @Loc = 'k' & eol => @(new Loc())\nparse R0\nparse L0 as PL";
		var host    = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			Direct         = true,
			Lexical        = lexical,
			Carrier        = lexical ? CarrierKind.Auto : CarrierKind.Tape,
			SourceFileSize = split,
			LocationType   = "Grammar.ILoc",
			LocatedFacade  = "Located",
		});

		var parser = Build(result, members).GetType("Grammar")!;
		var read   = (System.Array)parser.GetNestedType("Located")!.GetMethod("PL", [typeof(string)])!.Invoke(null, ["k\nk\n"])!;

		Assert.Equal(2, read.Length);
	}

	static GramCompilation Compiled(string grammar, string members, bool direct, bool lexical = false, bool spans = false, CarrierKind carrier = CarrierKind.Auto, bool bytes = false)
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
			BufferedBytes  = bytes,
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

using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the reader (<c>Machine.Reader.cs</c>) writes for a binding, a reference and a count of
/// a bound length: each form read on every carrier, and the bookkeeping that keeps a binding's
/// two positions where a reference can read them — handed between the methods of a rule, kept
/// where only a look reads them, apart in every call of a rule.
/// </summary>
/// <remarks>
/// Every grammar here is read by the reader and not by the engine: <see cref="Reader"/> asks
/// the emitted file for the reader's method of the rule and for the comparison it calls, so a
/// grammar that fell back to the engine fails here rather than passing there. The rows every
/// reading has to agree on are <c>BackReferenceTests</c>'.
/// </remarks>
public sealed class BindingReaderTests
{
	static readonly CarrierKind[] Carriers = [CarrierKind.Tape, CarrierKind.Immediate, CarrierKind.Auto];

	const string DollarQuoted = """
		R   = '$' & $tag: (Tag?) & '$' & { (?!('$' & $tag & '$') & any)* } & '$' & $tag & '$'
		Tag = ['a'..'z' | '_'] & ['a'..'z' | '0'..'9' | '_']*
		""";

	const string Fence = """
		R    = $f: '`'{3,} & Info & '\n' & (?!('\n' & $f & '`'* & eof) & any)* & '\n' & $f & '`'* & eof
		Info = [^ '`' | '\n']*
		""";

	const string RawString = """
		R = { $q: '"'{3,} } & { (?!$q & any)* } & $q & ?!'"'
		""";

	[Theory]
	[InlineData("$$$$", true)]
	[InlineData("$$body$$", true)]
	[InlineData("$tag$a $ b $tagx$ c$tag$", true)]
	[InlineData("$tag$body$TAG$", false)]
	[InlineData("$tag$body$ta$", false)]
	[InlineData("$a$unclosed", false)]
	public void Dollar_quoting_closes_at_its_own_tag(string input, bool accepted)
	{
		Reads(DollarQuoted, input, accepted);
	}

	[Theory]
	[InlineData("```c\nx\n```", true)]
	[InlineData("````\na\n```\nb\n````", true)]
	[InlineData("```\na\n`````", true)]
	[InlineData("````\na\n```", false)]
	[InlineData("```\nunclosed", false)]
	public void A_fence_closes_at_a_run_no_shorter(string input, bool accepted)
	{
		Reads(Fence, input, accepted);
	}

	[Theory]
	[InlineData("\"\"\"a\"\"\"", true)]
	[InlineData("\"\"\"\"a\"\"\" b\"\"\"\"", true)]
	[InlineData("\"\"\"\"a\"\"\"\"\"", false)]
	[InlineData("\"\"\"a\"\"", false)]
	public void A_raw_string_closes_at_its_opening_run(string input, bool accepted)
	{
		Reads(RawString, input, accepted);
	}

	// A count of one character is a run; of more than one, turns. Each with its bound read at
	// the count, either side, and a length of zero allowed.
	[Theory]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q}", "aa-bb", true)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q}", "aa-b", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q}", "aa-bbb", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q}", "-", true)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q,}", "aa-bbbb", true)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q,}", "aa-b", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{1,#q}", "aaa-bb", true)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{1,#q}", "aaa-bbbb", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{2,#q}", "a-b", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{2,#q}", "a-bb", false)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q,3}", "aa-bbb", true)]
	[InlineData("R = $q: 'a'* & '-' & 'b'{#q,3}", "aaaa-bbb", false)]
	[InlineData("R = $q: 'a'* & '-' & \"xy\"{#q}", "aa-xyxy", true)]
	[InlineData("R = $q: 'a'* & '-' & \"xy\"{#q}", "aa-xy", false)]
	[InlineData("R = $q: 'a'* & '-' & \"xy\"{#q,}", "-xyxy", true)]
	[InlineData("R = $q: 'a'* & '-' & $r: 'c'* & '-' & \"xy\"{#q,#r}", "a-ccc-xyxy", true)]
	[InlineData("R = $q: 'a'* & '-' & $r: 'c'* & '-' & \"xy\"{#q,#r}", "aa-c-xy", false)]
	public void A_count_reads_its_bounds_where_it_is_reached(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>A count that may stop short is asked back for a turn, as any repetition is.</summary>
	[Theory]
	[InlineData("R = $q: 'a'* & '-' & 'b'{0,#q} & \"bc\"", "aa-bbc", true)]
	[InlineData("R = $q: 'a'* & '-' & \"xy\"{0,#q} & \"xyz\"", "aa-xyxyz", true)]
	public void A_count_gives_a_turn_back(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>A repetition of a binding with no turns bound "", which a reference reads as nothing.</summary>
	[Theory]
	[InlineData("R = ($q: 'a')* & '|' & $q & '|'", "||", true)]
	[InlineData("R = ($q: 'a')* & '|' & $q & '|'", "aa|aa|", true)]
	[InlineData("R = ($q: 'a')* & '|' & $q & '|'", "aa|a|", false)]
	[InlineData("R = ($q: 'a')* & '|' & 'b'{#q} & '|'", "||", true)]
	[InlineData("R = ($q: V)* & '|' & $q & '|'\nV : @int = ['0'..'9'] => @(parserText[0] - '0')", "||", true)]
	[InlineData("R = ($q: V)* & '|' & $q & '|'\nV : @int = ['0'..'9'] => @(parserText[0] - '0')", "12|12|", true)]
	public void No_turns_bind_the_empty_text(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>A greedy binding gives back what the reference after it needs.</summary>
	[Theory]
	[InlineData("R = $q: 'a'+ & $q", "aaaa", true)]
	[InlineData("R = $q: 'a'+ & $q", "aaa", false)]
	[InlineData("R = $q: \"ab\"+ & $q", "abababab", true)]
	[InlineData("R = $q: \"ab\"+ & $q", "ababab", false)]
	public void A_binding_gives_back_what_its_reference_needs(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>A reference at the end of the input fails as any text would, where it stands.</summary>
	[Fact]
	public void A_reference_past_the_end_refuses_where_it_begins()
	{
		foreach (var carrier in Carriers)
		{
			var assembly = Compiled("R = $q: \"ab\" & 'x' & $q\nparse R\n", carrier);
			var (isSuccess, _, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParseR", "abxa");

			Assert.False(isSuccess);
			Assert.Equal(3, position);
			Assert.Contains("$q", error, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A binding written in the alternatives of a choice — methods of their own — reaches the
	/// reference after the choice, whichever bound it; and one bound before a repetition
	/// reaches a reference inside a turn.
	/// </summary>
	[Theory]
	[InlineData("R = ('\\'' & $t: W & '\\'' | $t: W) & '=' & $t\nW = ['a'..'z']+", "'ab'=ab", true)]
	[InlineData("R = ('\\'' & $t: W & '\\'' | $t: W) & '=' & $t\nW = ['a'..'z']+", "ab=ab", true)]
	[InlineData("R = ('\\'' & $t: W & '\\'' | $t: W) & '=' & $t\nW = ['a'..'z']+", "'ab'=ac", false)]
	[InlineData("R = $q: W & (',' & $q & ';')*\nW = ['a'..'z']+", "ab,ab;,ab;", true)]
	[InlineData("R = $q: W & (',' & $q & ';')*\nW = ['a'..'z']+", "ab,ab;,ac;", false)]
	public void A_binding_is_handed_between_the_methods_of_a_rule(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>
	/// A binding only a look reads is kept: the look is a method of its own, handed the two
	/// positions, and nothing outside it reads them.
	/// </summary>
	[Fact]
	public void A_binding_only_a_look_reads_is_kept()
	{
		const string Grammar = "R = $q: W & ':' & (?!$q & any)+\nW = ['a'..'z']+";

		Reads(Grammar, "ab:xyz", true);
		Reads(Grammar, "ab:xab", false);
		Reads(Grammar, "ab:", false);

		var source = Source(Grammar + "\nparse R\n", CarrierKind.Tape);

		Assert.Contains("a0 = p;", source, StringComparison.Ordinal);
		Assert.Contains("b0 = p;", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A binding nobody reads is no locals: the body, as it would be without the binding.
	/// </summary>
	[Fact]
	public void A_binding_nobody_reads_writes_nothing()
	{
		var source = Source("R = $q: ['a'..'z']+ & '!'\nparse R\n", CarrierKind.Tape);

		Assert.DoesNotContain("a0", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Same_DotGram", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule with a binding, called twice and recursively: every call keeps its own, so a
	/// reference reads the binding of the call it is in.
	/// </summary>
	[Theory]
	[InlineData("R = P & ' ' & P\nP = $q: W & '=' & $q\nW = ['a'..'z']+", "ab=ab cd=cd", true)]
	[InlineData("R = P & ' ' & P\nP = $q: W & '=' & $q\nW = ['a'..'z']+", "ab=ab cd=ab", false)]
	[InlineData("E = '<' & $n: N & '>' & E* & \"</\" & $n & '>'\nN = ['a'..'z']+", "<a><b></b><c></c></a>", true)]
	[InlineData("E = '<' & $n: N & '>' & E* & \"</\" & $n & '>'\nN = ['a'..'z']+", "<a><b></a></b>", false)]
	public void Every_call_keeps_its_own_binding(string grammar, string input, bool accepted)
	{
		Reads(grammar, input, accepted);
	}

	/// <summary>
	/// <c>q: ($q: X)</c> captures what <c>q: X</c> would — the value of a rule that has one —
	/// and binds its text.
	/// </summary>
	[Fact]
	public void A_capture_of_a_binding_keeps_the_value()
	{
		const string Grammar = """
			R : @int = q: ($q: N) & '-' & $q => @(q * 10)
			N : @int = ['0'..'9'] => @(parserText[0] - '0')
			parse R
			""";

		foreach (var carrier in Carriers)
		{
			var assembly = Compiled(Grammar, carrier);

			Assert.Equal(70, EmittedCode.Match(assembly, "Grammar", "TryParseR", "7-7").Value);
			Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "7-8").IsSuccess);
		}
	}

	/// <summary>A capture of a reference holds the text the reference matched.</summary>
	[Fact]
	public void A_capture_of_a_reference_holds_its_text()
	{
		const string Grammar = """
			R : @string = $q: W & '=' & r: $q => @(r + "!")
			W = ['a'..'z']+
			parse R
			""";

		foreach (var carrier in Carriers)
			Assert.Equal("ab!", EmittedCode.Match(Compiled(Grammar, carrier), "Grammar", "TryParseR", "ab=ab").Value);
	}

	/// <summary>
	/// A grammar with no binding does not carry the comparison, and no count writes a local:
	/// what the reader writes for bindings is written only where there is one.
	/// </summary>
	[Fact]
	public void A_grammar_without_bindings_carries_nothing_of_them()
	{
		var result = GramCompiler.Compile("R = ['a'..'z']+ & '-' & ('b' & 'c'?){2,3}\nparse R\n", new GramCompilerOptions { Direct = true });
		var source = string.Concat(result.Sources.Select(static one => one.Text));

		EmittedCode.Quiet(result.Diagnostics);

		Assert.DoesNotContain("Same_DotGram", source, StringComparison.Ordinal);
		Assert.DoesNotContain("var n0", source, StringComparison.Ordinal);
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	static void Reads(string grammar, string input, bool accepted)
	{
		foreach (var carrier in Carriers)
		{
			var assembly = Compiled(grammar.Contains("\nparse ", StringComparison.Ordinal) ? grammar : grammar + $"\nparse {RuleOf(grammar)}\n", carrier);
			var (isSuccess, _, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParse" + RuleOf(grammar), input);

			Assert.True(
				isSuccess == accepted,
				$"{carrier}: \"{input}\" is {(accepted ? $"refused ({error} at {position})" : "read")}");
		}
	}

	static string RuleOf(string grammar)
	{
		return grammar.TrimStart().Split(' ', 2)[0];
	}

	static Assembly Compiled(string grammar, CarrierKind carrier)
	{
		var source = Source(grammar, carrier, out var parts);

		return EmittedCode.Compile(source, sourceParts: parts);
	}

	static string Source(string grammar, CarrierKind carrier)
	{
		return Source(grammar, carrier, out _);
	}

	/// <summary>The emitted file, which must be read by the reader: the rule's reader method calls what bindings need.</summary>
	static string Source(string grammar, CarrierKind carrier, out string[] parts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = result.Sources[0].Text;

		parts = [.. result.Sources.Skip(1).Select(static one => one.Text)];

		Assert.Contains("public int Read_" + RuleOf(grammar), string.Concat([source, .. parts]), StringComparison.Ordinal);

		return source;
	}
}

using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Small grammars whose generated C# once failed to compile, each on every carrier and with
/// and without the direct reader.
/// </summary>
/// <remarks>
/// Each shape was legal and accepted without a word, and the generator wrote code the consumer's
/// compiler refused: an error, or a warning that is one under warnings as errors. A test here
/// compiles the shape every way it can be emitted and then reads it, because the code that
/// failed was in a corner one of those ways reaches and the others do not.
/// </remarks>
public sealed class GeneratedShapeTests
{
	/// <summary>Every carrier, with the direct reader and without it.</summary>
	static readonly (CarrierKind Carrier, bool Direct)[] Every =
	[
		(CarrierKind.Auto,      false),
		(CarrierKind.Auto,      true),
		(CarrierKind.Tape,      false),
		(CarrierKind.Tape,      true),
		(CarrierKind.Immediate, false),
		(CarrierKind.Immediate, true),
	];

	public static TheoryData<CarrierKind, bool> Ways
	{
		get
		{
			var data = new TheoryData<CarrierKind, bool>();

			foreach (var (carrier, direct) in Every)
				data.Add(carrier, direct);

			return data;
		}
	}

	static Assembly Compiled(string grammar, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = direct,
			});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text);
	}

	// ── A reading that begins where it is told, in a grammar with no parse ───

	/// <summary>
	/// A rule with trivia published only with <c>find</c> compiles. The entry that reads the
	/// trivia before the rule wrote down where the value began on the failure, a field only a
	/// grammar with a <c>parse</c> publication declares, since only a <c>parse</c> publication's
	/// positional forms read it: <c>Failure</c> had no <c>Began</c> (CS1061).
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_find_over_trivia_with_no_parse_compiles(CarrierKind carrier, bool direct)
	{
		const string grammar =
			"""
			trivia = ' '*
			Start : @int = n: ['0'..'9']+ => @(n.Length)
			find Start
			""";

		var assembly = Compiled(grammar, carrier, direct);

		Assert.Equal(new object?[] { 2, 3 }, EmittedCode.Found(assembly, "Grammar", "FindStart", "12 ab 345"));
	}

	// ── A recovery the engine never runs ──────────────────────────────────────

	/// <summary>
	/// A rule that recovers and is reached only through a negative lookahead compiles, and
	/// recovers there as written: a rule plans its own recovery (§8.2). The lookahead once scanned
	/// the rule rather than running it in states, so no recovering repetition was written, the local
	/// its search for a synchronization reads was declared and never read (CS0219, a failed build
	/// under warnings as errors), and the rule was read as a plain repetition.
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_recovery_reached_only_by_a_negative_lookahead_compiles(CarrierKind carrier, bool direct)
	{
		const string stop =
			"""
			Stop = 'a'+ recover ';'
			Start : @int = ?!Stop & b: (any*) => @(b.Length)
			parse Start
			""";

		var assembly = Compiled(stop, carrier, direct);

		// `Stop` steps over `xyz` as a bad element recovered through the end of the input, so it
		// is there and the negative lookahead refuses; read as a plain `'a'+` it was not.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xyz").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "aaa").IsSuccess);
		Assert.Equal(0, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "").Value);

		const string list =
			"""
			E = 'x'+
			R = (E & ';')* recover ';' & '.'
			Start = ?!R & E
			parse Start
			""";

		assembly = Compiled(list, carrier, direct);

		Assert.Equal("xx", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xx").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "x;x;.").IsSuccess);
	}

	// ── A negative lookahead over a body that cannot refuse ──────────────────

	/// <summary>
	/// A negative lookahead whose body matches nothing as well as something compiles. The
	/// lookahead keeps where it began in a local, declared only where the door its body's
	/// refusals leave by was written; a body that cannot refuse never writes that door, and the
	/// local the entry sets and the way out on a match reads went undeclared (CS0103).
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_negative_lookahead_over_a_body_that_cannot_refuse_compiles(CarrierKind carrier, bool direct)
	{
		const string grammar =
			"""
			D = 'x'? & 'y'?
			Body = (?!D & any)*
			Start : @int = b: Body & any* => @(b.Length)
			parse Start
			find Start
			""";

		var assembly = Compiled(grammar, carrier, direct);

		// `D` matches everywhere, so `?!D` never lets `Body` take a character.
		Assert.Equal(0, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xyz").Value);
		Assert.Equal(0, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "").Value);
	}

	// ── A left-recursive rule with no `=>` whose steps capture (GRAM4039) ─────

	/// <summary>
	/// A left-recursive rule with no <c>=&gt;</c> is not folded, so a step that captures is
	/// refused, every way the grammar could be emitted: the leading call, which names a value so
	/// far nothing builds, and an operand after it, which the record has one place for.
	/// </summary>
	/// <remarks>
	/// Each of these used to be accepted and then fail where the C# was compiled or written: a
	/// step's own capture declared <c>string[]</c> and handed a <c>string</c> (CS1503), or of a
	/// rule's value, collected on the engine and read from a local never declared by the direct
	/// reader the default carrier uses (CS0103); a guard
	/// asking an untyped rule for its value so far, which threw in the generator; a typed rule
	/// whose only capture is the leading call, built by a constructor with no arguments (CS1026
	/// on the engine, CS1729 in the direct reader) or called with too few (CS1501).
	/// </remarks>
	[Theory]
	[MemberData(nameof(FoldCaptures))]
	public void A_step_that_captures_in_a_rule_with_no_construction_is_refused(string rule, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(
			rule + "\nN = ['0'..'9']+\nparse E",
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = direct,
			});

		var refusal = Assert.Single(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltFoldCapture, refusal.Id);
		Assert.Empty(result.Sources);
	}

	public static TheoryData<string, CarrierKind, bool> FoldCaptures
	{
		get
		{
			var data = new TheoryData<string, CarrierKind, bool>();

			foreach (var rule in new[]
			{
				"E = E & '+' & d: N | c: N & '=' & N",
				"E = E & '+' & d: M | c: N & '=' & N\nM = n: N",
				"E = l: E & '+' & N & when @(l.c != null) | c: N & '=' & N",
				"E : @string = l: E & '+' & N & when @(l.Length > 0) | N",
				"E : @string = l: E & '+' & N | N",
				"E = l: E & '+' & N | c: N & '=' & N",
				"E = E & '+' & d: N << 1 | c: N",
			})
				foreach (var (carrier, direct) in Every)
					data.Add(rule, carrier, direct);

			return data;
		}
	}

	/// <summary>
	/// What the refusal leaves alone: a rule with no <c>=&gt;</c> whose steps capture nothing,
	/// which is the record of its base's captures or its text, and a fold whose every alternative
	/// builds, which may capture its leading call and ask it from a guard.
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_rule_with_no_construction_whose_steps_capture_nothing_compiles(CarrierKind carrier, bool direct)
	{
		var record = Compiled("E = E & '+' & N | c: N & '=' & N\nN = ['0'..'9']+\nparse E", carrier, direct);
		var value  = EmittedCode.Match(record, "Grammar", "TryParseE", "12=3+4").Value!;

		Assert.Equal("12", value.GetType().GetProperty("C")!.GetValue(value));

		var text = Compiled("E : @string = E & '+' & N | N\nN = ['0'..'9']+\nparse E", carrier, direct);

		Assert.Equal("1+2+34", EmittedCode.Match(text, "Grammar", "TryParseE", "1+2+34").Value);

		var folded = Compiled(
			"E : @string = l: E & '+' & n: N & when @(l.Length < 3) => @(l + n) | n: N => @(n)\nN = ['0'..'9']\nparse E",
			carrier,
			direct);

		Assert.Equal("123", EmittedCode.Match(folded, "Grammar", "TryParseE", "1+2+3").Value);
		Assert.False(EmittedCode.Match(folded, "Grammar", "TryParseE", "1+2+3+4").IsSuccess);
	}

	// ── A helper nothing calls ───────────────────────────────────────────────

	/// <summary>
	/// Every helper that says how far a literal run matched is written only where a scanner
	/// calls it. The support of a file read by methods carried one of its own that nothing
	/// called (IDE0051 in every such file), compiled by every consumer for nothing.
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_helper_for_how_far_a_run_matched_is_written_only_where_it_is_called(CarrierKind carrier, bool direct)
	{
		foreach (var grammar in new[]
		{
			// A number joined with `~`: no seam between its parts, and no run to compare.
			"Start = ['0'..'9']+ ~ ('.' & ['0'..'9']+)?\nparse Start",
			// The same inside a reading that reaches itself, which is read by methods with a tape.
			"trivia = ' '*\n" +
			"Value : @int = '[' & items: Value* & ']' => @(items.Length) | n: Number => @(n)\n" +
			"Number : @int = d: ['0'..'9']+ ~ f: ('.' & ['0'..'9']+)? => @(d.Length)\n" +
			"parse Value",
			// A literal run a scanner compares in one go, and refuses on.
			"Start = { \"ab\" & 'c' }+ & eof\nparse Start",
			"trivia = ' '*\nValue : @int = '[' & items: Value* & ']' => @(items.Length) | { \"ab\" & 'c' } => @(1)\nparse Value",
		})
		{
			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = direct,
				});

			EmittedCode.Quiet(result.Diagnostics);

			var text = result.Sources[0].Text;

			foreach (Match declared in Regex.Matches(text, @"static int (Reach_DotGram\w*)\("))
			{
				var name = declared.Groups[1].Value;

				Assert.True(
					Regex.Matches(text, name + @"\(").Count > 1,
					$"{name} is written and never called:\n\n{text}");
			}

			EmittedCode.Compile(text);
		}
	}

	/// <summary>
	/// A reader that keeps no tape of ways back declares the constant that stands in for one
	/// only where it names it. A number joined with <c>~</c> leaves nothing to give back, and
	/// its reader carried the constant with nothing reading it (IDE0051).
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_reader_with_no_tape_declares_none_it_does_not_name(CarrierKind carrier, bool direct)
	{
		foreach (var grammar in new[]
		{
			"Start = ['0'..'9']+ ~ ('.' & ['0'..'9']+)?\nparse Start",
			// The JSON example's shape: values in lists, and a number joined with `~`.
			"""
			namespace Lexical
			{
				trivia = none

				Digits   = ['0'..'9']+
				Fraction = '.' & Digits
				Body     = [^ '"']*
			}

			trivia = [' ' | '\n']*

			List(item, sep) : item[] = item & (sep & item)*

			Json   : @object = value: Value & eof => @(value)
			Value  : @object = value: Object => @(value)
			                 | value: Array  => @(value)
			                 | text:  Text   => @(text)
			                 | value: Number => @(value)
			                 | "null"        => @(null!)
			Object : @object = '{' & members: List(Member, ',') & '}' => @(members)
			                 | '{' & '}'                              => @(null!)
			Array  : @object = '[' & items: List(Value, ',') & ']'    => @(items)
			                 | '[' & ']'                              => @(null!)
			Member : @object = name: Text & ':' & value: Value => @(value)
			Text   : @object = '"' ~ body: Lexical.Body ~ '"' => @(body)
			Number : @object = digits: Lexical.Digits ~ fraction: Lexical.Fraction? => @(digits + fraction)

			parse Json
			""",
		})
		{
			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = direct,
				});

			EmittedCode.Quiet(result.Diagnostics);

			var text = result.Sources[0].Text;

			foreach (Match reader in Regex.Matches(text, @"ref struct Reader_DotGram\w*\r?\n(\t*)\{(?s:.*?)\n\1\}"))
				if (reader.Value.Contains("ways = null;", StringComparison.Ordinal))
					Assert.True(
						Regex.Matches(reader.Value, @"\bways\b").Count > 1,
						$"The reader declares a constant for a tape it never names:\n\n{reader.Value}");

			EmittedCode.Compile(text);
		}
	}

	// ── The engine after a negative lookahead on a reference ─────────────────

	/// <summary>
	/// A value built after a negative lookahead on a reference gave its turn back is built on
	/// every carrier, by the engine as by the reader. The engine over characters threw while it
	/// built the list (ArgumentOutOfRangeException in its materializer); a grammar that asks to
	/// be read over tokens and is read over characters instead (GRAM5004) gets that engine too.
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_value_after_a_negative_lookahead_on_a_reference_is_built(CarrierKind carrier, bool direct)
	{
		const string grammar =
			"""
			T = $q: ['a'..'b']* & ?!$q & 'a' | "ab"
			Item : @string = t: T => @(t)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var assembly = Compiled(grammar, carrier, direct);

		foreach (var (input, value) in new[] { ("baab", "ba|ab"), ("ab", "ab"), ("abab", "ab|ab"), ("", "") })
		{
			var read = EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input);

			Assert.True(read.IsSuccess, input + ": " + read.Error);
			Assert.Equal(value, read.Value);
		}
	}

	/// <summary>
	/// A capture kept in the arena closes at a cost that does not grow with what it holds. A
	/// close searched back over every record inside the capture for its opening, and a
	/// repetition inside it, given back a turn at a time, closed it once per turn: quadratic in
	/// the turns. And where a later turn of a loop around the capture had opened it again, the
	/// close given back into an earlier turn found its opening by searching and kept searching
	/// at every turn after. Counted, not timed (D144): the records searched, at a size and at
	/// four times it, on every carrier of the engine.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_capture_kept_in_the_arena_closes_in_constant_time(CarrierKind carrier)
	{
		foreach (var (grammar, tails) in new[]
		{
			(
				"""
				Item : @string = t: "aa"+ => @(t)
				Root : @string = i: Item & "aa" & eof => @(i)
				parse Root
				""",
				new[] { ("c", false), ("", true) }
			),
			(
				"""
				Item : @string = t: "aa"+ => @(t)
				Root = (Item & "aa" & "b")* & "z" & eof
				parse Root
				""",
				new[] { ("baaaac", false), ("bz", true) }
			),
		})
		{
			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = false,
					CountRules = true,
				});

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text, symbols: ["DOTGRAM_COUNTS"]);
			var searched = assembly.GetType("Grammar")!.GetField(
				"Searched_DotGram", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;

			long Searched(string input, bool read)
			{
				searched.SetValue(null, 0L);

				Assert.Equal(read, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).IsSuccess);

				return (long)searched.GetValue(null)!;
			}

			foreach (var (tail, read) in tails)
			{
				var small = Searched(string.Concat(Enumerable.Repeat("aa", 64)) + tail, read);
				var large = Searched(string.Concat(Enumerable.Repeat("aa", 256)) + tail, read);

				Assert.True(large <= 4 * small + 64, $"searched {small} records at 64 turns and {large} at 256 ({tail})");
			}
		}
	}
}

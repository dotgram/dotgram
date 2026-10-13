using System.Reflection;

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
	/// A rule that recovers and is reached only through a negative lookahead compiles. The
	/// lookahead scans the rule rather than running it in states, so no recovering repetition is
	/// written, and the local its search for a synchronization reads was declared and never read
	/// (CS0219, a failed build under warnings as errors).
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

		Assert.Equal(3, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xyz").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "aaa").IsSuccess);

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

	// ── A left-recursive rule with no `=>` whose steps capture (GRAM4037) ─────

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
}

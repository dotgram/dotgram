using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

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
}

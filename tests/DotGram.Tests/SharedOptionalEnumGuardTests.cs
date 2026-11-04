using System.Collections.Generic;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A guard over an optional capture of a value type sees null where the capture read nothing, also
/// when another alternative of the rule names a capture the same: the name is one per alternative,
/// and an absent enum is not the enum's zero.
/// </summary>
public sealed class SharedOptionalEnumGuardTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	const string Day = "M : @System.DayOfWeek = 'a' => @(System.DayOfWeek.Sunday) | 'b' => @(System.DayOfWeek.Monday)\n";

	[Theory]
	[MemberData(nameof(Readings))]
	public void An_absent_enum_is_null_in_the_guard_with_the_name_alone(CarrierKind carrier, bool direct)
	{
		const string Rules = "T : @string = 'y' & m: M? & when @(m is null) => @(\"null\") | 'y' & n: M? => @(\"refused\")\n";

		Assert.Equal("null", Answer(Day + Rules, "y", carrier, direct));
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void An_absent_enum_is_null_in_the_guard_with_the_name_shared(CarrierKind carrier, bool direct)
	{
		const string Rules = "T : @string = 'x' & m: M? => @(\"x\") | 'y' & m: M? & when @(m is null) => @(\"null\") | 'y' & n: M? => @(\"refused\")\n";

		Assert.Equal("null", Answer(Day + Rules, "y", carrier, direct));
	}

	static string Answer(string grammar, string input, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(grammar + "parse T", new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Direct        = direct,
		});
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(result.Sources).Text), "Grammar", "TryParseT", input);

		return match.IsSuccess ? (string)match.Value! : "<refused " + match.Error + ">";
	}
}

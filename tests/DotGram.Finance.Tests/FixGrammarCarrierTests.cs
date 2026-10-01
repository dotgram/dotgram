using System;
using System.Linq;
using System.Reflection;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// FixGrammar pins <c>Carrier = GramCarrier.Immediate</c> (FixGrammar.cs) rather than leaving
/// it to <c>Auto</c>: the switch that decides a field's shape reads state the immediately
/// preceding field's own construction wrote, which is only right once a construction runs
/// where it is read rather than later, on the tape, in a replay. The shape that depends on it
/// -- a length-and-data pair -- is held to both carriers in <c>FixBinaryPairCarrierTests</c>
/// (DotGram.Tests, which carries the reference to the generator this project deliberately
/// does not). This only checks that the pin itself is where it belongs, so a change to the
/// generator's own follow analysis cannot move FixGrammar onto the tape without anyone
/// noticing.
/// </summary>
public sealed class FixGrammarCarrierTests
{
	// GramCarrier (Auto = 0, Tape = 1, Immediate = 2) is embedded by the generator into
	// DotGram.Finance itself rather than shipped as a runtime type (no runtime assembly
	// ships at all, docs/syntax.md §6.1), and Roslyn hides a type embedded that way from
	// every project that only references the assembly -- InternalsVisibleTo included -- so
	// it cannot be named here even though this project can see FixGrammar's other internals.
	// Read as attribute metadata instead, which answers the same question without it.
	const int Immediate = 2;

	[Fact]
	public void Every_reading_of_FixGrammar_pins_the_immediate_carrier()
	{
		var readings = typeof(FixGrammar)
			.GetCustomAttributesData()
			.Where(one => one.AttributeType.FullName is "DotGram.GramAttribute" or "DotGram.GramOptionsAttribute")
			.ToList();

		Assert.NotEmpty(readings);

		foreach (var reading in readings)
		{
			var hasCarrier = reading.NamedArguments.Any(one => one.MemberName == "Carrier");

			Assert.True(hasCarrier, $"{reading.AttributeType.Name} on FixGrammar does not pin Carrier.");

			var carrier = reading.NamedArguments.First(one => one.MemberName == "Carrier");

			Assert.Equal(Immediate, (int)carrier.TypedValue.Value!);
		}
	}
}

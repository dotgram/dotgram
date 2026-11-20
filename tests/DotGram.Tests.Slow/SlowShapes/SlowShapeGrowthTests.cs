using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Tests.SlowShapes;

/// <summary>
/// The grammars as they were before they were rewritten to be fast, refused (or, for a contrast row, read) at six sizes, doubling from the input's first, which for most is 2,048 up to 64 KB: what each
/// refusal costs in rule entries, and the exponent between the smallest and the largest.
/// </summary>
/// <remarks>
/// <para>
/// These are fixtures for the generator and not examples. The rewrites of the shipped grammars made them fast by changing what they say
/// (atomic runs, a publication in braces); what is meant to make them fast is the generator, so the old grammars are kept as they were,
/// in <c>Sources</c>, and each input that was slow is a row here with the exponent it costs today.
/// </para>
/// <para>
/// <b>Flipping a row.</b> An input not yet fixed is held to its recorded exponent, within <see cref="SlowShapeLadder.Tolerance"/>, in
/// either direction: the test fails when the refusal gets cheaper, and the message says to set <c>Linear</c> on the input, which holds
/// it to <see cref="SlowShapeLadder.LinearBound"/> from then on. A shape's <see cref="GeneratorItem"/> says which generator work does it:
/// <see cref="GeneratorItem.ClassA"/> for a refused loop that reads its prefix again for every retry (the intermediate form's resumable
/// replay and the C# emitter that follows it), <see cref="GeneratorItem.Seal"/> for the square of a nest's depth that sealing a group costs.
/// </para>
/// <para>
/// What is held is a count of rule entries, which no machine changes; the milliseconds printed beside it are directional. The counters are
/// static, so this is in <see cref="Alone"/>. <c>SLOWSHAPES_REPORT</c> names a directory the table is appended to.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class SlowShapeGrowthTests(Xunit.ITestOutputHelper output)
{
	public static IEnumerable<TheoryDataRow<string, string>> Rows()
	{
		return SlowShapes.All.SelectMany(static shape => shape.Inputs.Select(input => new TheoryDataRow<string, string>(shape.Id, input.Name)));
	}

	[Theory]
	[MemberData(nameof(Rows))]
	public void A_kept_shape_costs_what_it_is_recorded_to(string shapeId, string inputName)
	{
		var shape  = SlowShapes.All.Single(one => one.Id == shapeId);
		var input  = shape.Inputs.Single(one => one.Name == inputName);
		var ladder = SlowShapeLadder.Climb(SlowShapeCompiler.Parser(shape), shape.Entry, input);
		var lines  = new List<string>();

		try
		{
			SlowShapeLadder.Hold(shape, input, ladder, lines.Add);
		}
		finally
		{
			foreach (var line in lines)
				output.WriteLine(line);

			SlowShapeLadder.Report(lines);
		}
	}
}

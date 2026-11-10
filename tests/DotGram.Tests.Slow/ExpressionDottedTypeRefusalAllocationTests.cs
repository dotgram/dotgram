using System;
using System.Diagnostics;
using System.Linq;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A dotted name that names no type, refused, allocates what its parts need, not their square.
/// </summary>
/// <remarks>
/// <para>
/// <c>NamedType</c> reads a word and then as many dotted parts as there are, and asks the reading's
/// context whether the name they make is a type. Where it is not, the repetition gives a part back and
/// the guard asks again about the shorter name, down to the head alone. A parameter typed
/// <c>MissingRoot.A.A.…</c>, whose root is nowhere, is refused only once every one of those prefixes
/// has been asked about.
/// </para>
/// <para>
/// <b>The gate is bytes an element, at four doubling sizes</b>, as in
/// <see cref="WebListRefusalAllocationTests"/>: a reading that built the parts and the name again for
/// every shorter prefix doubles the bytes a part at every doubling of the parts.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ExpressionDottedTypeRefusalAllocationTests
{
	/// <summary>Doubling sizes, in dotted parts after the head.</summary>
	static readonly int[] Sizes = [100, 200, 400, 800];

	/// <summary>How many runs a size is measured over; the smallest of them is kept.</summary>
	const int Runs = 5;

	[Fact]
	public void A_parameter_typed_by_an_unresolved_dotted_name_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(ExpressionParser.TryParse(Parameter(size)).IsSuccess, $"a type of {size} parts was read.");
	}

	/// <summary>A type whose name has more parts than a name may have before the scope is asked how deep its types go.</summary>
	[Fact]
	public void A_type_with_a_long_name_is_still_named_by_it()
	{
		const string type = "DotGram.Tests.Deep.N1.N2.N3.C1.C2.C3.C4.C5";

		var match = ExpressionParser.TryParse("(" + type + " x) => x");

		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(typeof(Deep.N1.N2.N3.C1.C2.C3.C4.C5), match.Value.Parameters[0].Type);

		match = ExpressionParser.TryParse("() => " + type + ".Value");

		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(typeof(int), match.Value.Body.Type);
	}

	/// <summary>
	/// A generic name asks nothing while it is read, so one longer than any type is read to its
	/// arguments and refused where the type is looked for, naming the whole of it.
	/// </summary>
	[Fact]
	public void A_long_generic_name_is_still_refused_where_it_is_built()
	{
		var name  = "MissingRoot" + string.Concat(Enumerable.Repeat(".A", 100));
		var match = ExpressionParser.TryParse("(" + name + "<int> x) => x");

		Assert.False(match.IsSuccess);
		Assert.Equal($"The type or namespace name '{name}<>' could not be found.", match.Error);
	}

	[Fact]
	public void A_parameter_typed_by_an_unresolved_dotted_name_allocates_about_as_many_bytes_a_part_at_every_size()
	{
		var texts = Sizes.Select(Parameter).ToArray();

		// The largest size first, run and discarded: the reader's thread-static pools grow to fit
		// it here, and not in whichever measured run asks them for more room next.
		ExpressionParser.TryParse(texts[^1]);

		var perPart = new double[Sizes.Length];

		for (var i = 0; i < Sizes.Length; i++)
		{
			var (bytes, elapsed) = Allocated(texts[i]);

			perPart[i] = bytes / (double)Sizes[i];
			TestContext.Current.TestOutputHelper?.WriteLine(
				$"{Sizes[i]} parts: {bytes:N0} bytes, {perPart[i]:F1} a part, {elapsed.TotalMilliseconds:F2} ms");
		}

		for (var i = 1; i < Sizes.Length; i++)
			Assert.True(
				perPart[i] <= perPart[i - 1] * 1.15,
				$"At {Sizes[i - 1]} parts a refusal allocated {perPart[i - 1]:F1} bytes each, and at " +
				$"{Sizes[i]} it allocated {perPart[i]:F1} bytes each — {perPart[i] / perPart[i - 1]:F2} " +
				"times as many, where a name asked about once a prefix would stay flat.");
	}

	/// <summary>The fewest bytes a refusal of <paramref name="text"/> allocates across <see cref="Runs"/> runs, and the shortest time.</summary>
	static (long Bytes, TimeSpan Elapsed) Allocated(string text)
	{
		var fewest   = long.MaxValue;
		var shortest = TimeSpan.MaxValue;

		for (var run = 0; run < Runs; run++)
		{
			var before = GC.GetAllocatedBytesForCurrentThread();
			var clock  = Stopwatch.StartNew();

			ExpressionParser.TryParse(text);

			clock.Stop();

			fewest   = Math.Min(fewest, GC.GetAllocatedBytesForCurrentThread() - before);
			shortest = clock.Elapsed < shortest ? clock.Elapsed : shortest;
		}

		return (fewest, shortest);
	}

	/// <summary>A lambda whose parameter is typed by a name of <paramref name="parts"/> parts after a root nothing declares.</summary>
	static string Parameter(int parts)
	{
		return "(MissingRoot" + string.Concat(Enumerable.Repeat(".A", parts)) + " x) => x";
	}
}

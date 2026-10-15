extern alias tape;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using DotGram.Examples.Expressions;
using DotGram.Examples.Languages;

using Xunit;

using TapeCalculator = tape::DotGram.Examples.Expressions.Calculator;
using TapeFilter = tape::DotGram.Examples.Languages.Filter;

namespace DotGram.Tests;

/// <summary>
/// Calculator and Filter, which name the immediate carrier with <c>ConstructionsMayThrow</c>,
/// answer every text as the tape does: the value, the refusal, or the exception.
/// </summary>
/// <remarks>
/// <para>
/// Each has a construction that throws for some of what it may be handed — Calculator's `/` on a
/// zero divisor, `int.Parse` on digits past <c>int.MaxValue</c>, Filter's <c>decimal.Parse</c> on a
/// number read with a space inside it — and a reading that builds it before the parse has decided
/// whether it stands: `1/0=` divides before the `=` is refused, `cpu=0 .75 mem=2048` parses
/// `0 .75` before the text is. The tape's reading of each (tests/DotGram.Examples.Tape) runs every
/// construction once the parse has accepted, so the two must agree on every text, throwers
/// included; without the option the immediate reading would throw where the tape refuses.
/// </para>
/// <para>
/// The texts are a handful of shapes and, from each, every prefix and every one-character
/// deletion, the way the expression language's agreement walks its corpus
/// (<see cref="ExpressionLanguage.CarrierAgreementTests"/>).
/// </para>
/// </remarks>
public sealed class ExampleCarrierTests
{
	static readonly string[] Sums =
	[
		"1 + 2 * 3",
		"(1 + 2) * 3 - 4 / 2",
		"2 ^ 3 ^ 2",
		"-(1 + 2) * -3",
		"1/0",
		"1/0=",
		"8 / (2 - 2) + 1",
		"8 / (2 - 2) )",
		"1.5 * 2",
		"0.5 / 0",
		"99999999999 + 1",
		"99999999999 +",
	];

	static readonly string[] Filters =
	[
		"Price > 10 AND Country IN ('UK', 'DE') OR NOT Discontinued",
		"cpu=0.75 mem=2048",
		"cpu=0 .75 mem=2048",
		"cpu=0 .75",
		"Note = null OR Note <> 'it''s'",
		"(Price < 1 OR Price > 10) AND Country = 'FR'",
		"Size IN (1, 2 .5, 3)",
		"Size IN (1, 2.5, 3)",
	];

	[Fact]
	public void Calculator_answers_as_the_tape()
	{
		var threw = Alike(Sums, IntImmediately, IntOnTheTape) +
			Alike(Sums, DecimalImmediately, DecimalOnTheTape) +
			Alike(Sums, TreeImmediately, TreeOnTheTape);

		// Not a bound: that some texts throw on the tape says the walk reaches the throwers, so
		// that agreement is not covering for texts nothing throws on.
		Assert.True(threw > 0, "No text threw on the tape.");

		static string IntImmediately(string text)
		{
			var match = Calculator.TryEvaluateInt(text);

			return match.IsSuccess ? "accepted " + match.Value : $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string IntOnTheTape(string text)
		{
			var match = TapeCalculator.Tape.TryEvaluateInt(text);

			return match.IsSuccess ? "accepted " + match.Value : $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string DecimalImmediately(string text)
		{
			var match = Calculator.TryEvaluateDecimal(text);

			return match.IsSuccess
				? "accepted " + match.Value.ToString(CultureInfo.InvariantCulture)
				: $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string DecimalOnTheTape(string text)
		{
			var match = TapeCalculator.Tape.TryEvaluateDecimal(text);

			return match.IsSuccess
				? "accepted " + match.Value.ToString(CultureInfo.InvariantCulture)
				: $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string TreeImmediately(string text)
		{
			var match = Calculator.TryBuildTree(text);

			return match.IsSuccess ? "accepted " + Shown(match.Value) : $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string TreeOnTheTape(string text)
		{
			var match = TapeCalculator.Tape.TryBuildTree(text);

			return match.IsSuccess ? "accepted " + Shown(match.Value) : $"{match.Outcome} at {match.Position}: {match.Error}";
		}
	}

	[Fact]
	public void Filter_answers_as_the_tape()
	{
		var threw = Alike(Filters, Immediately, OnTheTape);

		Assert.True(threw > 0, "No text threw on the tape.");

		static string Immediately(string text)
		{
			var match = Filter.TryParseFilter(text);

			return match.IsSuccess ? "accepted " + Shown(match.Value) : $"{match.Outcome} at {match.Position}: {match.Error}";
		}

		static string OnTheTape(string text)
		{
			var match = TapeFilter.Tape.TryParseFilter(text);

			return match.IsSuccess ? "accepted " + Shown(match.Value) : $"{match.Outcome} at {match.Position}: {match.Error}";
		}
	}

	/// <summary>
	/// The examples name the immediate carrier and are given it, and their tape readings are not:
	/// the store each carrier rents is the evidence (ExpressionCarrierTests says why it matters).
	/// </summary>
	[Fact]
	public void The_examples_are_carried_immediately()
	{
		const BindingFlags Nested = BindingFlags.Public | BindingFlags.NonPublic;

		Assert.NotNull(typeof(Calculator).GetNestedType("ImmediateValues", Nested));
		Assert.NotNull(typeof(Filter).GetNestedType("ImmediateValues", Nested));
		Assert.NotNull(typeof(TapeCalculator.Tape).GetNestedType("DirectValues", Nested));
		Assert.NotNull(typeof(TapeFilter.Tape).GetNestedType("DirectValues", Nested));
		Assert.Null(typeof(TapeCalculator.Tape).GetNestedType("ImmediateValues", Nested));
		Assert.Null(typeof(TapeFilter.Tape).GetNestedType("ImmediateValues", Nested));
	}

	/// <summary>The texts the two readings must agree on; how many threw on the tape.</summary>
	static int Alike(string[] shapes, Func<string, string> immediate, Func<string, string> tape)
	{
		var differed = new List<string>();
		var threw    = 0;

		foreach (var text in Texts(shapes))
		{
			var expected = Answer(() => tape(text));
			var actual   = Answer(() => immediate(text));

			if (expected.StartsWith(Threw, StringComparison.Ordinal))
				threw++;

			if (expected != actual)
				differed.Add($"\"{text}\"\n  tape:      {expected}\n  immediate: {actual}");
		}

		Assert.True(differed.Count == 0,
			$"{differed.Count} texts told the carriers apart; the first three:\n" + string.Join("\n", differed.Take(3)));

		return threw;
	}

	/// <summary>Every shape, every prefix of one, and every one-character deletion from one.</summary>
	static IEnumerable<string> Texts(string[] shapes)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var shape in shapes)
		{
			if (seen.Add(shape))
				yield return shape;

			for (var length = 1; length < shape.Length; length++)
				if (seen.Add(shape.Substring(0, length)))
					yield return shape.Substring(0, length);

			for (var at = 0; at < shape.Length; at++)
				if (seen.Add(shape.Remove(at, 1)))
					yield return shape.Remove(at, 1);
		}
	}

	/// <summary>What a reading made of a text: its answer, or what it threw.</summary>
	static string Answer(Func<string> read)
	{
		try
		{
			return read();
		}
		catch (Exception thrown) when (thrown is ArithmeticException or FormatException)
		{
			return Threw + thrown.GetType().Name + ": " + thrown.Message;
		}
	}

	const string Threw = "threw ";

	/// <summary>
	/// A tree as its records hold it, read through reflection: the two readings' records are types of
	/// two assemblies, and what is compared is what they hold, a list included.
	/// </summary>
	static string Shown(object? value)
	{
		switch (value)
		{
			case null:
				return "null";
			case string text:
				return "'" + text + "'";
			case IFormattable formattable:
				return value.GetType().Name + " " + formattable.ToString(null, CultureInfo.InvariantCulture);
			case IEnumerable items:
				return "[" + string.Join(", ", items.Cast<object?>().Select(Shown)) + "]";
		}

		var properties = value.GetType().GetProperties()
			.Where(static property => property.GetIndexParameters().Length == 0 && property.Name != "EqualityContract")
			.Select(property => property.Name + " = " + Shown(property.GetValue(value)));

		return value.GetType().Name + " { " + string.Join(", ", properties) + " }";
	}
}

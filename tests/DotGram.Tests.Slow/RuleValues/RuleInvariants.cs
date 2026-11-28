using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// What one cell of a run says, by whoever is asked: the oracle, the published version or this build. Compared as text:
/// the value as printed, or <c>REFUSED</c>, or <c>NO MATCH</c>; a text extent over bytes is printed as the string it is.
/// </summary>
delegate string? Observer(RuleShape shape, Mode mode, string input);

/// <summary>
/// The properties a grammar's value must have whatever the published version did: they need no other version to be
/// checked. Each takes an observer and returns the cells that break the property.
/// </summary>
static class RuleInvariants
{
	/// <summary>A violation: the two shapes, the mode, the input and what each gave.</summary>
	public sealed record Violation(string Invariant, string Shape, string Other, Mode Mode, string Input, string? A, string? B);

	/// <summary>A cell's value for comparison: bytes are text, a refusal is one word.</summary>
	public static string? Normalize(string? text)
	{
		return text?.Replace("bytes\"", "\"", StringComparison.Ordinal);
	}

	// An id is tier/type/structure/operands/names/reader/recursion/caller/template, behind "inlined/" for an extraction.
	static string WithSegment(RuleShape shape, int fromEnd, string value)
	{
		var parts = shape.Id.Split('/');

		parts[parts.Length - fromEnd] = value;

		return string.Join("/", parts);
	}

	/// <summary>The declaration that says the same type as a rule that takes one.</summary>
	static string? Declared(RuleType type)
	{
		return type.Name switch
		{
			":Word" or ":Bare" => "@string",
			":Sp" or ":Span"   => "@SourceSpan",
			":Bx"              => "@Box",
			":Lf"              => "@Pr",
			":Num"             => "@int",
			":O"               => "@object",
			_                  => null,
		};
	}

	static void Compare(List<Violation> into, string invariant, RuleShape a, RuleShape b, Observer observe, bool valuesOnly = false)
	{
		foreach (var route in RulePrinter.Print(a).Routes)
			foreach (var mode in Enum.GetValues<Mode>())
			{
				var x = Normalize(observe(a, mode, route.Input));
				var y = Normalize(observe(b, mode, route.Input));

				if (x == "NO MATCH" || y == "NO MATCH" || valuesOnly && (x == "REFUSED" || y == "REFUSED"))
					continue;

				if (x is not null && y is not null && x != y)
					into.Add(new Violation(invariant, a.Id, b.Id, mode, route.Input, x, y));
			}
	}

	/// <summary>(a) A rule that takes a type says the same as the declaration of that type, for every type.</summary>
	public static List<Violation> TakenIsDeclared(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			var parts = shape.Id.Split('/');
			var index = Array.IndexOf(parts, shape.Type.Name);

			if (shape.Type.How != TypeSaid.Taken || Declared(shape.Type) is not { } own || index < 0)
				continue;

			parts[index] = own;

			if (shapes.TryGetValue(string.Join("/", parts), out var twin))
				Compare(found, "taken-is-declared", shape, twin, observe);
		}

		return found;
	}

	/// <summary>
	/// (b) Extraction: a shape and the same shape with the bodies of its text rules written in place give the same value.
	/// An untyped rule is read through, so for it that holds for every caller; a typed one is joined by a string rule and
	/// bounded by a span rule, but an array or another type counts its calls. A refusal on either side is not a value, and
	/// is left out: written in place a piece may no longer be one of a type that is refused.
	/// </summary>
	public static List<Violation> ExtractionKeepsTheValue(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			if (shape.InlinedFrom is not { } original || !shapes.ContainsKey(original.Id))
				continue;

			var kind    = RuleOracle.Describe(original).Tested.Kind;
			var untyped = RuleShapes.Operands(original).Where(static h => h.Inline is not null).All(static h => h.Type is null);

			if (kind is Kind.C or Kind.S || untyped)
				Compare(found, "extraction-keeps-the-value", original, shape, observe, valuesOnly: true);
		}

		return found;
	}

	/// <summary>
	/// (c) In a string, span or array rule a name on a literal or on an operand, and a guard or switch that reads a name change nothing:
	/// the shape with them gives the value of the shape without.
	/// </summary>
	public static List<Violation> NamesAndReadersChangeNothing(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			if (shape.Tier is "inlined" or "random" or "fixed" || RuleOracle.Describe(shape).Tested.Kind is not (Kind.C or Kind.S or Kind.A))
				continue;

			// A name written inside a lookahead is refused, so that no twin without it is the same.
			if (RuleOracle.Describe(shape).Decision is { Source: Source.Error, ErrorId: "GRAM4006" })
				continue;

			var neutral = shape.Names is NameKind.None or NameKind.OnLiteral or NameKind.OnOperand;

			if (shape.Reader != ReaderKind.None && neutral)
			{
				var twinId = WithSegment(shape, 4, ReaderKind.None.ToString());

				if (shape.Tier == "deviation")
					twinId = "core" + twinId.Substring("deviation".Length);

				if (shapes.TryGetValue(twinId, out var twin))
					Compare(found, "reader-changes-nothing", shape, twin, observe);
			}

			if (shape.Names is NameKind.OnLiteral or NameKind.OnOperand
				&& shapes.TryGetValue(WithSegment(shape, 5, NameKind.None.ToString()), out var unnamed))
				Compare(found, "name-changes-nothing", shape, unnamed, observe);
		}

		return found;
	}

	/// <summary>(c) The value of a rule that matched its own text and no other is that text, or its bounds.</summary>
	public static List<Violation> PureRuleIsItsSlice(IReadOnlyDictionary<string, RuleShape> shapes)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
			foreach (var route in RulePrinter.Print(shape).Routes)
				foreach (var mode in Enum.GetValues<Mode>())
				{
					var expected = RuleOracle.Expected(shape, mode, route.Input);

					if (expected.Source != Source.Extent || !expected.IsValue)
						continue;

					var slice = expected.Row == "S"
						? Val.Span(0, route.Input.Length).Print
						: (mode == Mode.B ? Val.Bytes(route.Input) : Val.Str(route.Input)).Print;

					if (expected.Value != slice)
						found.Add(new Violation("pure-rule-is-its-slice", shape.Id, shape.Id, mode, route.Input, expected.Value, slice));
				}

		return found;
	}

	/// <summary>(d) The value's source is the same in the default, the span-capture and the byte modes; over bytes only GRAM4026 (a string that cannot be joined) and CS1503 (a capture of bytes into a constructor) differ.</summary>
	public static List<Violation> SourceIsTheSameInEveryMode(IReadOnlyDictionary<string, RuleShape> shapes)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
			foreach (var route in RulePrinter.Print(shape).Routes)
			{
				var d = RuleOracle.Expected(shape, Mode.D, route.Input);
				var s = RuleOracle.Expected(shape, Mode.S, route.Input);
				var b = RuleOracle.Expected(shape, Mode.B, route.Input);

				if (d.Source != s.Source || d.Value != s.Value)
					found.Add(new Violation("same-in-D-and-S", shape.Id, shape.Id, Mode.S, route.Input, d.ToString(), s.ToString()));

				var refused = b.ErrorId is "GRAM4026" or "CS1503";

				if (!refused && (d.Source != b.Source || Normalize(d.Value) != Normalize(b.Value)) && !d.IsUnknown && !b.IsUnknown)
					found.Add(new Violation("same-in-D-and-B", shape.Id, shape.Id, Mode.B, route.Input, d.ToString(), b.ToString()));
			}

		return found;
	}
}

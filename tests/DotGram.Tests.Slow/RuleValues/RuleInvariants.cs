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
		if (text is null)
			return null;

		if (text.StartsWith("bytes\"", StringComparison.Ordinal))
			return text.Substring(5);

		return text;
	}

	static string? Id(RuleShape shape, string newType)
	{
		var parts = shape.Id.Split('/');
		var index = Array.IndexOf(parts, shape.Type.Name);

		if (index < 0)
			return null;

		parts[index] = newType;

		return string.Join("/", parts);
	}

	// An id is tier/type/structure/operands/names/reader/recursion/caller/template, behind "inlined/" for an extraction.
	static string WithSegment(RuleShape shape, int fromEnd, string value)
	{
		var parts = shape.Id.Split('/');

		parts[parts.Length - fromEnd] = value;

		return string.Join("/", parts);
	}

	/// <summary>The type said as the declaration of the same type, for a type that takes a rule of another kind than text.</summary>
	static string? OwnOfTakenOther(RuleType type)
	{
		return type.Name switch
		{
			":Bx"  => "@Box",
			":Lf"  => "@Pr",
			":Num" => "@int",
			":O"   => "@object",
			_      => null,
		};
	}

	static string? OwnOfTakenText(RuleType type)
	{
		return type.Name switch
		{
			":Word" or ":Bare" => "@string",
			":Sp" or ":Span"   => "@SourceSpan",
			_                  => null,
		};
	}

	static IEnumerable<(string Input, Mode Mode)> Cells(RuleShape shape)
	{
		var routes = RulePrinter.Print(shape).Routes;

		foreach (var mode in Enum.GetValues<Mode>())
			foreach (var route in routes)
				yield return (route.Input, mode);
	}

	static void Compare(List<Violation> into, string invariant, RuleShape a, RuleShape b, Observer observe)
	{
		foreach (var (input, mode) in Cells(a))
		{
			var x = Normalize(observe(a, mode, input));
			var y = Normalize(observe(b, mode, input));

			if (x is not null && y is not null && x != y)
				into.Add(new Violation(invariant, a.Id, b.Id, mode, input, x, y));
		}
	}

	/// <summary>(a) A rule that takes the type of a rule of a type that is not text says the same as the declaration of that type.</summary>
	public static List<Violation> TakenIsDeclaredForOtherTypes(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
			if (shape.Type.How == TypeSaid.Taken && OwnOfTakenOther(shape.Type) is { } own && Id(shape, own) is { } id && shapes.TryGetValue(id, out var twin))
				Compare(found, "taken-is-declared", shape, twin, observe);

		return found;
	}

	/// <summary>(b) A rule that takes a text type and reads no operand says the same as the declaration of its type.</summary>
	public static List<Violation> TakenTextWithoutOperandIsDeclared(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe, bool takenTextPassesOn)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			if (shape.Type.How != TypeSaid.Taken || OwnOfTakenText(shape.Type) is not { } own || Id(shape, own) is not { } id || !shapes.TryGetValue(id, out var twin))
				continue;

			var grammar = RuleOracle.Describe(shape, takenTextPassesOn);

			if (grammar.Tested.Routes.Any(static r => r.Operands > 0))
				continue;

			Compare(found, "taken-text-without-operand-is-declared", shape, twin, observe);
		}

		return found;
	}

	/// <summary>
	/// (c) Extraction: a shape and the same shape with the bodies of its text rules written in place give the same value;
	/// where the caller is not an own text, only if what was written in place reads no operand that makes a value.
	/// </summary>
	public static List<Violation> ExtractionKeepsTheValue(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			if (shape.InlinedFrom is not { } original || !shapes.ContainsKey(original.Id))
				continue;

			var ownText = original.Type.How == TypeSaid.Own && (original.Type.Family is Family.Text or Family.Span);
			var exposes = RuleShapes.Operands(original).Any(static h => h.Inline is not null && h.Calls.Any(static x => HelperInfo.Of(x, true).Making));

			if (!ownText && exposes)
				continue;

			Compare(found, "extraction-keeps-the-value", original, shape, observe);
		}

		return found;
	}

	/// <summary>
	/// (d) A neutral name on something that is not an operand, and a guard or switch that reads a name, change nothing
	/// while no name fills a construction.
	/// </summary>
	public static List<Violation> NeutralNamesAndReadersChangeNothing(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe, bool takenTextPassesOn)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			if (shape.Tier == "inlined" || shape.Tier == "random" || shape.Tier == "fixed")
				continue;

			// A reader beside the same shape with no reader.
			if (shape.Reader != ReaderKind.None && shape.Names is NameKind.None or NameKind.OnLiteral or NameKind.OnGroup)
			{
				var twinId = WithSegment(shape, 4, ReaderKind.None.ToString());

				if (shape.Tier == "deviation")
					twinId = "core" + twinId.Substring("deviation".Length);

				if (shapes.TryGetValue(twinId, out var twin) && !WritesConstruction(shape, takenTextPassesOn))
					Compare(found, "reader-changes-nothing", shape, twin, observe);
			}

			// A name on a literal or on a group beside the same shape with no name.
			if (shape.Names is NameKind.OnLiteral or NameKind.OnGroup && shape.Type.Family != Family.Generated)
			{
				if (shapes.TryGetValue(WithSegment(shape, 5, NameKind.None.ToString()), out var twin) && !WritesConstruction(shape, takenTextPassesOn))
					Compare(found, "name-changes-nothing", shape, twin, observe);
			}
		}

		return found;
	}

	static bool WritesConstruction(RuleShape shape, bool takenTextPassesOn)
	{
		var grammar = RuleOracle.Describe(shape, takenTextPassesOn);

		return grammar.Rules.Any(grammar.WritesConstructionNames);
	}

	/// <summary>(e) The value's source is the same in the default, the span-capture and the byte modes.</summary>
	public static List<Violation> SourceIsTheSameInEveryMode(IReadOnlyDictionary<string, RuleShape> shapes, Observer observe)
	{
		var found = new List<Violation>();

		foreach (var shape in shapes.Values)
		{
			foreach (var route in RulePrinter.Print(shape).Routes)
			{
				var d = Normalize(observe(shape, Mode.D, route.Input));
				var s = Normalize(observe(shape, Mode.S, route.Input));
				var b = Normalize(observe(shape, Mode.B, route.Input));

				if (d is not null && s is not null && d != s)
					found.Add(new Violation("same-in-D-and-S", shape.Id, shape.Id, Mode.S, route.Input, d, s));

				// A refusal that only the byte mode has (a text route and a value route of a string) is its own rule.
				if (d is not null && b is not null && d != b && b != "REFUSED" && d != "REFUSED")
					found.Add(new Violation("same-in-D-and-B", shape.Id, shape.Id, Mode.B, route.Input, d, b));
			}
		}

		return found;
	}
}

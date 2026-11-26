using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.SlowShapes;

/// <summary>Which generator work a shape's refusal growth belongs to, and so what flips its check to a linear one.</summary>
enum GeneratorItem
{
	/// <summary>Resumable replay of a refused loop (the retry of a loop re-reads its prefix): the intermediate form's slice 5 and its C# emitter.</summary>
	ClassA,

	/// <summary>Sealing a group walks every way kept inside it, so an accepted nest costs the square of its depth: the deep-descent work's seal slice.</summary>
	Seal,

	/// <summary>
	/// Rule entries grow with the square of a name's length in a way the counts do not place: not yet known whether a loop's retry (the first item) or
	/// the grammar's own shape is at fault, to be settled by the counts of the intermediate form's slice.
	/// </summary>
	ToConfirm,

	/// <summary>
	/// A run of one class split between a name that ends in it and a second name after it, where what follows the second cannot begin with
	/// a character of the class: every character the second name gives back is refused at once, and each length of the first is tried with
	/// each length of the second. The name at such a call settles (it gives nothing back there), and a run read again from inside a run of
	/// the same class takes the end it already found.
	/// </summary>
	LexemeSplit,

	/// <summary>
	/// The same split where what follows the second name can begin with a character of the class, so that a character given back may be
	/// the one it needs: the square of the regular expression <c>[a-z]+[a-z]+x</c>, which settling cannot remove.
	/// </summary>
	LexemeSplitOverlap,
}

/// <summary>One input that grows with a size and is refused (or, where <see cref="Accepted"/>, read), of a grammar kept as it was.</summary>
/// <param name="Name">What the input is, for a report.</param>
/// <param name="Text">The input at a size, in characters of about.</param>
/// <param name="Accepted">Whether the grammar reads the input to its end (a contrast row), or refuses it.</param>
/// <param name="Today">
/// The growth exponent of the rule entries between the first and the last size reached, as measured when this was written, rounded to
/// a tenth. When <see cref="Linear"/> it is not read.
/// </param>
/// <param name="First">The smallest size of the ladder, in characters; it doubles from there for six sizes, so 2,048 reaches 64 KB and a shape that costs a second at 2,048 starts lower.</param>
/// <param name="ByTime">
/// Whether what is held is the time and not the count: for a cost that no counter sees, such as the walk a seal makes over every way kept inside
/// its group. The exponent is then read over the last three sizes, and is as loose as a clock is.
/// </param>
/// <param name="Item">The generator work this input waits for where it is not its shape's.</param>
/// <param name="Linear">
/// Whether the exponent is held to <see cref="SlowShapeLadder.LinearBound"/>. A shape is flipped to it by setting this, once the
/// generator work it belongs to has landed, and then <see cref="Today"/> is not needed.
/// </param>
sealed record SlowInput(string Name, Func<int, string> Text, bool Accepted, double Today, int First = 2048, bool Linear = false, bool ByTime = false, GeneratorItem? Item = null);

/// <summary>A grammar kept as it was written before a rewrite made it fast, and the inputs that make it slow.</summary>
/// <param name="Id">The namespace-safe name, which is also the file under <c>Sources</c> and the type's namespace.</param>
/// <param name="Class">The partial class the grammar is published on.</param>
/// <param name="Entry">The generated method that reads a string.</param>
/// <param name="Item">The generator work the slow inputs belong to.</param>
/// <param name="Inputs">The inputs.</param>
sealed record SlowShape(string Id, string Class, string Entry, GeneratorItem Item, IReadOnlyList<SlowInput> Inputs)
{
	public override string ToString()
	{
		return Id;
	}
}

/// <summary>The shapes, each over the grammar as it was, taken whole from the history of the examples.</summary>
/// <remarks>
/// A file under <c>Sources</c> is a shipped example as it was at the commit named beside it, not edited: the generator runs over it here
/// in a namespace of its own, with the entry counters written, so that what is held is a count of the readings the parser makes, which
/// no machine changes, beside the milliseconds a ladder prints.
/// </remarks>
static class SlowShapes
{
	static string Repeated(string text, int length)
	{
		return string.Concat(Enumerable.Repeat(text, Math.Max(1, length / text.Length)));
	}

	/// <summary>Elements nested as deep as a text of this length holds, closed.</summary>
	static string Nest(int length)
	{
		var depth = Math.Max(1, length / 7);

		return string.Concat(Enumerable.Repeat("<a>", depth)) + string.Concat(Enumerable.Repeat("</a>", depth));
	}

	static string Run(char c, int length)
	{
		return new string(c, length);
	}

	public static IReadOnlyList<SlowShape> All { get; } =
	[
		// The calculator before its publication was put in braces (the parent of the commit that added `Whole`).
		new("Calculator", "Calculator", "TryEvaluateInt", GeneratorItem.ClassA,
		[
			new("a sum, then a stray ')'", n => Repeated("1 + ", n) + "1 )", false, 2.0),
			new("a sum, then an operator with no operand", n => Repeated("1 + ", n) + "1 +", false, 2.0),
			new("a sum in parentheses, never closed", n => "(" + Repeated("1 + ", n) + "1", false, 2.0),
		]),

		// YAML before its runs and its lines were made atomic (the parent of the first rewrite). A run of one unit, then something no unit can be.
		new("Yaml", "YamlLite", "TryParseDoc", GeneratorItem.ClassA,
		[
			new("lines, then a character no line starts with", n => Repeated("a: b\r\n", n) + "!", false, 2.0),
			new("lines with blanks round the value, then the same", n => Repeated("a:  b  c  \r\n", n) + "!", false, 2.0),
			new("indented keys with no value, then the same", n => Repeated("  a:\r\n", n) + "!", false, 2.0),
			new("blank lines, then the same", n => Repeated("\r\n", n) + "!", false, 1.0, Linear: true),
			new("indented comments, then the same", n => Repeated("  # c\r\n", n) + "!", false, 1.0, Linear: true),
			new("a line ended by LF CR, then the same", n => Repeated("a: b\n\r", n) + "!", false, 2.0),
			new("a nest of keys, then the same", n => Repeated("a:\r\n  b: c\r\n", n) + "!", false, 2.0, First: 512),
			new("lines, accepted", n => Repeated("a: b\r\n", n), true, 1.0, Linear: true),
		]),

		// XML before its spaces, values and attributes were made atomic; the children and the name as they were.
		new("Xml", "XmlParser", "TryParseXml", GeneratorItem.ClassA,
		[
			new("attributes, then a character that is none", n => "<a" + Repeated(" k=\"v\"", n) + " !", false, 2.0),
			new("a long name, then a character that is none", n => "<" + Run('a', n) + "!", false, 1.0, Linear: true, Item: GeneratorItem.LexemeSplit),
			new("children with attributes, the element never closed", n => "<a>" + Repeated("<b k=\"v\"/>", n), false, 2.0, First: 512),
			new("children with attributes, closed by another name", n => "<a>" + Repeated("<b k=\"v\"/>", n) + "</b>", false, 2.0, First: 512),
			new("elements nested to a depth, accepted", n => Nest(n), true, 1.0, Linear: true),
		]),

		// XML with braces round the attributes and round the children, as it was for the hour in which it was rewritten and rolled back:
		// what the braces on the children make linear (an element refused at its closing tag), and a nest, which they made quadratic
		// until a seal skipped the ways sealed before it (held by time, the fewest of settled readings a size).
		new("XmlBracedChildren", "XmlParser", "TryParseXml", GeneratorItem.Seal,
		[
			new("children with attributes, the element never closed", n => "<a>" + Repeated("<b k=\"v\"/>", n), false, 1.0, Linear: true),
			new("children with attributes, closed by another name", n => "<a>" + Repeated("<b k=\"v\"/>", n) + "</b>", false, 1.0, Linear: true),
			new("elements nested to a depth, accepted", n => Nest(n), true, 1.0, First: 8192, Linear: true, ByTime: true),
			new("a long name, then a character that is none", n => "<" + Run('a', n) + "!", false, 1.0, Linear: true, Item: GeneratorItem.LexemeSplit),
		]),

		// Filter and Gram as they ship.
		new("Filter", "Filter", "TryParseFilter", GeneratorItem.ClassA,
		[
			new("a chain of ORs, then a character that is none", n => Repeated("a > 1 OR ", n) + "!", false, 2.0),
			new("a chain of ANDs, then a character that is none", n => Repeated("a > 1 AND ", n) + "!", false, 2.0),
			new("a list of values, never closed", n => "a IN (" + Repeated("'x', ", n) + "!", false, 2.0),
		]),

		// Two names over one run, before a character of the same class: written for the case, not taken from an example.
		new("OverlappingSplit", "OverlappingSplit", "TryParseSplit", GeneratorItem.LexemeSplitOverlap,
		[
			new("a run of the class, then a character that is none", n => Run('x', n) + "!", false, 2.0, First: 256),
		]),

		new("Gram", "GramGrammar", "TryParseFile", GeneratorItem.ClassA,
		[
			new("rules, then a character that is none", n => Repeated("A = 'a'\n", n) + "!", false, 2.0),
			new("a long choice, then a character that is none", n => "A = " + Repeated("'a' | ", n) + "!", false, 2.0),
			new("a long sequence, then a character that is none", n => "A = " + Repeated("'a' & ", n) + "!", false, 2.0, First: 1024),
		]),
	];
}

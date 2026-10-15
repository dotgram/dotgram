using System;
using System.Diagnostics;
using System.Text;

using DotGram.Examples.Expressions;
using DotGram.Examples.Formats;
using DotGram.Examples.Languages;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The examples refuse what they cannot read in the time its length takes (D57).
/// </summary>
/// <remarks>
/// <para>
/// An example is copied, so a shape in one reaches a consumer as surely as a shipped package
/// does. Two of them repeated something that could read more than one character inside another
/// repetition — a list of bullets inside a document, a run of text inside an element's content —
/// which reads a text that fails later every way it can be cut: sixteen bullets took twelve
/// seconds, and twenty-four characters of XML text thirty-five.
/// </para>
/// <para>
/// Both are atomic now, and this holds them there. The input is the one that drove each, which
/// is not the same as the one a reader would guess: paragraphs, an unclosed code fence, nested
/// elements and unterminated attribute lists were all flat, and only these two were not.
/// </para>
/// </remarks>
public sealed class ExampleBlowUpTests
{
	[Fact]
	public void A_document_of_bullets_that_never_ends_is_refused_at_once()
	{
		// The last line without its newline is what the document cannot finish on.
		Refused("markdown", () => MarkdownParser.Read(Repeated("- a\n", 64) + "- a"));
	}

	[Fact]
	public void An_element_whose_text_never_closes_is_refused_at_once()
	{
		Refused("xml", () => XmlParser.Read("<a>" + new string('t', 64)));
	}

	/// <remarks>
	/// A number could end before the space after it or after it, the next seam taking the space
	/// either way, so a refused text was read two ways per number: twenty took over ten seconds.
	/// </remarks>
	[Fact]
	public void A_filter_of_comparisons_that_never_closes_is_refused_at_once()
	{
		Refused("filter", () => Filter.Read(Joined("a = 1", " AND ", 64) + " )"));
	}

	/// <remarks>The same shape in the calculator's decimal number, `Point`.</remarks>
	[Fact]
	public void A_sum_of_decimals_that_never_ends_is_refused_at_once()
	{
		Refused("calculator", () => Calculator.EvaluateDecimal(Joined("1", " + ", 64) + " ="));
	}

	/// <remarks>
	/// A quotient read as the right side of a `+` at strength 3 could give its `/0` back and the
	/// loop take it, the same text grouped the other way: two readings a quotient, and twenty
	/// of them took seconds. The loop takes only what the operand before it could not have
	/// taken now (§4.3.1), and what is left is a way back per term, read again from the start:
	/// quadratic, and five hundred and twelve terms well inside the bound.
	/// </remarks>
	[Fact]
	public void A_sum_of_quotients_that_never_ends_is_refused_at_once()
	{
		Refused("calculator", () => Calculator.EvaluateInt(Joined("1/0", "+", 128) + "="), 128);
	}

	/// <summary>
	/// Runs it, expects it not to finish, and holds how long it took.
	/// </summary>
	/// <remarks>
	/// Five seconds: far above what the reading costs — both answer in under a millisecond —
	/// and far below what the shape cost, which doubles with every further character. A loaded
	/// machine cannot fail it and the shape cannot pass it.
	/// </remarks>
	static void Refused(string what, Action read, int count = 64)
	{
		var watch = Stopwatch.StartNew();

		Assert.ThrowsAny<Exception>(read);

		Assert.True(
			watch.Elapsed.TotalSeconds < 5,
			$"The {what} example took {watch.Elapsed.TotalSeconds:F1} s to refuse {count} of them.");
	}

	static string Joined(string piece, string separator, int times)
	{
		var built = new StringBuilder();

		for (var at = 0; at < times; at++)
			built.Append(at > 0 ? separator : "").Append(piece);

		return built.ToString();
	}

	static string Repeated(string piece, int times)
	{
		var built = new StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}

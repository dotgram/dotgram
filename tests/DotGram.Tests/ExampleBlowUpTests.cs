using System;
using System.Diagnostics;
using System.Text;

using DotGram.Examples.Formats;

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

	/// <summary>
	/// Runs it, expects it not to finish, and holds how long it took.
	/// </summary>
	/// <remarks>
	/// Five seconds: far above what the reading costs — both answer in under a millisecond —
	/// and far below what the shape cost, which doubles with every further character. A loaded
	/// machine cannot fail it and the shape cannot pass it.
	/// </remarks>
	static void Refused(string what, Action read)
	{
		var watch = Stopwatch.StartNew();

		Assert.ThrowsAny<Exception>(read);

		Assert.True(
			watch.Elapsed.TotalSeconds < 5,
			$"The {what} example took {watch.Elapsed.TotalSeconds:F1} s to refuse 64 of them.");
	}

	static string Repeated(string piece, int times)
	{
		var built = new StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}

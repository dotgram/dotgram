using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// What both readings of the Link header field answer — accepted with which values, or refused where
/// and with which message — held against what they answered before.
/// </summary>
/// <remarks>
/// <para>
/// <c>LinkValue</c> is two alternatives that differ only in which reading they belong to and in how
/// they hold the relation types to the ABNF. Where in an alternative the reading is asked is a question
/// of cost alone: asked last, every link in the default reading was read and built twice, once by the
/// strict alternative only to be refused on the reading, and once more by its own. Asked first, the
/// other reading's alternative stops before it reads anything. Nothing it answers may move with it,
/// and this is what says so: the texts below and what both readings said about each of them, written
/// down before the reading was asked first.
/// </para>
/// <para>
/// The texts are links that read and links that are refused for each reason §3 gives one, and from
/// each, every third prefix and the text with every fifth character left out, so that refusals in the
/// middle of a target, a parameter, a quoted string and between two links are all held.
/// </para>
/// </remarks>
public sealed class Rfc8288ReadingTests
{
	[Fact]
	public void Every_answer_of_both_readings_is_the_one_it_was()
	{
		var answers = new StringBuilder();

		foreach (var text in Texts())
		{
			var practice = Answer(Rfc8288.TryParseLinks(text));
			var strict   = Answer(Rfc8288.TryParseStrictLinks(text));

			answers
				.Append(Escaped(text))
				.Append(" | ").Append(practice)
				.Append(" | ").Append(strict == practice ? "same" : strict)
				.Append('\n');
		}

		var actual = answers.ToString();

		if (!File.Exists(Expected))
		{
			File.WriteAllText(Expected, actual, Utf8);

			Assert.Fail($"No record of the answers; wrote one to {Expected}. Read it, and commit it if it is right.");
		}

		if (File.ReadAllText(Expected).Replace("\r\n", "\n") == actual)
			return;

		File.WriteAllText(Expected + ".actual", actual, Utf8);

		Assert.Fail($"An answer is not the one it was; what it is now is in {Expected}.actual.");
	}

	/// <summary>The two readings part only on a registered relation type that is not lowercase.</summary>
	[Fact]
	public void The_readings_part_where_they_did()
	{
		Assert.True(WebLink.TryParseField("<https://example.com/1>; rel=Next", out _));
		Assert.False(WebLink.TryParseStrictField("<https://example.com/1>; rel=Next", out _));
		Assert.True(WebLink.TryParseStrictField("<https://example.com/1>; rel=next", out _));
	}

	static readonly string[] Shapes =
	[
		"<http://example.com/TheBook/chapter2>; rel=\"previous\"; title=\"previous chapter\"",
		"</>; rel=\"http://example.net/foo\"",
		"</terms>; rel=\"copyright\"; anchor=\"#foo\"",
		"</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel, </TheBook/chapter4>; rel=\"next\"; title*=UTF-8'de'n%c3%a4chstes%20Kapitel",
		"<http://example.org/>; rel=\"start http://example.net/relation/other\"",
		"<https://example.org/>; rel=\"start\", <https://example.org/index>; rel=\"index\"",
		"<https://example.com/1>; rel=Next",
		"<https://example.com/1>; rel=\"Next Prev\"; rev=UP",
		", ,<a>;rel=next ,  , <b> ; rel = prev ;x, ",
		"<a>; type=\"text/html\"; media=screen; hreflang=en-US; hreflang=de",
		"<a>; title=\"a \\\"quoted\\\" \\\\ value\"; flag",
		"<a>; rel=next; rel=prev",
		"<a>; anchor=\"#x\"; anchor=y",
		"<a>; title*=bad",
		"<a>; title*=UTF-8''%e2%82%ac; title*=iso-8859-1'en'%A3%20rates",
		"<a>; type=\"bad type\"",
		"<a>; type=text/html",
		"<a>; hreflang=\"!!\"",
		"<a>; media=\"(min-width: 10px)\"",
		"<a b>; rel=next",
		"<%zz>; rel=next",
		"https://example.com/>; rel=next",
		"<a>; rel=next x",
		"<a>; a\u0001",
		"<unclosed",
		"",
		",",
		" ",
		"<a>,<b>",
		"<a>;;rel=next",
		"<a>; =x",
		"<a>; rel=\"unclosed",
		"<a>; rel=",
		"<a> rel=next",
		"<a>; rel=next,, <b>; rel=PREV",
		"<a>; REL=Next",
		"<a>; rel=\"next PREV\"; rev=\"Up\"",
		"<a>; rel=http://x/y",
		"<a>; rel=\"\"",
		"<a>; rel=\"next  prev\"",
		"<a>; rel=\" next\"",
		"<a>\t;\trel=next",
	];

	static IEnumerable<string> Texts()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var shape in Shapes)
		{
			if (seen.Add(shape))
				yield return shape;

			for (var length = 1; length < shape.Length; length += 3)
				if (seen.Add(shape.Substring(0, length)))
					yield return shape.Substring(0, length);

			for (var at = 0; at < shape.Length; at += 5)
				if (seen.Add(shape.Remove(at, 1)))
					yield return shape.Remove(at, 1);
		}
	}

	/// <summary>Accepted with every value read, or how and where it was refused.</summary>
	static string Answer(Rfc8288.Match<WebLink[]> match)
	{
		return match.IsSuccess
			? "accepted " + string.Join(", ", match.Value.Select(Shown))
			: $"{match.Outcome} at {match.Position}: {match.Error}";
	}

	static string Shown(WebLink link)
	{
		var text = new StringBuilder("<").Append(link.Target).Append('>');

		foreach (var parameter in link.Parameters)
		{
			text.Append("; ").Append(parameter.Name).Append('=').Append(Escaped(parameter.Value));

			if (parameter.Extended is { } extended)
				text.Append(" {").Append(extended.Charset).Append('\'').Append(extended.Language).Append('\'')
					.Append(extended.Value is null ? "null" : Escaped(extended.Value)).Append('}');
		}

		return text.ToString();
	}

	static string Escaped(string input)
	{
		var text = new StringBuilder("\"");

		foreach (var c in input)
		{
			if (c is '\\' or '"')
				text.Append('\\').Append(c);
			else if (c < ' ' || c > '~')
				text.Append("\\u").Append(((int)c).ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
			else
				text.Append(c);
		}

		return text.Append('"').ToString();
	}

	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	static string Expected => Path.Combine(Path.GetDirectoryName(ThisFile)!, "Rfc8288ReadingTests.txt");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}

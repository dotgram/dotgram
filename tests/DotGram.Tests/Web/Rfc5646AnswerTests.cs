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
/// What a language tag answers — accepted with which subtags, or refused where and with which message —
/// held against what it answered before.
/// </summary>
/// <remarks>
/// <para>
/// The repetitions of a tag (variants, extensions, the subtags of each, private use) read to their end:
/// nothing that may follow one begins the way one does, so a shorter reading of any of them leads
/// nowhere. Given back, they made a tag cut after many extensions take the square of its length to be
/// refused. Committed, nothing a tag answers may move, and this is what says so: the texts below and what
/// was said about each of them, written down before the repetitions were committed.
/// </para>
/// <para>
/// The texts are tags that read and tags that are refused for each reason §2.1 gives one, and from each,
/// every third prefix and the text with every fifth character left out.
/// </para>
/// </remarks>
public sealed class Rfc5646AnswerTests
{
	[Fact]
	public void Every_answer_is_the_one_it_was()
	{
		var answers = new StringBuilder();

		foreach (var text in Texts())
			answers.Append(Escaped(text)).Append(" | ").Append(Answer(Rfc5646.TryParseTag(text))).Append('\n');

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

	static readonly string[] Shapes =
	[
		"en",
		"en-US",
		"zh-Hant-TW",
		"sr-Latn-RS",
		"de-CH-1901",
		"sl-rozaj-biske",
		"hy-Latn-IT-arevela",
		"zh-yue-HK",
		"zh-cmn-Hans-CN",
		"en-a-bbb-x-a-ccc",
		"en-a-abc-b-def-ghij-x-private",
		"en-Latn-US-valencia-a-abc-12-u-ca-gregory-co-phonebk",
		"de-Latn-DE-1996-u-co-phonebk-x-priv1-priv2",
		"x-whatever-it-is",
		"i-klingon",
		"zh-min-nan",
		"zh-min-xyz",
		"en-GB-oed",
		"art-lojban",
		"qaa-Qaaa-QM-x-southern",
		"en-a-abc-a-abc-a-abc-a",
		"en-a-abc-a",
		"en-a",
		"en-a-",
		"en-x",
		"en-x-",
		"en-US-",
		"en--US",
		"en-u-ca-gregory-x",
		"en-12345-abcde-abc",
		"en-Latn-US-abcde-abcde-abcdefghi",
		"en-a-abcdefghi",
		"en-x-abcdefghi",
		"enabcdefghi",
		"e",
		"",
		"-",
		"1234",
		"en_US",
		"en-US-u-ca-gregory-u-nu-latn",
		"abcd-Latn",
		"abcdefgh-US",
		"en-abc-def-ghi-jkl",
		"en-x-a-b-c-1-2-3",
		"EN-latn-us-A-ABC",
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

	/// <summary>Accepted with every subtag read, or how and where it was refused.</summary>
	static string Answer(Rfc5646.Match<LanguageTag> match)
	{
		if (!match.IsSuccess)
			return $"{match.Outcome} at {match.Position}: {match.Error}";

		var tag = match.Value;

		return "accepted " + string.Join(" ",
			tag.Language ?? "-",
			"[" + string.Join(",", tag.ExtendedLanguages) + "]",
			tag.Script ?? "-",
			tag.Region ?? "-",
			"[" + string.Join(",", tag.Variants) + "]",
			"[" + string.Join(",", tag.Extensions.Select(static one => one.Singleton + ":" + string.Join(",", one.Subtags))) + "]",
			"[" + string.Join(",", tag.PrivateUse) + "]",
			tag.Grandfathered ?? "-");
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

	static string Expected => Path.Combine(Path.GetDirectoryName(ThisFile)!, "Rfc5646AnswerTests.txt");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}

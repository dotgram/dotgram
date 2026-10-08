using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The shipped RFC 5322 reading, carried immediately, answers as its tape copy does
/// (<see cref="Rfc5322Tape"/>): the same verdict, value, message and position for every
/// publication over is_email's addresses and the lists below, every prefix of each and every
/// text one character shorter.
/// </summary>
/// <remarks>
/// The grammar's constructions build records and strings and touch nothing else, so the
/// immediate carrier, which runs them where they are read rather than after the parse has
/// accepted, may differ from the tape only by a construction that throws on a reading the tape
/// never builds. None does: this is what says so, and keeps saying so as the grammar changes.
/// </remarks>
public sealed class Rfc5322TapeTests
{
	static readonly string[] Publications =
	[
		"ParseAddrSpec", "ParseMailbox", "ParseMailboxList", "ParseAddressList",
		"ParseStrictAddrSpec", "ParseStrictMailbox", "ParseStrictMailboxList", "ParseStrictAddressList",
	];

	static readonly string[] Lists =
	[
		"Joe Q. Public <john.q.public@example.com>",
		"Who? <one@y.test>",
		"\"Giant; \\\"Big\\\" Box\" <sysservices@example.net>",
		"A Group:Ed Jones <c@a.test>,joe@where.test;",
		"Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>",
		"a@b, c@d, e@f",
		"a@b,,c@d",
		"Undisclosed recipients:;",
		"<@a.test,@b.test:joe@c.test>",
		"a <b>",
		"Group:a@b",
	];

	[Fact]
	public void The_shipped_reading_answers_as_the_tape_does()
	{
		var texts = new HashSet<string>(StringComparer.Ordinal);

		foreach (var text in Corpus().Concat(Lists))
		{
			for (var length = 0; length <= text.Length; length++)
				texts.Add(text.Substring(0, length));

			for (var at = 0; at < text.Length; at++)
				texts.Add(text.Remove(at, 1));
		}

		var differ = new List<string>();

		foreach (var text in texts)
			foreach (var publication in Publications)
			{
				var tape    = Rfc5322Tape.Try(publication, text);
				var shipped = Rfc5322Tape.Shipped(publication, text);

				if (tape != shipped)
					differ.Add($"{publication} '{text}': tape {tape}, shipped {shipped}");
			}

		Assert.True(texts.Count > 5_000, $"{texts.Count} texts: the corpus was not read.");
		Assert.True(differ.Count == 0, $"{differ.Count} of {texts.Count * Publications.Length} differ:\n" + string.Join("\n", differ.Take(20)));
	}

	/// <summary>is_email's addresses, the control pictures in them taken for what they picture.</summary>
	static IEnumerable<string> Corpus()
	{
		foreach (var test in XDocument.Load(Path.Combine(Here(), "..", "DotGram.Tests", "Web", "IsEmail", "tests.xml")).Root!.Elements("test"))
		{
			var address = new StringBuilder();

			foreach (var character in (string?)test.Element("address") ?? "")
				address.Append(character switch
				{
					>= '␀' and <= '␟' => (char)(character - '␀'),
					'␡'                    => '\u007F',
					_                           => character,
				});

			yield return address.ToString();
		}
	}

	static string Here([CallerFilePath] string path = "")
	{
		return Path.GetDirectoryName(path)!;
	}
}

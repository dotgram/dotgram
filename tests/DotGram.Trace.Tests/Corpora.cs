using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DotGram.Trace.Tests;

/// <summary>What a publication answered, in the shape every grammar here can be read into.</summary>
/// <param name="Thrown">
/// Whether a construction the grammar ran threw rather than the grammar refusing: the expression
/// language's refuse a name nothing declares that way, after the text has been read.
/// </param>
readonly record struct Answer(bool Ok, string? Error, long Position, bool Thrown = false);

/// <summary>What <c>GramWhy</c> said about a refusal, read off whichever host's it was.</summary>
sealed record Explained(
	Answer Answer, bool Refused, string? Message, long Position, string Cause, string[][] Stacks, int Elements);

/// <summary>
/// One publication of a shipped grammar three ways — as the library ships it, as a trace build, and
/// as a trace build that calls its forwarding rules as written — and the refused inputs it is held to.
/// </summary>
sealed record Target(
	string Name,
	Func<string, Answer> Plain,
	Func<string, Explained> Traced,
	Func<string, Explained> Unfolded,
	Func<string, (Answer Answer, Targets.Tally Tally)> Counted,
	IReadOnlyList<string> Seeds)
{
	List<string>? _refused;

	/// <summary>
	/// The inputs the shipped parser refuses: every refused line of a record of refusals where the
	/// grammar has one, and otherwise the accepted seeds cut short, missing a character, or with one
	/// too many — chosen by a fixed seed, so that every run holds the same rows.
	/// </summary>
	public IReadOnlyList<string> Refused => _refused ??= Made();

	/// <summary>Where a record of refusals is the corpus, its inputs; then the seeds are not mutated.</summary>
	public IReadOnlyList<string>? Recorded { get; init; }

	public int Cap { get; init; } = 1000;

	public int PerSeed { get; init; } = 12;

	List<string> Made()
	{
		if (Recorded is not null)
			return [.. Recorded.Where(one => !Plain(one).Ok)];

		var random = new Random(42);
		var made   = new HashSet<string>(StringComparer.Ordinal);
		var list   = new List<string>();

		void Try(string text)
		{
			if (list.Count < Cap && made.Add(text) && !Plain(text).Ok)
				list.Add(text);
		}

		foreach (var seed in Seeds)
		{
			for (var i = 0; i < PerSeed; i++)
			{
				var cut = random.Next(1, Math.Max(2, seed.Length));

				Try(seed.Substring(0, Math.Min(cut, seed.Length - 1 < 1 ? 0 : cut)));

				var gone = random.Next(seed.Length);

				Try(seed.Remove(gone, 1));

				var junk = "@)(;,.'\"x1 "[random.Next(11)];

				Try(seed.Insert(random.Next(seed.Length + 1), junk.ToString()));
			}
		}

		return list;
	}

	public override string ToString()
	{
		return Name;
	}
}

/// <summary>The texts the corpora are made from, found where the repository keeps them.</summary>
static class Corpora
{
	public static string Repository([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
	}

	/// <summary>Every input of the expression language's record of refusals, accepted and refused.</summary>
	public static List<string> Expressions()
	{
		var all = new List<string>();

		foreach (var line in File.ReadLines(Path.Combine(Repository(), "tests", "DotGram.Tests", "ExpressionLanguage", "ExpressionRefusalTests.txt")))
		{
			var text = line.TrimStart('\uFEFF');

			if (!text.StartsWith('"'))
				continue;

			var built = new StringBuilder();

			for (var at = 1; at < text.Length && text[at] != '"'; at++)
			{
				if (text[at] != '\\' || at + 1 >= text.Length)
				{
					built.Append(text[at]);

					continue;
				}

				at++;

				switch (text[at])
				{
					case 'n': built.Append('\n'); break;
					case 'r': built.Append('\r'); break;
					case 't': built.Append('\t'); break;
					case '0': built.Append('\0'); break;
					case 'u':
						built.Append((char)Convert.ToInt32(text.Substring(at + 1, 4), 16));
						at += 4;
						break;
					default: built.Append(text[at]); break;
				}
			}

			all.Add(built.ToString());
		}

		return all;
	}

	/// <summary>
	/// The batches of SqlScriptDOM's test scripts the shipped T-SQL parser accepts, a spread of six
	/// hundred or so rather than the first files.
	/// </summary>
	public static List<string> Batches(Func<string, bool> accepted)
	{
		var seeds = new List<string>();
		var go    = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
		var files = Directory.GetFiles(Path.Combine(Repository(), "tests", "Corpus", "ScriptDom", "TestScripts"), "*.sql")
			.OrderBy(static one => one, StringComparer.Ordinal);

		foreach (var file in files)
			foreach (var chunk in go.Split(File.ReadAllText(file)))
			{
				var text = chunk.Trim();

				if (text.Length > 0 && text.Length <= 4000 && accepted(text))
					seeds.Add(text);
			}

		var step = Math.Max(1, seeds.Count / 600);

		return [.. seeds.Where((_, at) => at % step == 0)];
	}

	public static readonly string[] Json =
	[
		"{\"a\": [1, 2.5e3, true, null, {\"b\": \"x\\u0041y\"}], \"c\": {}}",
		"[[], {}, \"s\", -0.1, 0, 1E+2]",
		" { \"name\" : \"value\" , \"list\" : [ 1 , 2 , 3 ] } ",
		"\"just a string with \\\"escapes\\\" and \\n\"",
	];

	public static readonly string[] Accepts =
	[
		"text/html, application/xhtml+xml, application/xml;q=0.9, */*;q=0.8",
		"text/*;q=0.3, text/html;q=0.7, text/html;level=1, text/html;level=2;q=0.4, */*;q=0.5",
		"application/json",
		"*/*",
		" , audio/*; q=0.2, audio/basic",
	];

	public static readonly string[] Cookies =
	[
		"SID=31d4d96e407aad42; lang=en-US",
		"a=b",
		"name=\"quoted value\"; x=1; empty=",
		"  theme=dark; session_id=abc123; tracking=no  ",
	];

	public static readonly string[] Values =
	[
		"-12.5E3",
		"N'it''s'",
		"U&'\\0041bc' UESCAPE '!'",
		"DATE '2024-01-31'",
		"INTERVAL -'1:30' HOUR TO MINUTE",
		"X'0A1B'",
		"NULL",
	];

	public static readonly string[] Uris =
	[
		"https://user@example.com:8080/a/b%20c?q=1&r=2#frag",
		"urn:isbn:0451450523",
		"mailto:someone@example.org",
		"http://[2001:db8::1]:80/path/to?x",
		"file:///etc/hosts",
	];
}

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

using DotGram.Web;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// The hand-written hosts: the JSON and the timestamp grammars that DotGram.Web ships, lowered as
/// they stand, printed in TypeScript and in C, and given a host written by hand in each — a tree
/// for JSON, the checks of the calendar and the clock for timestamps. Held to the shipped C#
/// parsers on a corpus: what they accept, what is built, and (through the evaluator) where and
/// why a text is refused.
/// </summary>
public sealed class HostTests
{
	static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	static string Corpus(string name)
	{
		return Path.Combine(Graphs.Repository, "tests", "DotGram.Tests", "Web", name);
	}

	/// <summary>
	/// nst/JSONTestSuite's parsing cases, as the repository holds them, that are text at all: a
	/// file that is not UTF-8 is refused before any parser sees it.
	/// </summary>
	internal static List<(string Name, string Text)> JsonCorpus()
	{
		var corpus = new List<(string, string)>();

		foreach (var file in Directory.GetFiles(Corpus("JsonTestSuite"), "*.json").Order(StringComparer.Ordinal))
		{
			// The two that nest tens of thousands deep are DepthTests': what reads them is a question of stack.
			if (Path.GetFileName(file) is "n_structure_100000_opening_arrays.json" or "n_structure_open_array_object.json")
				continue;

			try
			{
				corpus.Add((Path.GetFileName(file), Strict.GetString(File.ReadAllBytes(file))));
			}
			catch (DecoderFallbackException)
			{
			}
		}

		corpus.Add(("escapes", "{\"a\\u00e9\\\"b\\\\\": [\"\\ud83d\\ude00\", \"\\ud800\", \"\\n\\t\\/\", \"é\u20ac\"], \"\": -0.5e+3, \"a\": 1, \"a\": 2}"));
		corpus.Add(("nested", " [ [ ] , { } , [ { \"k\" : [ null , true , false ] } ] ] "));

		return corpus;
	}

	static IrProgram Json => Reading.Program(Graphs.WebGrammar("Rfc8259.cs"));

	static List<SliceAnswer> JsonExpected(List<(string Name, string Text)> corpus)
	{
		var expected = new List<SliceAnswer>();

		foreach (var (name, text) in corpus)
		{
			var read      = JsonValue.TryParse(text, out var value);
			var evaluated = Slices.Evaluated(Json, "ParseJson", text, null);

			Assert.True(read == evaluated.Head.StartsWith('A'), name + ": the evaluator and the shipped parser disagree");

			if (name[1] == '_')
				Assert.True(name[0] == 'i' || read == (name[0] == 'y'), name + " is answered against the suite");

			expected.Add(new SliceAnswer(
				read ? "A " + text.Length.ToString(CultureInfo.InvariantCulture) + " " + value : evaluated.Head, []));
		}

		return expected;
	}

	static void Same(IReadOnlyList<string> names, List<SliceAnswer> expected, List<SliceAnswer> actual)
	{
		Assert.Equal(expected.Count, actual.Count);

		for (var i = 0; i < expected.Count; i++)
			Assert.True(expected[i].Head == actual[i].Head, $"{names[i]}: expected {expected[i].Head}, read {actual[i].Head}");
	}

	/// <summary>The ten host functions of the JSON grammar: its three constants, its five node kinds and the two that join a sequence.</summary>
	[Fact]
	public void TheJsonGrammarAsksItsHostForTenFunctions()
	{
		Assert.Equal(10, Json.Fragments.Count);
		Assert.DoesNotContain(Json.Fragments, static fragment => fragment.IsGuard);
		Assert.DoesNotContain(Json.Functions, static function => function.Decides || !function.Complete);
	}

	[Theory]
	[InlineData(SliceFrames.None)]
	[InlineData(SliceFrames.Recursive)]
	[InlineData(SliceFrames.All)]
	public void TypeScriptBuildsTheJsonTheShippedParserBuilds(SliceFrames frames)
	{
		var corpus = JsonCorpus();

		Assert.True(corpus.Count > 250);

		Same([.. corpus.Select(static one => one.Name)], JsonExpected(corpus),
			Slices.TypeScript("JsonHost", Json, "ParseJson", [.. corpus.Select(static one => one.Text)], host: "json.ts", frames: frames));
	}

	[Fact]
	public void CBuildsTheJsonTheShippedParserBuilds()
	{
		var corpus = JsonCorpus();

		Same([.. corpus.Select(static one => one.Name)], JsonExpected(corpus),
			Slices.InC("JsonHost", Json, "ParseJson", [.. corpus.Select(static one => one.Text)], host: "json.c"));
	}

	static IrProgram Timestamps => Reading.Program(Graphs.WebGrammar("Rfc3339.cs"));

	/// <summary>
	/// The date, time and date-time strings of JSON Schema's format tests, which are RFC 3339's,
	/// each with the publication that reads it.
	/// </summary>
	static List<(string Entry, string Text)> TimestampCorpus()
	{
		var corpus = new List<(string, string)>();

		foreach (var (file, entry) in new[] { ("date-time.json", "ParseTimestamp"), ("date.json", "ParseFullDate"), ("time.json", "ParseFullTime") })
		{
			using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Corpus("Timestamps"), file)));

			foreach (var group in document.RootElement.EnumerateArray())
				foreach (var test in group.GetProperty("tests").EnumerateArray())
					if (test.GetProperty("data").ValueKind == JsonValueKind.String)
						corpus.Add((entry, test.GetProperty("data").GetString()!));
		}

		return corpus;
	}

	static string Rendered(FullDate date)
	{
		return string.Create(CultureInfo.InvariantCulture, $"D({date.Year},{date.Month},{date.Day})");
	}

	static string Rendered(FullTime time)
	{
		return string.Create(CultureInfo.InvariantCulture,
			$"T({time.Hour},{time.Minute},{time.Second},{time.Fraction ?? "null"},{(int)time.Offset.TotalMinutes},{(time.LocalOffsetUnknown ? "true" : "false")})");
	}

	/// <summary>
	/// What the shipped parser says of a text, or where it refuses, what the evaluator says: its
	/// guards answered by the shipped parser itself, asked for the date or the time alone.
	/// </summary>
	static SliceAnswer TimestampExpected(string entry, string text)
	{
		var rendered = entry switch
		{
			"ParseTimestamp" => Timestamp.TryParse(text, out var timestamp) ? "TS(" + Rendered(timestamp.Date) + "," + Rendered(timestamp.Time) + ")" : null,
			"ParseFullDate"  => FullDate.TryParse(text, out var date) ? Rendered(date) : null,
			_                => FullTime.TryParse(text, out var time) ? Rendered(time) : null,
		};

		var evaluated = Slices.Evaluated(Timestamps, entry, text, static (fragment, arguments) => fragment.Parameters.Count == 3
			? FullDate.TryParse($"{arguments[0]}-{arguments[1]}-{arguments[2]}", out _)
			: FullTime.TryParse($"{arguments[0]}:{arguments[1]}:{arguments[2]}{arguments[3]}", out _));

		Assert.True(rendered is not null == evaluated.Head.StartsWith('A'), text + ": the evaluator and the shipped parser disagree");

		return new SliceAnswer(rendered is null ? evaluated.Head : "A " + text.Length.ToString(CultureInfo.InvariantCulture) + " " + rendered, []);
	}

	[Theory]
	[InlineData("ParseTimestamp")]
	[InlineData("ParseFullDate")]
	[InlineData("ParseFullTime")]
	public void TypeScriptReadsTimestampsAsTheShippedParser(string entry)
	{
		var corpus = TimestampCorpus().Where(one => one.Entry == entry).Select(static one => one.Text).ToList();

		Assert.True(corpus.Count > 20);

		Same(corpus, [.. corpus.Select(text => TimestampExpected(entry, text))],
			Slices.TypeScript("Timestamps", Timestamps, entry, corpus, host: "rfc3339.ts", frames: SliceFrames.None));

		Same(corpus, [.. corpus.Select(text => TimestampExpected(entry, text))],
			Slices.TypeScript("Timestamps", Timestamps, entry, corpus, host: "rfc3339.ts", frames: SliceFrames.All));
	}

	[Theory]
	[InlineData("ParseTimestamp")]
	[InlineData("ParseFullDate")]
	[InlineData("ParseFullTime")]
	public void CReadsTimestampsAsTheShippedParser(string entry)
	{
		var corpus = TimestampCorpus().Where(one => one.Entry == entry).Select(static one => one.Text).ToList();

		Same(corpus, [.. corpus.Select(text => TimestampExpected(entry, text))],
			Slices.InC("Timestamps", Timestamps, entry, corpus, host: "rfc3339.c"));
	}
}

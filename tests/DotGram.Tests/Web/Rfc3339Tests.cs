using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 3339, held to its own examples and to the JSON Schema test suite's date, time and date-time formats.
/// </summary>
/// <remarks>
/// <para>
/// <c>Timestamps/</c> is json-schema-org/JSON-Schema-Test-Suite's
/// <c>tests/draft2020-12/optional/format/</c> as it stood at 1d82f70, under its own licence beside it.
/// JSON Schema's <c>date</c>, <c>time</c> and <c>date-time</c> are RFC 3339's <c>full-date</c>,
/// <c>full-time</c> and <c>date-time</c>, so a string the suite calls valid has to read and one it calls
/// invalid has to be refused.
/// </para>
/// <para>
/// A case whose data is not a string is about JSON Schema — every string format ignores a number — and
/// not about the RFC, so it is not run.
/// </para>
/// </remarks>
public sealed class Rfc3339Tests
{
	[Theory]
	[MemberData(nameof(Cases))]
	public void Every_string_case_of_the_suite_is_read_as_the_suite_says(string file, int index)
	{
		var test  = Load(file)[index];
		var text  = test.GetProperty("data").GetString()!;
		var valid = test.GetProperty("valid").GetBoolean();

		var read = file switch
		{
			"date-time.json" => Both.TryTimestamp(text, out _),
			"date.json"      => Both.TryFullDate(text, out _),
			_                => Both.TryFullTime(text, out _),
		};

		Assert.True(
			read == valid,
			$"{file}: '{test.GetProperty("description").GetString()}' was {(read ? "read" : "refused")}.");
	}

	// §5.8.
	[Theory]
	[InlineData("1985-04-12T23:20:50.52Z",       "1985-04-12T23:20:50.5200000+00:00")]
	[InlineData("1996-12-19T16:39:57-08:00",     "1996-12-19T16:39:57.0000000-08:00")]
	[InlineData("1937-01-01T12:00:27.87+00:20",  "1937-01-01T12:00:27.8700000+00:20")]
	public void The_RFC_s_examples_are_the_instants_it_says(string text, string instant)
	{
		var timestamp = Both.Timestamp(text);

		Assert.Equal(DateTimeOffset.Parse(instant, System.Globalization.CultureInfo.InvariantCulture), timestamp.ToDateTimeOffset());
		Assert.Equal(text, timestamp.ToString());
	}

	/// <summary>§5.8's leap seconds: read, written back, and refused by a type that cannot hold them.</summary>
	[Theory]
	[InlineData("1990-12-31T23:59:60Z")]
	[InlineData("1990-12-31T15:59:60-08:00")]
	public void A_leap_second_reads_and_has_no_DateTimeOffset(string text)
	{
		var timestamp = Both.Timestamp(text);

		Assert.Equal(60, timestamp.Time.Second);
		Assert.Equal(text, timestamp.ToString());
		Assert.Throws<InvalidOperationException>(() => timestamp.ToDateTimeOffset());
	}

	/// <summary>§4.3: `-00:00` is UTC with the local offset unknown, which `Z` and `+00:00` are not.</summary>
	[Fact]
	public void An_unknown_local_offset_is_told_from_UTC()
	{
		var unknown = Both.FullTime("12:34:56-00:00");
		var utc     = Both.FullTime("12:34:56+00:00");

		Assert.True(unknown.LocalOffsetUnknown);
		Assert.False(utc.LocalOffsetUnknown);
		Assert.Equal(TimeSpan.Zero, unknown.Offset);
		Assert.Equal("12:34:56-00:00", unknown.ToString());
		Assert.Equal("12:34:56Z", utc.ToString());
	}

	/// <summary>A fraction is kept as written, and cut to a tick where .NET is asked for the instant.</summary>
	[Fact]
	public void A_fraction_is_kept_whole()
	{
		var timestamp = Both.Timestamp("1985-04-12T00:59:59.999999999999999Z");

		Assert.Equal("999999999999999", timestamp.Time.Fraction);
		Assert.Equal(9_999_999, timestamp.ToDateTimeOffset().Ticks % TimeSpan.TicksPerSecond);
	}

	public static TheoryData<string, int> Cases
	{
		get
		{
			var cases = new TheoryData<string, int>();

			foreach (var path in Directory.GetFiles(Suite, "*.json").OrderBy(one => one, StringComparer.Ordinal))
			{
				var file = Path.GetFileName(path);
				var all  = Load(file);

				for (var index = 0; index < all.Count; index++)
					if (all[index].GetProperty("data").ValueKind == JsonValueKind.String)
						cases.Add(file, index);
			}

			return cases;
		}
	}

	/// <summary>Every test of every group of a file, in order.</summary>
	static List<JsonElement> Load(string file)
	{
		lock (Loaded)
		{
			if (!Loaded.TryGetValue(file, out var tests))
			{
				using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Suite, file)));

				Loaded[file] = tests =
				[
					.. document.RootElement.EnumerateArray()
						.SelectMany(group => group.GetProperty("tests").EnumerateArray())
						.Select(test => test.Clone()),
				];
			}

			return tests;
		}
	}

	static readonly Dictionary<string, List<JsonElement>> Loaded = new(StringComparer.Ordinal);

	static string Suite =>
		Path.Combine(Path.GetDirectoryName(ThisFile)!, "Timestamps");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}

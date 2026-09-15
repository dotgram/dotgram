using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 9110's media types: the Content-Type and Accept fields, held to the RFC's examples and to every
/// media type the IANA registry holds.
/// </summary>
/// <remarks>
/// There is no shared test suite for these fields, so the material is the RFC — §8.3.1's four spellings
/// of one media type, §12.5.1's examples and its table of quality values, with verified erratum 7138
/// applied to the table — and <c>MediaTypes/*.csv</c>, the IANA Media Types registry as published on
/// 2026-09-15, one file per top-level type, whose every template has to read as a Content-Type.
/// </remarks>
public sealed class Rfc9110Tests
{
	// ── Content-Type ─────────────────────────────────────────────────────────────

	[Fact]
	public void A_media_type_with_a_parameter()
	{
		var type = Rfc9110.ParseContentType("text/html; charset=ISO-8859-4");

		Assert.Equal("text", type.Type);
		Assert.Equal("html", type.Subtype);
		Assert.Equal("ISO-8859-4", type.Charset);
		Assert.Equal("text/html;charset=ISO-8859-4", type.ToString());
	}

	/// <summary>§8.3.1: these four are equivalent.</summary>
	[Fact]
	public void The_four_spellings_of_one_media_type_are_equal()
	{
		var spellings = new[]
		{
			"text/html;charset=utf-8",
			"Text/HTML;Charset=\"utf-8\"",
			"text/html; charset=\"utf-8\"",
			"text/html;charset=UTF-8",
		}
		.Select(Rfc9110.ParseContentType)
		.ToArray();

		foreach (var type in spellings)
		{
			Assert.Equal(spellings[0], type);
			Assert.Equal(spellings[0].GetHashCode(), type.GetHashCode());
		}
	}

	[Fact]
	public void A_value_other_than_a_charset_keeps_its_case()
	{
		Assert.NotEqual(Rfc9110.ParseContentType("multipart/mixed; boundary=ABC"), Rfc9110.ParseContentType("multipart/mixed; boundary=abc"));
	}

	[Fact]
	public void A_quoted_value_is_unquoted_and_written_back_quoted()
	{
		var type = Rfc9110.ParseContentType("multipart/form-data; boundary=\"a b\\\"c\\\\\"");

		Assert.Equal("a b\"c\\", type.ParameterValue("BOUNDARY"));
		Assert.Equal("multipart/form-data;boundary=\"a b\\\"c\\\\\"", type.ToString());
		Assert.Equal(type, Rfc9110.ParseContentType(type.ToString()));
	}

	[Fact]
	public void Empty_parameters_are_accepted_and_left_out()
	{
		var type = Rfc9110.ParseContentType(" text/plain ; ; charset=utf-8;\t; ");

		Assert.Equal([new MediaType.Parameter("charset", "utf-8")], type.Parameters);
	}

	[Fact]
	public void A_suffix_follows_the_last_plus()
	{
		Assert.Equal("json", Rfc9110.ParseContentType("application/vnd.api+json").Suffix);
		Assert.Equal("xml", Rfc9110.ParseContentType("image/svg+xml").Suffix);
		Assert.Null(Rfc9110.ParseContentType("text/plain").Suffix);
	}

	[Theory]
	[InlineData("")]
	[InlineData("text")]
	[InlineData("text/")]
	[InlineData("/html")]
	[InlineData("text/html/5")]
	[InlineData("text /html")]
	[InlineData("text/html; charset = utf-8")]
	[InlineData("text/html; charset=")]
	[InlineData("text/html; charset")]
	[InlineData("text/html; charset=\"utf-8")]
	[InlineData("text/html, text/plain")]
	[InlineData("t\u00EBxt/html")]
	public void What_is_no_media_type(string text)
	{
		Assert.False(Rfc9110.TryParseContentType(text).IsSuccess, $"'{text}' was read.");
	}

	// ── Accept ───────────────────────────────────────────────────────────────────

	[Fact]
	public void The_audio_example()
	{
		var ranges = Rfc9110.ParseAccept("audio/*; q=0.2, audio/basic");

		Assert.Equal(
			[
				new MediaRange(new MediaType("audio", "*", []), 0.2m),
				new MediaRange(new MediaType("audio", "basic", []), 1m),
			],
			ranges);
	}

	[Fact]
	public void The_text_example()
	{
		// The example's line break unfolded to the space a field value carries.
		var ranges = Rfc9110.ParseAccept("text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c");

		Assert.Equal(["text/plain", "text/html", "text/x-dvi", "text/x-c"], ranges.Select(range => range.Media.ToString()).ToArray());
		Assert.Equal([0.5m, 1m, 0.8m, 1m], ranges.Select(range => range.Weight).ToArray());
	}

	/// <summary>§12.5.1's precedence example: more specific ranges override less specific ones.</summary>
	[Fact]
	public void More_specific_ranges_override()
	{
		// The example's ranges, in its order of precedence, each given a weight to tell which one counted.
		var accept = Rfc9110.ParseAccept("*/*;q=0.1, text/*;q=0.2, text/plain;q=0.3, text/plain;format=flowed;q=0.4");

		Assert.Equal(0.4m, Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/plain; format=flowed")));
		Assert.Equal(0.3m, Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/plain")));
		Assert.Equal(0.3m, Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/plain; format=fixed")));
		Assert.Equal(0.2m, Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/html")));
		Assert.Equal(0.1m, Rfc9110.Quality(accept, Rfc9110.ParseContentType("image/png")));
	}

	/// <summary>§12.5.1's table, with erratum 7138: <c>text/html;level=3</c> takes <c>text/*</c>'s 0.3, not 0.7.</summary>
	[Theory]
	[InlineData("text/plain;format=flowed", "1")]
	[InlineData("text/plain", "0.7")]
	[InlineData("text/html", "0.3")]
	[InlineData("image/jpeg", "0.5")]
	[InlineData("text/plain;format=fixed", "0.4")]
	[InlineData("text/html;level=3", "0.3")]
	public void The_quality_table(string media, string quality)
	{
		// The example's line break unfolded to the space a field value carries.
		var accept = Rfc9110.ParseAccept("text/*;q=0.3, text/plain;q=0.7, text/plain;format=flowed, text/plain;format=fixed;q=0.4, */*;q=0.5");

		Assert.Equal(decimal.Parse(quality, System.Globalization.CultureInfo.InvariantCulture), Rfc9110.Quality(accept, Rfc9110.ParseContentType(media)));
	}

	[Fact]
	public void What_no_range_matches_is_not_acceptable()
	{
		Assert.Equal(0m, Rfc9110.Quality(Rfc9110.ParseAccept("text/*"), Rfc9110.ParseContentType("image/png")));
		Assert.Equal(0m, Rfc9110.Quality(Rfc9110.ParseAccept(""), Rfc9110.ParseContentType("image/png")));
	}

	[Fact]
	public void Empty_elements_are_accepted()
	{
		var ranges = Rfc9110.ParseAccept(" , text/html ,, ,\t*/* ;q=0 , ");

		Assert.Equal(2, ranges.Length);
		Assert.Equal(0m, ranges[1].Weight);
	}

	/// <summary>§12.5.1 asks a recipient to take any parameter named q as the weight, wherever it stands.</summary>
	[Fact]
	public void A_q_anywhere_is_the_weight()
	{
		var range = Assert.Single(Rfc9110.ParseAccept("text/html;Q=0.25;level=1"));

		Assert.Equal(0.25m, range.Weight);
		Assert.Equal("text/html;level=1", range.Media.ToString());
	}

	[Theory]
	[InlineData("0", "0")]
	[InlineData("0.", "0")]
	[InlineData("0.5", "0.5")]
	[InlineData("0.125", "0.125")]
	[InlineData("1", "1")]
	[InlineData("1.", "1")]
	[InlineData("1.000", "1")]
	public void A_qvalue(string text, string weight)
	{
		var range = Assert.Single(Rfc9110.ParseAccept("*/*;q=" + text));

		Assert.Equal(decimal.Parse(weight, System.Globalization.CultureInfo.InvariantCulture), range.Weight);
	}

	[Theory]
	[InlineData("*/*;q=1.001")]
	[InlineData("*/*;q=0.1234")]
	[InlineData("*/*;q=2")]
	[InlineData("*/*;q=.5")]
	[InlineData("*/*;q=")]
	[InlineData("*/html")]
	[InlineData("text/html;q=0.5 text/plain")]
	[InlineData("text")]
	public void What_is_no_accept_field(string text)
	{
		Assert.False(Rfc9110.TryParseAccept(text).IsSuccess, $"'{text}' was read.");
	}

	// ── The registry ─────────────────────────────────────────────────────────────

	/// <summary>Every template the IANA registry holds reads as a Content-Type and writes back as written.</summary>
	[Fact]
	public void Every_registered_media_type_reads()
	{
		var seen = 0;

		foreach (var template in Templates())
		{
			var match = Rfc9110.TryParseContentType(template);

			Assert.True(match.IsSuccess, $"'{template}' was not read.");
			Assert.Equal(template, match.Value!.ToString());

			seen++;
		}

		Assert.True(seen > 2000, $"Only {seen} media types were found in the registry.");
	}

	/// <summary>The Template column of each registry file, rows with an empty one left out.</summary>
	static IEnumerable<string> Templates()
	{
		foreach (var file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(ThisFile)!, "MediaTypes"), "*.csv"))
			foreach (var line in File.ReadLines(file).Skip(1))
				if (Columns(line) is [_, { Length: > 0 } template, ..])
					yield return template;
	}

	/// <summary>A CSV line's fields, a quoted one unquoted.</summary>
	static List<string> Columns(string line)
	{
		var columns = new List<string>();
		var current = new System.Text.StringBuilder();
		var quoted  = false;

		for (var at = 0; at < line.Length; at++)
		{
			var c = line[at];

			if (quoted)
			{
				if (c != '"')
					current.Append(c);
				else if (at + 1 < line.Length && line[at + 1] == '"')
					current.Append(line[++at]);
				else
					quoted = false;
			}
			else if (c == '"')
				quoted = true;
			else if (c == ',')
			{
				columns.Add(current.ToString());
				current.Clear();
			}
			else
				current.Append(c);
		}

		columns.Add(current.ToString());

		return columns;
	}

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}

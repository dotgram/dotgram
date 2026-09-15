using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 5646, held to its own examples and to every tag and subtag the IANA registry holds.
/// </summary>
/// <remarks>
/// <para>
/// There is no test suite for BCP 47 the way there is for URI Templates, so the material is the
/// two things a language tag answers to. Appendix A's examples, the well-formed and the ones it
/// calls invalid. And <c>LanguageTags/language-subtag-registry.txt</c>, the IANA Language Subtag
/// Registry as it stood on 2026-08-08: every grandfathered and redundant tag in it, every prefix a
/// subtag names, and every subtag placed where its type puts it.
/// </para>
/// <para>
/// The registry writes each tag in the case §2.1.1 recommends, so reading one and writing it back
/// must give it unchanged — which holds the reading and the formatting against the same text.
/// </para>
/// </remarks>
public sealed class Rfc5646Tests
{
	[Theory]
	[InlineData("de",                     "de")]
	[InlineData("fr",                     "fr")]
	[InlineData("ja",                     "ja")]
	[InlineData("i-enochian",             "i-enochian")]
	[InlineData("zh-Hant",                "zh-Hant")]
	[InlineData("zh-Hans",                "zh-Hans")]
	[InlineData("sr-Cyrl",                "sr-Cyrl")]
	[InlineData("sr-Latn",                "sr-Latn")]
	[InlineData("zh-cmn-Hans-CN",         "zh-cmn-Hans-CN")]
	[InlineData("cmn-Hans-CN",            "cmn-Hans-CN")]
	[InlineData("zh-yue-HK",              "zh-yue-HK")]
	[InlineData("yue-HK",                 "yue-HK")]
	[InlineData("zh-Hans-CN",             "zh-Hans-CN")]
	[InlineData("sr-Latn-RS",             "sr-Latn-RS")]
	[InlineData("sl-rozaj",               "sl-rozaj")]
	[InlineData("sl-rozaj-biske",         "sl-rozaj-biske")]
	[InlineData("sl-nedis",               "sl-nedis")]
	[InlineData("de-CH-1901",             "de-CH-1901")]
	[InlineData("sl-IT-nedis",            "sl-IT-nedis")]
	[InlineData("hy-Latn-IT-arevela",     "hy-Latn-IT-arevela")]
	[InlineData("de-DE",                  "de-DE")]
	[InlineData("en-US",                  "en-US")]
	[InlineData("es-419",                 "es-419")]
	[InlineData("de-CH-x-phonebk",        "de-CH-x-phonebk")]
	[InlineData("az-Arab-x-AZE-derbend",  "az-Arab-x-aze-derbend")]
	[InlineData("x-whatever",             "x-whatever")]
	[InlineData("qaa-Qaaa-QM-x-southern", "qaa-Qaaa-QM-x-southern")]
	[InlineData("de-Qaaa",                "de-Qaaa")]
	[InlineData("sr-Latn-QM",             "sr-Latn-QM")]
	[InlineData("sr-Qaaa-RS",             "sr-Qaaa-RS")]
	[InlineData("en-US-u-islamcal",       "en-US-u-islamcal")]
	[InlineData("zh-CN-a-myext-x-private","zh-CN-a-myext-x-private")]
	[InlineData("en-a-myext-b-another",   "en-a-myext-b-another")]
	// Appendix A calls it invalid — two extensions under one singleton — and it is well-formed.
	[InlineData("ar-a-aaa-b-bbb-a-ccc",   "ar-a-aaa-b-bbb-a-ccc")]
	// A variant beginning with a digit is four characters, and `123a` is one.
	[InlineData("en-123a",                "en-123a")]
	public void The_RFC_s_examples_are_well_formed(string tag, string formatted)
	{
		var accepted = LanguageTag.TryParse(tag, out var parsed);

		Assert.True(accepted, $"'{tag}' was refused.");
		Assert.Equal(formatted, parsed!.ToString());
	}

	/// <summary>What the ABNF does not make, the RFC's own two among them.</summary>
	[Theory]
	[InlineData("de-419-DE")]            // two regions
	[InlineData("a-DE")]                 // a singleton where the language stands
	[InlineData("")]
	[InlineData("en-")]
	[InlineData("-en")]
	[InlineData("en--US")]
	[InlineData("en_US")]
	[InlineData("en US")]
	[InlineData("abcdefghi")]            // a language of nine letters
	[InlineData("en-US-abcdefghi")]      // a variant of nine
	[InlineData("en-a")]                 // a singleton with nothing after it
	[InlineData("en-x")]                 // private use with nothing after it
	[InlineData("en-a-b")]               // an extension subtag of one character
	[InlineData("zh-cmn-yue-wuu-min")]   // four extended languages
	[InlineData("en-Latn-Latn")]         // two scripts
	[InlineData("en-12a")]               // neither a region of three digits nor a variant of four characters
	[InlineData("i-klingonx")]           // a grandfathered tag is one only where the tag ends
	[InlineData("x")]
	[InlineData("12-US")]
	public void What_is_not_well_formed_is_refused(string tag) =>
		Assert.False(LanguageTag.TryParse(tag, out _), $"'{tag}' was read.");

	[Fact]
	public void A_tag_comes_apart_into_its_subtags()
	{
		var tag = LanguageTag.Parse("zh-cmn-Hans-CN-pinyin-u-ca-chinese-x-private");

		Assert.Equal("zh", tag.Language);
		Assert.Equal(["cmn"], tag.ExtendedLanguages);
		Assert.Equal("Hans", tag.Script);
		Assert.Equal("CN", tag.Region);
		Assert.Equal(["pinyin"], tag.Variants);
		Assert.Equal('u', Assert.Single(tag.Extensions).Singleton);
		Assert.Equal(["ca", "chinese"], tag.Extensions[0].Subtags);
		Assert.Equal(["private"], tag.PrivateUse);
		Assert.Null(tag.Grandfathered);
	}

	/// <summary>Case carries no meaning (§2.1.1): parts are kept as written and formatted as recommended.</summary>
	[Fact]
	public void Case_is_kept_and_carries_no_meaning()
	{
		var tag = LanguageTag.Parse("MN-cYRL-mn");

		Assert.Equal(("MN", "cYRL", "mn"), (tag.Language, tag.Script, tag.Region));
		Assert.Equal("mn-Cyrl-MN", tag.ToString());
	}

	/// <summary>A grandfathered tag is whole, and one that only begins like one is an ordinary tag.</summary>
	[Fact]
	public void A_grandfathered_tag_is_one_only_when_it_is_the_whole_tag()
	{
		Assert.Equal("zh-min-nan", LanguageTag.Parse("ZH-MIN-NAN").Grandfathered?.ToLowerInvariant());
		Assert.Equal("zh-min", LanguageTag.Parse("zh-min").Grandfathered);

		var ordinary = LanguageTag.Parse("zh-min-xyz");

		Assert.Null(ordinary.Grandfathered);
		Assert.Equal(["min", "xyz"], ordinary.ExtendedLanguages);
	}

	/// <summary>Tags are equal whatever their case, since case carries no meaning in one (§2.1.1).</summary>
	[Fact]
	public void Tags_are_equal_whatever_their_case()
	{
		Assert.Equal(LanguageTag.Parse("zh-Hant-TW-u-ca-chinese-x-a"), LanguageTag.Parse("ZH-hant-tw-U-CA-Chinese-X-A"));
		Assert.Equal(LanguageTag.Parse("en-US").GetHashCode(), LanguageTag.Parse("EN-us").GetHashCode());
		Assert.Equal(LanguageTag.Parse("i-klingon"), LanguageTag.Parse("I-KLINGON"));

		Assert.NotEqual(LanguageTag.Parse("en-US"), LanguageTag.Parse("en-GB"));
		Assert.NotEqual(LanguageTag.Parse("sl-rozaj-biske"), LanguageTag.Parse("sl-biske-rozaj"));
	}

	// ── The registry ─────────────────────────────────────────────────────────────

	/// <summary>Every tag the registry holds reads, and writes back as the registry spells it.</summary>
	[Fact]
	public void Every_registered_tag_is_well_formed_and_written_as_registered()
	{
		var failures = new List<string>();
		var tags     = Registry().Where(record => record.ContainsKey("Tag")).ToList();

		Assert.Equal(26 + 67, tags.Count);

		foreach (var record in tags)
		{
			var written       = record["Tag"][0];
			var grandfathered = record["Type"][0] == "grandfathered";

			foreach (var spelling in new[] { written, written.ToLowerInvariant(), written.ToUpperInvariant() })
			{
				var accepted = LanguageTag.TryParse(spelling, out var parsed);

				if (!accepted)
					failures.Add($"'{spelling}' was refused");
				else if (parsed!.ToString() != written)
					failures.Add($"'{spelling}' was written '{parsed}'");
				else if ((parsed!.Grandfathered is not null) != grandfathered)
					failures.Add($"'{spelling}' was {(grandfathered ? "not " : "")}read as grandfathered");
			}
		}

		Assert.Empty(failures);
	}

	/// <summary>Every subtag stands where its type puts it, after the prefix the registry gives it.</summary>
	[Fact]
	public void Every_registered_subtag_stands_where_its_type_puts_it()
	{
		var failures = new List<string>();
		var seen     = 0;

		foreach (var record in Registry().Where(record => record.ContainsKey("Subtag")))
		{
			var subtag = record["Subtag"][0];

			// A range of private-use subtags, `qaa..qtz`, is not one subtag.
			if (subtag.Contains(".."))
				continue;

			var prefixes = record.TryGetValue("Prefix", out var given) ? given : ["und"];

			foreach (var prefix in prefixes)
			{
				var (tag, part) = record["Type"][0] switch
				{
					"language" => (subtag,                     (Func<LanguageTag, string?>)(one => one.Language)),
					"extlang"  => ($"{prefix}-{subtag}",       one => one.ExtendedLanguages.LastOrDefault()),
					"script"   => ($"und-{subtag}",            one => one.Script),
					"region"   => ($"und-{subtag}",            one => one.Region),
					"variant"  => ($"{prefix}-{subtag}",       one => one.Variants.LastOrDefault()),
					var other  => throw new InvalidOperationException($"A subtag of a type nobody told this about: {other}."),
				};

				seen++;

				var accepted = LanguageTag.TryParse(tag, out var parsed);

				if (!accepted)
					failures.Add($"'{tag}' was refused");
				else if (part(parsed!) != subtag)
					failures.Add($"'{tag}' did not read '{subtag}' as a {record["Type"][0]}");
				else if (parsed!.ToString() != tag)
					failures.Add($"'{tag}' was written '{parsed}'");
			}
		}

		Assert.True(seen > 9000, $"Only {seen} subtags were found in the registry.");
		Assert.Empty(failures);
	}

	/// <summary>The registry's records, each field's values in the order written (RFC 5646 §3.1.1).</summary>
	static List<Dictionary<string, List<string>>> Registry()
	{
		var records = new List<Dictionary<string, List<string>>>();
		var current = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		foreach (var line in File.ReadLines(Path.Combine(Path.GetDirectoryName(ThisFile)!, "LanguageTags", "language-subtag-registry.txt")))
		{
			if (line == "%%")
			{
				if (current.Count > 0)
					records.Add(current);

				current = new Dictionary<string, List<string>>(StringComparer.Ordinal);
				continue;
			}

			// A line beginning with a space continues the field before it, which nothing here reads.
			if (line.Length == 0 || line[0] == ' ' || line.IndexOf(": ", StringComparison.Ordinal) is var colon && colon < 0)
				continue;

			var name = line.Substring(0, colon);

			if (!current.TryGetValue(name, out var values))
				current[name] = values = [];

			values.Add(line.Substring(colon + 2));
		}

		if (current.Count > 0)
			records.Add(current);

		return records;
	}

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
